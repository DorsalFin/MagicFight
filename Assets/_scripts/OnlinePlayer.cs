using PDollarGestureRecognizer;
using System;
using System.Collections.Generic;
using UnityEngine;

public class OnlinePlayer : Caster
{
    public Transform gestureTransform;

    List<LineRenderer> gestureLinesRenderer = new List<LineRenderer>();
    LineRenderer currentGestureLineRenderer;


    public void StartedDrawing()
    {
        ++strokeId;
        vertexCount = 0;

        Transform tmpGesture = Instantiate(gestureOnScreenPrefab, transform.position, transform.rotation) as Transform;
        currentGestureLineRenderer = tmpGesture.GetComponent<LineRenderer>();
        currentGestureLineRenderer.widthMultiplier = 0.1f;
        gestureLinesRenderer.Add(currentGestureLineRenderer);
    }

    public override void MoveTrailRenderer(Vector3 position)
    {
        position = position * 0.1f;
        trailRenderer.transform.localPosition = position;

        if (strokeId > -1)
        {
            Vector3 worldPos = gestureTransform.localToWorldMatrix.MultiplyPoint(position);

            points.Add(new Point(worldPos.x, -worldPos.y, strokeId));

            currentGestureLineRenderer.numPositions = ++vertexCount;
            currentGestureLineRenderer.SetPosition(vertexCount - 1, worldPos);
        }
    }

    public void AppliedRune(Rune rune)
    {
        activeRune = rune;
        anim.Play("Summon");

        foreach (LineRenderer lr in gestureLinesRenderer)
            lr.GetComponent<DrawnRune>().Cast();
    }

    public void ClearRune()
    {
        activeRune = null;
        strokeId = -1;

        points.Clear();

        foreach (LineRenderer lineRenderer in gestureLinesRenderer)
        {

            lineRenderer.numPositions = 0;
            Destroy(lineRenderer.gameObject);
        }

        gestureLinesRenderer.Clear();
    }

    public override void HideCurrentRuneCallback()
    {
        base.HideCurrentRuneCallback();
    }

    public override void MyRuneWasCounteredCallback()
    {
        base.MyRuneWasCounteredCallback();
    }

    public override void HitByRune(Rune rune)
    {
        base.HitByRune(rune);
    }

    public override void RuneCastCallback()
    {
        base.RuneCastCallback();
    }

    
}
