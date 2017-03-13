using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AIOpponent : Caster
{
    public float actionTime;

    [Range(1,100)]
    public float chanceForAction;

    [Range(1,100)]
    public float chanceToCounter;

    [Range(0.1f,1f)]
    public float runeStrength;

    public Image runeImage;

    float _timer;

    void Update()
    {
        if (myState == CasterState.ready)
        {
            _timer += Time.deltaTime;

            if (_timer > actionTime)
                ActionTimeUp();
        }
    }

    void ActionTimeUp()
    {
        bool willCastRune = Random.Range(0, 100) < chanceForAction;
        if (willCastRune)
            CastRune();
        else
            _timer = 0f;
    }

    void CastRune(string runeName = "")
    {
        myState = CasterState.casting;
        Rune rune = runeName == "" ? RuneManager.Instance.CloneRandomRune() : RuneManager.Instance.CloneRuneByName(runeName);
        rune.power = runeStrength;
        runeImage.sprite = rune.sprite;
        runeImage.SetNativeSize();
        runeImage.gameObject.SetActive(true);

        RuneManager.Instance.StartRune(rune, this);
    }

    public override void RuneCastCallback()
    {
        runeImage.gameObject.SetActive(false);
        //runeTimeRoot.SetActive(false);

        activeRune = null;
        _timer = 0f;
        myState = CasterState.ready;
    }

    public override void MyRuneWasCounteredCallback()
    {
        base.MyRuneWasCounteredCallback();
    }

    public override void HideCurrentRuneCallback()
    {
        base.HideCurrentRuneCallback();

        runeImage.gameObject.SetActive(false);
        _timer = 0f;
        myState = CasterState.ready;
    }

    public bool ChanceToCounterRune(Rune rune)
    {
        bool willCounter = Random.Range(0, 100) > chanceToCounter;
        if (willCounter)
            StartCoroutine(_CounterRune(rune));
        return willCounter;
    }

    IEnumerator _CounterRune(Rune rune)
    {
        myState = CasterState.casting;
        yield return new WaitForSeconds(Random.Range(0.5f, RuneManager.Instance.runeTime - 0.25f));
        CastRune(rune.counterRuneName);
    }
}
