using System;

public class ActionManager
{
    public event Action<float> leftRightMove;
    public event Action<Skill> useSkill;
    public event Action idle;
    public event Func<Skill> getCastingSkill;


    public event Action<int, float> CooldownUI;

    public event Action<ResultStateEnum> endGame;


    public event Action<int> setGameTimeUI; //TimePanel�� �ð��� �����ϴ� ��������Ʈ
    public event Action<ResultStateEnum> gameResultUI; //ResultPanel�� UI�� �����ϴ� ��������Ʈ

    public event Action<float, float> setEnemyHPinUI;
    public event Action<float, float> setPlayerHPinUI;


    public void leftRightMoveM(float a)
    {
        leftRightMove?.Invoke(a);
    }

    public void useSkillM(Skill a)
    {
        useSkill?.Invoke(a);
    }

    public void idleM()
    {
        idle?.Invoke();
    }

    public Skill getCastingSkillM()
    {
        return getCastingSkill?.Invoke() ?? Skill.Skill1;
    }

    public void CooldownUIM(int a, float b)
    {
        CooldownUI?.Invoke(a, b);
    }

    public void endGameM(ResultStateEnum a)
    {
        endGame?.Invoke(a);
    }

    public void setGameTimeUIM(int a)
    {
        setGameTimeUI?.Invoke(a);
    }

    public void gameResultUIM(ResultStateEnum a)
    {
        gameResultUI?.Invoke(a);
    }

    public void setEnemyHPinUIM(float a, float b)
    {
        setEnemyHPinUI?.Invoke(a, b);
    }

    public void setPlayerHPinUIM(float a, float b)
    {
        setPlayerHPinUI?.Invoke(a, b);
    }

}