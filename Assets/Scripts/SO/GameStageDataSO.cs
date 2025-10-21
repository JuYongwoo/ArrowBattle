using UnityEngine;

[CreateAssetMenu(fileName = "NewGameStageData", menuName = "Game/GameStageData")]
public class GameStageDataSO : ScriptableObject
{
    [SerializeField]
    private StageData[] stageDatas; //스테이지 1, 2, 3, ...
    [SerializeField]
    private AudioClip victoryMusic;
    [SerializeField]
    private AudioClip defeatMusic;

    public StageData GetStageDatabyStageID(int stageId)
    {
        foreach (var data in stageDatas)
        {
            if (data.StageId == stageId)
                return data;
        }
        return null;
    }
    public AudioClip GetVictoryMusic()
    {
        return victoryMusic;
    }
    public AudioClip GetDefeatMusic()
    {
        return defeatMusic;
    }
}

[System.Serializable]
public class StageData
{
    [SerializeField]
    private int stageId; //스테이지 별 ID
    [SerializeField]
    private int gameTime; //스테이지마다 다른 게임 시간
    [SerializeField]
    private AudioClip bgm; //스테이지마다 다른 BGM
    [SerializeField]
    private CharacterTypeEnumByTag[] characterTypeEnum; // 스테이지에서 소환되는 캐릭터 종류들(player, enemy1), (player, enemy2), ...

    public int StageId => stageId;
    public int GameTime => gameTime;
    public AudioClip Bgm => bgm;
    public CharacterTypeEnumByTag[] CharacterTypeEnum => characterTypeEnum;
}