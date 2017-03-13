using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawnRune : MonoBehaviour
{
    public void Cast()
    {
        iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.one * 4, "time", 0.5f, "easetype", "easeOutCubic", "oncomplete", "ScaleBackDown", "oncompletetarget", gameObject));
    }
	
    void ScaleBackDown()
    {
        iTween.ScaleTo(gameObject, iTween.Hash("scale", Vector3.one, "time", 1.25f, "easetype", "easeOutCubic"));
    }
}
