using System;

public class ActionManager
{
    public Action<float> leftRightMove;
    public Action<Skill> useSkill;
    public Action idle;
    public Func<Skill> getCastingSkill;


    public Action<Skill, float> CooldownStarted;
    public Action<Skill> CooldownEnded;

    public Action<int, float> CooldownUI;

    public Action<ResultStateEnum> endGame;


    public Action<int> setGameTime; //TimePanel의 시간을 세팅하는 델리게이트
    public Action<ResultStateEnum> gameResultUI; //ResultPanel의 UI를 세팅하는 델리게이트

    public Action<float, float> setEnemyHPinUI;
    public Action<float, float> setPlayerHPinUI;

}
