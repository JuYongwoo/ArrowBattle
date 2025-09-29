using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

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

    public Dictionary<Skill, AsyncOperationHandle<SkillDataSO>> attackSkillData;
    public Dictionary<ResultStateEnum, AsyncOperationHandle<Sprite>> ResultImgmap;
    public Dictionary<CharacterTypeEnumByTag, AsyncOperationHandle<CharacterStatDataSO>> playerDatas;


    public AsyncOperationHandle<GameModeDataSO> gameModeData;



    public void OnAwake()
    {

        ResultImgmap = Util.LoadDictWithEnum<ResultStateEnum, Sprite>();

        attackSkillData = Util.LoadDictWithEnum<Skill, SkillDataSO>();

        playerDatas = Util.LoadDictWithEnum<CharacterTypeEnumByTag, CharacterStatDataSO>();

        gameModeData = Util.AsyncLoad<GameModeDataSO>("GameModeData");
    }
}
