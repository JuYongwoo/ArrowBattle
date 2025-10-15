using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;


public enum CharacterStateEnum
{
    Idle,
    Moving,
    UsingSkill
}
public enum Skill
{
    Attack,
    Skill1, Skill2, Skill3, Skill4, Skill5
}


public enum ResultStateEnum
{
    Victory,
    Defeat
}

public enum CharacterTypeEnumByTag
{
    Player,
    Enemy
}


public class ResourceManager
{

    public AsyncOperationHandle<SkillDatasSO> SkillDatas;
    public Dictionary<ResultStateEnum, AsyncOperationHandle<Sprite>> ResultImgmap;
    public AsyncOperationHandle<CharacterStatDataSO> characterDatas;


    public AsyncOperationHandle<GameStageDataSO> gameModeData;



    public void OnAwake()
    {

        ResultImgmap = Util.LoadDictWithEnum<ResultStateEnum, Sprite>();

        SkillDatas = Util.AsyncLoad<SkillDatasSO>("SkillDatas");

        characterDatas = Util.AsyncLoad<CharacterStatDataSO>("CharacterDatas");

        gameModeData = Util.AsyncLoad<GameStageDataSO>("GameStageDataSO");
    }
}
