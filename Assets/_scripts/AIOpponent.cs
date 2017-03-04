using UnityEngine;
using UnityEngine.UI;

public class AIOpponent : Caster
{
    public float actionTime;
    [Range(1,100)]
    public float chanceForAction;

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
            CastRandomRune();
        else
            _timer = 0f;
    }

    void CastRandomRune()
    {
        myState = CasterState.casting;
        Rune rune = RuneManager.Instance.GetRandomRune();
        runeImage.sprite = rune.sprite;
        runeImage.gameObject.SetActive(true);

        RuneManager.Instance.StartRune(rune, this);
    }

    public override void RuneCastCallback()
    {
        runeImage.gameObject.SetActive(false);
        runeTimeRoot.SetActive(false);

        _timer = 0f;
        myState = CasterState.ready;
    }

    public override void RuneCounteredCallback()
    {
        base.RuneCounteredCallback();
    }

    public override void HideCurrentRuneCallback()
    {
        base.HideCurrentRuneCallback();

        runeImage.gameObject.SetActive(false);
        _timer = 0f;
        myState = CasterState.ready;
    }
}
