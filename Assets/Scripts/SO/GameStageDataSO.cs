using UnityEngine;

[CreateAssetMenu(fileName = "NewGameStageData", menuName = "Game/GameStageData")]
public class GameStageDataSO : ScriptableObject
{
    public StageData GetStageDatabyStageID(int stageId)
    {
        foreach (var data in stageDatas)
        {
            if (data.stageId == stageId)
                return data;
        }
        return null;
    }

    public StageData[] stageDatas; //스테이지 1, 2, 3, ...
    public AudioClip victoryMusic;
    public AudioClip defeatMusic;
}

[System.Serializable]
public class StageData
{
    public int stageId; //스테이지 별 ID
    public int gameTime; //스테이지마다 다른 게임 시간
    public AudioClip bgm; //스테이지마다 다른 BGM
    public CharacterTypeEnumByTag[] characterTypeEnum; // 스테이지에서 소환되는 캐릭터 종류들(player, enemy1), (player, enemy2), ...
}