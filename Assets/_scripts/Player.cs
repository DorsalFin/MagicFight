using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using PDollarGestureRecognizer;

public class Player : Caster
{
    public Transform gestureOnScreenPrefab;
    public GameObject trailRenderer;

    public float runeSuccessScore = 0.88f;

    private List<Gesture> trainingSet = new List<Gesture>();

    private List<Point> points = new List<Point>();
    private int strokeId = -1;

    private Vector3 virtualKeyPosition = Vector2.zero;
    private Rect drawArea;

    private RuntimePlatform platform;
    private int vertexCount = 0;

    private List<LineRenderer> gestureLinesRenderer = new List<LineRenderer>();
    private LineRenderer currentGestureLineRenderer;

    //GUI
    private string message;
    private bool recognized;

    void Start()
    {
        platform = Application.platform;
        drawArea = new Rect(0, 0, Screen.width - Screen.width / 3, Screen.height);

        ////Load pre-made gestures
        //TextAsset[] gesturesXml = Resources.LoadAll<TextAsset>("GestureSet/10-stylus-MEDIUM/");
        //foreach (TextAsset gestureXml in gesturesXml)
        //    trainingSet.Add(GestureIO.ReadGestureFromXML(gestureXml.text));

        //Load user custom gestures
        string[] filePaths = Directory.GetFiles(Application.persistentDataPath, "*.xml");
        foreach (string filePath in filePaths)
            trainingSet.Add(GestureIO.ReadGestureFromFile(filePath));
    }

    void Update()
    {
        if (myState != CasterState.ready)
        {
            // TODO: would i ever have to cancel an in progress rune here?
            return;
        }

        if (platform == RuntimePlatform.Android || platform == RuntimePlatform.IPhonePlayer)
        {
            if (Input.touchCount > 0)
            {
                virtualKeyPosition = new Vector3(Input.GetTouch(0).position.x, Input.GetTouch(0).position.y);
            }
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                virtualKeyPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y);
            }
        }

        if (drawArea.Contains(virtualKeyPosition))
        {

            if (Input.GetMouseButtonDown(0))
            {
                if (recognized)
                    ClearRune();

                ++strokeId;

                Transform tmpGesture = Instantiate(gestureOnScreenPrefab, transform.position, transform.rotation) as Transform;
                currentGestureLineRenderer = tmpGesture.GetComponent<LineRenderer>();
                currentGestureLineRenderer.startColor = Color.blue;
                currentGestureLineRenderer.endColor = Color.magenta;

                gestureLinesRenderer.Add(currentGestureLineRenderer);

                vertexCount = 0;
            }

            if (strokeId > -1)
            {
                if (Input.GetMouseButton(0))
                {
                    points.Add(new Point(virtualKeyPosition.x, -virtualKeyPosition.y, strokeId));

                    currentGestureLineRenderer.numPositions = ++vertexCount;
                    currentGestureLineRenderer.SetPosition(vertexCount - 1, Camera.main.ScreenToWorldPoint(new Vector3(virtualKeyPosition.x, virtualKeyPosition.y, 10)));

                    Vector2 pos = Camera.main.ScreenToWorldPoint(new Vector3(virtualKeyPosition.x, virtualKeyPosition.y, 10));
                    trailRenderer.transform.position = pos;
                }

                if (Input.GetMouseButtonUp(0) && !recognized)
                {
                    recognized = true;

                    Gesture candidate = new Gesture(points.ToArray());
                    Result gestureResult = PointCloudRecognizer.Classify(candidate, trainingSet.ToArray());

                    if (gestureResult.Score > runeSuccessScore)
                        RuneSuccess(gestureResult);
                    else
                        RuneFailure(gestureResult);
                }
            }
        }
    }

    void RuneSuccess(Result runeResult)
    {
        if (recognizedRuneText)
            recognizedRuneText.text = runeResult.GestureClass;

        Rune rune = RuneManager.Instance.GetRuneByName(runeResult.GestureClass);
        RuneManager.Instance.StartRune(rune, this);
    }

    void RuneFailure(Result runeResult)
    {
        if (recognizedRuneText)
            recognizedRuneText.text = "the rune fades to nothing...";
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

        if (recognizedRuneText)
            recognizedRuneText.text = "";
        //runeTimeRoot.SetActive(false);
        gestureLinesRenderer.Clear();
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
}
