using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Rune
{
    public string runeName;
    public string counterRuneName;
    public Sprite sprite;
    public int baseDamage;

    [HideInInspector]
    public float power;

    public Rune Clone()
    {
        Rune clonedRune = new Rune();
        clonedRune.runeName = runeName;
        clonedRune.counterRuneName = counterRuneName;
        clonedRune.sprite = sprite;
        clonedRune.baseDamage = baseDamage;
        return clonedRune;
    }
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
        if (PhotonNetwork.inRoom && PhotonNetwork.isMasterClient)
        {
            if (_countingLeft)
            {
                _timerLeft += Time.deltaTime;

                if (_timerLeft > runeTime)
                    CastRune(leftCaster);
            }

            if (_countingRight)
            {
                _timerRight += Time.deltaTime;

                if (_timerRight > runeTime)
                    CastRune(rightCaster);
            }
        }
    }

    public Rune CloneRuneByName(string runeName)
    {
        foreach (Rune rune in runes)
        {
            if (rune.runeName == runeName)
            {
                // TODO new copy of rune
                return rune.Clone();
            }
        }

        return null;
    }

    public Rune CloneRandomRune()
    {
        int idx = Random.Range(0, runes.Count);
        return runes[idx].Clone();
    }

    /// <summary>
    /// a player has swiped right and 'created' their rune. only the host will receive this call and should push the result
    /// to all clients
    /// </summary>
    /// <param name="rune"></param>
    /// <param name="caster"></param>
    public void StartRune(Rune rune, Caster caster)
    {
        caster.activeRune = rune;
        caster.anim.Play("Summon");

        Caster otherCaster = OtherCaster(caster);

        // if the other caster has a rune on screen and this is the counter
        if (otherCaster.activeRune != null && otherCaster.activeRune.counterRuneName == rune.runeName)
        {
            _countingRight = false;
            _countingLeft = false;

            caster.HideCurrentRuneCallback();
            otherCaster.MyRuneWasCounteredCallback();

            // tell the client a rune was countered
            Network.Instance.pView.RPC("RuneCountered", PhotonTargets.Others, otherCaster == leftCaster);
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

            // tell the client this rune was successfully cast
            Network.Instance.pView.RPC("RuneSuccessfullyCreated", PhotonTargets.Others, rune.runeName, rune.power, caster == leftCaster);

            // AI chance to counter your rune
            if (otherCaster is AIOpponent && otherCaster.activeRune == null)
            {
                AIOpponent ai = otherCaster as AIOpponent;
                bool countering = ai.ChanceToCounterRune(rune);
                if (countering)
                {
                    _timerRight = 0f;
                    _countingRight = false;
                }
            }
        }
    }

    public void CastRune(Caster caster)
    {
        if (PhotonNetwork.isMasterClient)
        {
            _countingLeft = false;
            _timerLeft = 0f;
            _countingRight = false;
            _timerRight = 0f;

            // need to notify the client a rune has been successfully cast
            Network.Instance.pView.RPC("RuneWasCast", PhotonTargets.Others, caster == leftCaster);
        }

        Caster otherCaster = OtherCaster(caster);
        if (otherCaster.activeRune != null)
            otherCaster.MyRuneWasCounteredCallback();

        otherCaster.HitByRune(caster.activeRune);
        caster.RuneCastCallback();
    }

    Caster OtherCaster(Caster caster)
    {
        if (caster == leftCaster)
            return rightCaster;
        else
            return leftCaster;
    }

    public void CancelRightRune()
    {
        _countingRight = false;
        _timerRight = 0f;
    }
}
