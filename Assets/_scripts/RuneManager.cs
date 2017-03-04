using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Rune
{
    public string runeName;
    public string counterRuneName;
    public Sprite sprite;
    public float damage;
}

public class RuneManager : MonoBehaviour
{
    public static RuneManager Instance;

    public List<Rune> runes = new List<Rune>();

    public float runeTime;

    public Caster leftCaster;
    public Caster rightCaster;

    bool _countingLeft;
    bool _countingRight;
    float _timerLeft;
    float _timerRight;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (_countingLeft)
        {
            _timerLeft += Time.deltaTime;

            leftCaster.runeTimeFillImage.fillAmount = _timerLeft / runeTime;

            if (_timerLeft > runeTime)
                CastRune(leftCaster);
        }

        if (_countingRight)
        {
            _timerRight += Time.deltaTime;

            rightCaster.runeTimeFillImage.fillAmount = _timerRight / runeTime;

            if (_timerRight > runeTime)
                CastRune(rightCaster);
        }
    }

    public Rune GetRuneByName(string runeName)
    {
        foreach (Rune rune in runes)
            if (rune.runeName == runeName)
                return rune;

        return null;
    }

    public Rune GetRandomRune()
    {
        int idx = Random.Range(0, runes.Count);
        return runes[idx];
    }

    public void StartRune(Rune rune, Caster caster)
    {
        caster.activeRune = rune;
        caster.runeTimeFillImage.fillAmount = 0f;
        caster.runeTimeRoot.SetActive(true);

        Caster otherCaster = OtherCaster(caster);

        // if the other caster has a rune on screen and this is the counter
        if (otherCaster.activeRune != null && otherCaster.activeRune.counterRuneName == rune.runeName)
        {
            _countingRight = false;
            _countingLeft = false;

            otherCaster.RuneCounteredCallback();
            caster.HideCurrentRuneCallback();
        }
        else
        {
            if (caster == rightCaster)
            {
                _timerRight = 0f;
                _countingRight = true;
            }
            else
            {
                _timerLeft = 0f;
                _countingLeft = true;
            }
        }
    }

    void CastRune(Caster caster)
    {
        // BOOM we hit for the rune's damage

        Caster otherCaster = OtherCaster(caster);

        otherCaster.HitByRune(caster.activeRune);

        caster.RuneCastCallback();

        if (caster == leftCaster)
        {
            _countingLeft = false;
            _timerLeft = 0f;
        }
        else
        {
            _countingRight = false;
            _timerRight = 0f;
        }
    }

    Caster OtherCaster(Caster caster)
    {
        if (caster == leftCaster)
            return rightCaster;
        else
            return leftCaster;
    }
}
