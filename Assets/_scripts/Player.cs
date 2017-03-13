using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using TouchScript.Gestures;
using TouchScript.Utils;

using PDollarGestureRecognizer;

public class Player : Caster
{
    public float runeSuccessScore = 0.88f;

    private List<PDollarGestureRecognizer.Gesture> trainingSet = new List<PDollarGestureRecognizer.Gesture>();

    private Vector3 virtualKeyPosition = Vector2.zero;
    private Rect drawArea;

    private RuntimePlatform platform;

    private List<LineRenderer> gestureLinesRenderer = new List<LineRenderer>();
    private LineRenderer currentGestureLineRenderer;

    public ScreenTransformGesture OneFingerMoveGesture;
    public FlickGesture twoFingerFlickGesture;

    private bool recognized;

    void Start()
    {
        platform = Application.platform;
        drawArea = new Rect(0, 0, Screen.width - Screen.width / 3, Screen.height);

        //Load pre-made gestures
        TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/runes/");
        foreach (TextAsset gestureXml in gesturesXml)
            trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));

        //Load user custom gestures
        //string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.xml");
        //foreach (string filePath in filePaths)
        //    trainingSet.Add(GestureIO.ReadGestureFromFile(filePath));
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (!recognized)
            {
                recognized = true;

                PDollarGestureRecognizer.Gesture candidate = new PDollarGestureRecognizer.Gesture(points.ToArray());
                Result gestureResult = PointCloudRecognizer.Classify(candidate, trainingSet.ToArray());

                if (gestureResult.Score > runeSuccessScore)
                    RuneSuccess(gestureResult);
                else
                    RuneFailure();
            }
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            RuneFailure();
        }
    }
#endif

    private void OnEnable()
    {
        OneFingerMoveGesture.TransformStarted += OneFingerDown;
        OneFingerMoveGesture.Transformed += OneFingerMoved;
        OneFingerMoveGesture.TransformCompleted += OneFingerEnded;

        twoFingerFlickGesture.Flicked += FlickOccured;        
    }

    private void OnDisable()
    {
        OneFingerMoveGesture.TransformStarted -= OneFingerDown;
        OneFingerMoveGesture.Transformed -= OneFingerMoved;
        OneFingerMoveGesture.TransformCompleted -= OneFingerEnded;

        twoFingerFlickGesture.Flicked -= FlickOccured;
    }

    public override void OneFingerDown(object sender, System.EventArgs e)
    {
        if (myState != CasterState.ready)
            return;

        virtualKeyPosition = OneFingerMoveGesture.ScreenPosition;

        if (drawArea.Contains(virtualKeyPosition))
        {
            if (recognized)
                ClearRune();

            ++strokeId;

            Transform tmpGesture = Instantiate(gestureOnScreenPrefab, transform.position, transform.rotation) as Transform;
            currentGestureLineRenderer = tmpGesture.GetComponent<LineRenderer>();
            gestureLinesRenderer.Add(currentGestureLineRenderer);

            vertexCount = 0;

            Network.Instance.pView.RPC("StartedDrawing", PhotonTargets.Others);
        }
    }

    public override void OneFingerMoved(object sender, System.EventArgs e)
    {
        virtualKeyPosition = OneFingerMoveGesture.ScreenPosition;

        if (strokeId > -1)
        {
            points.Add(new Point(virtualKeyPosition.x, -virtualKeyPosition.y, strokeId));

            currentGestureLineRenderer.numPositions = ++vertexCount;
            currentGestureLineRenderer.SetPosition(vertexCount - 1, Camera.main.ScreenToWorldPoint(new Vector3(virtualKeyPosition.x, virtualKeyPosition.y, 10)));

            Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(virtualKeyPosition.x, virtualKeyPosition.y, 10));
            MoveTrailRenderer(pos);

            Network.Instance.pView.RPC("FingerMoved", PhotonTargets.Others, pos);
        }
    }

    public override void OneFingerEnded(object sender, System.EventArgs e)
    {
        virtualKeyPosition = Vector2.zero;
    }

    public override void FlickOccured(object sender, System.EventArgs e)
    {
        bool cast = twoFingerFlickGesture.ScreenFlickVector.x > 0f;
        if (cast && !recognized)
        {
            recognized = true;

            PDollarGestureRecognizer.Gesture candidate = new PDollarGestureRecognizer.Gesture(points.ToArray());
            Result gestureResult = PointCloudRecognizer.Classify(candidate, trainingSet.ToArray());

            if (gestureResult.Score > runeSuccessScore)
                RuneSuccess(gestureResult);
            else
                RuneFailure();
        }
        else if (!cast)
            RuneFailure();
    }

    void RuneSuccess(Result runeResult)
    {
        Rune rune = RuneManager.Instance.CloneRuneByName(runeResult.GestureClass);
        float power = Mathf.InverseLerp(runeSuccessScore, 1f, runeResult.Score);
        rune.power = power;

        if (PhotonNetwork.isMasterClient)
        {
            RuneManager.Instance.StartRune(rune, this);

            foreach (LineRenderer lr in gestureLinesRenderer)
                lr.GetComponent<DrawnRune>().Cast();
        }
        else
        {
            Debug.Log(Time.timeSinceLevelLoad + " - requesting to start rune");
            // we are NOT the host, we must send our actions through the host's machine to avoid any out of sync issues
            Network.Instance.pView.RPC("WantsToStartRune", PhotonTargets.MasterClient, rune.runeName, power);
        }
    }

    public void HostCreatedRune(Rune rune)
    {
        activeRune = rune;
        anim.Play("Summon");

        foreach (LineRenderer lr in gestureLinesRenderer)
            lr.GetComponent<DrawnRune>().Cast();
    }

    void RuneFailure()
    {
        ClearRune();
    }

    void ClearRune()
    {
        activeRune = null;
        recognized = false;
        strokeId = -1;
        points.Clear();

        foreach (LineRenderer lineRenderer in gestureLinesRenderer)
        {
            lineRenderer.numPositions = 0;
            Destroy(lineRenderer.gameObject);
        }

        gestureLinesRenderer.Clear();

        Network.Instance.pView.RPC("ClearRune", PhotonTargets.Others);
    }

    public override void MyRuneWasCounteredCallback()
    {
        base.MyRuneWasCounteredCallback();
    }

    public override void RuneCastCallback()
    {
        ClearRune();
    }

    public override void HideCurrentRuneCallback()
    {
        base.HideCurrentRuneCallback();
        ClearRune();
    }

    public override void MoveTrailRenderer(Vector3 position)
    {
        trailRenderer.transform.position = position;
    }
}
