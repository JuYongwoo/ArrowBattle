using UnityEngine;




[CreateAssetMenu(fileName = "NewGameModeData", menuName = "Game/GameModeData")]
public class GameModeDataSO : ScriptableObject //단순 게임 시작 시 정보를 담는 SO 파일
{
    [System.Serializable]
    public class CharacterDatas
    {
        public GameObject characterPrefab;
        public Vector2 characterStartPosition;
    }

    public CharacterDatas[] Characters;

}