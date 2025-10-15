using System;

public class ActionManager
{
    public event Action<float> LeftRightMoveEvent;
    public event Action<Skill> UseSkillEvent;
    public event Action IdleEvent;
    public event Func<Skill> GetCastingSkillEvent;


    public event Action<int, float> CooldownUIEvent;

    public event Action<ResultStateEnum> EndGameEvent;


    public event Action<int> SetGameTimeUIEvent; //TimePanel�� �ð��� �����ϴ� ��������Ʈ
    public event Action<ResultStateEnum> GameResultUIEvent; //ResultPanel�� UI�� �����ϴ� ��������Ʈ

    public event Action<float, float> SetEnemyHPInUIEvent;
    public event Action<float, float> SetPlayerHPInUIEvent;


    public void OnLeftRightMove(float a)
    {
        LeftRightMoveEvent?.Invoke(a);
    }

    public void OnUseSkill(Skill a)
    {
        UseSkillEvent?.Invoke(a);
    }

    public void OnIdle()
    {
        IdleEvent?.Invoke();
    }

    public Skill OnGetCastingSkill()
    {
        return GetCastingSkillEvent?.Invoke() ?? Skill.Skill1;
    }

    public void OnCooldownUI(int a, float b)
    {
        CooldownUIEvent?.Invoke(a, b);
    }

    public void OnEndGame(ResultStateEnum a)
    {
        EndGameEvent?.Invoke(a);
    }

    public void OnSetGameTimeUI(int a)
    {
        SetGameTimeUIEvent?.Invoke(a);
    }

    public void OnGameResultUI(ResultStateEnum a)
    {
        GameResultUIEvent?.Invoke(a);
    }

    public void OnSetEnemyHPInUI(float a, float b)
    {
        SetEnemyHPInUIEvent?.Invoke(a, b);
    }

    public void OnSetPlayerHPInUI(float a, float b)
    {
        SetPlayerHPInUIEvent?.Invoke(a, b);
    }

}