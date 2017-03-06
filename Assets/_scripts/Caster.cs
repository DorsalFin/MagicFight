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

public class Caster : MonoBehaviour
{
    public string myName;
    public CasterState myState = CasterState.ready;

    public float totalHealth;

    public GameObject myHealthBarRoot;
    public Image myHealthBarFillImage;

    public Animator anim;

    //public GameObject runeTimeRoot;
    //public Image runeTimeFillImage;

    public float counterStunnedTime = 1f;
    public Image myRuneWasblockedImage;

    [Header("If you want rune name to show when cast")]
    public Text recognizedRuneText;

    [HideInInspector]
    public Rune activeRune;

    float _currentHealth;

    void Awake()
    {
        _currentHealth = totalHealth;
        myHealthBarFillImage.fillAmount = 1f;
        //runeTimeFillImage.fillAmount = 0f;
        //runeTimeRoot.SetActive(false);
    }

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
        //runeTimeRoot.SetActive(false);
        activeRune = null;
    }

    public virtual void HitByRune(Rune rune)
    {
        //float scaledDamage = rune.damage 
        _currentHealth = Mathf.Clamp(_currentHealth - rune.damage, 0f, totalHealth);
        myHealthBarFillImage.fillAmount = _currentHealth / totalHealth;

        if (_currentHealth == 0)
        {
            myState = CasterState.dead;
            anim.Play("Die");
        }
        else
            anim.Play("Get_Hit_Front");
    }
}
