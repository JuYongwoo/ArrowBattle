using System;

public class ActionManager
{
    public event Action<float> leftRightMove;
    public event Action<Skill> useSkill;
    public event Action idle;
    public event Func<Skill> getCastingSkill;


    public event Action<int, float> CooldownUI;

    public event Action<ResultStateEnum> endGame;


    public event Action<int> setGameTimeUI; //TimePanel의 시간을 세팅하는 델리게이트
    public event Action<ResultStateEnum> gameResultUI; //ResultPanel의 UI를 세팅하는 델리게이트

    public event Action<float, float> setEnemyHPinUI;
    public event Action<float, float> setPlayerHPinUI;


    public void OnLeftRightMove(float a)
    {
        leftRightMove?.Invoke(a);
    }

    public void OnUseSkill(Skill a)
    {
        useSkill?.Invoke(a);
    }

    public void OnIdle()
    {
        idle?.Invoke();
    }

    public Skill OnGetCastingSkill()
    {
        return getCastingSkill?.Invoke() ?? Skill.Skill1;
    }

    public void OnCooldownUI(int a, float b)
    {
        CooldownUI?.Invoke(a, b);
    }

    public void OnEndGame(ResultStateEnum a)
    {
        endGame?.Invoke(a);
    }

    public void OnSetGameTimeUI(int a)
    {
        setGameTimeUI?.Invoke(a);
    }

    public void OnGameResultUI(ResultStateEnum a)
    {
        gameResultUI?.Invoke(a);
    }

    public void OnSetEnemyHPinUI(float a, float b)
    {
        setEnemyHPinUI?.Invoke(a, b);
    }

    public void OnSetPlayerHPinUI(float a, float b)
    {
        setPlayerHPinUI?.Invoke(a, b);
    }

}