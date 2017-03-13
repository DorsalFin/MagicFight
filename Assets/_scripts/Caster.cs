using PDollarGestureRecognizer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CasterState
{
    ready = 0,
    casting = 1,
    stunned = 2,
    dead = 3
}

public class Caster : Photon.PunBehaviour
{
    public PhotonView pView;

    public string myName;
    public CasterState myState = CasterState.ready;

    public int totalHealth;
    public EnergyBar healthBar;

    public Transform gestureOnScreenPrefab;

    public GameObject trailRenderer;
    protected List<Point> points = new List<Point>();
    protected int strokeId = -1;
    protected int vertexCount = 0;

    public Animator anim;

    //public GameObject runeTimeRoot;
    //public Image runeTimeFillImage;

    public float counterStunnedTime = 1f;
    public Image myRuneWasblockedImage;

    [HideInInspector]
    public Rune activeRune;

    int _currentHealth;

    void Awake()
    {
        _currentHealth = totalHealth;
        healthBar.valueMax = totalHealth;
        healthBar.valueCurrent = totalHealth;
    }

    public virtual void OneFingerDown(object sender, System.EventArgs e) { }

    public virtual void OneFingerMoved(object sender, System.EventArgs e) { }

    public virtual void OneFingerEnded(object sender, System.EventArgs e) { }

    public virtual void FlickOccured(object sender, System.EventArgs e) { }

    public virtual void RuneCastCallback() { }

    public virtual void MyRuneWasCounteredCallback()
    {
        // do something here if something extra should happen when ebing countered
        myState = CasterState.stunned;
        anim.Play("Block");
        myRuneWasblockedImage.gameObject.SetActive(true);
        CancelInvoke("EndStun");
        Invoke("EndStun", 1f);

        HideCurrentRuneCallback();
    }

    void EndStun()
    {
        myState = CasterState.ready;
        myRuneWasblockedImage.gameObject.SetActive(false);
    }

    public virtual void HideCurrentRuneCallback()
    {
        activeRune = null;
    }

    public virtual void HitByRune(Rune rune)
    {
        int scaledDmg = Mathf.RoundToInt((float)rune.baseDamage * rune.power);
        _currentHealth = Mathf.Clamp(_currentHealth - scaledDmg, 0, totalHealth);
        healthBar.SetValueCurrent(_currentHealth);

        if (_currentHealth == 0)
        {
            myState = CasterState.dead;
            anim.Play("Die");
        }
        else
            anim.Play("Get_Hit_Front");
    }

    public virtual void MoveTrailRenderer(Vector3 position) { }
}
