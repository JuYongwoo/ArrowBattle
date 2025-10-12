using UnityEngine;


[CreateAssetMenu(fileName = "NewCharacterStatData", menuName = "Game/CharacterStatData")]
public class CharacterStatDataSO : ScriptableObject //이 SO를 이용하여 플레이어와 적의 스탯 데이터를 할당
{
    public CharacterStatData[] characterStatDatas;

    public CharacterStatData GetCharacterDataById(CharacterTypeEnumByTag characterTypeEnum)
    {
        foreach (var data in characterStatDatas)
        {
            if (data.characterTypeEnum == characterTypeEnum)
                return data;
        }
        return null;
    }
}

[System.Serializable]
public class CharacterStatData
{
    public CharacterTypeEnumByTag characterTypeEnum;
    public GameObject characterPrefab;
    public float MaxHP;
    public float CurrentHP;
    public float CurrentMoveSpeed;
    public AudioClip HitSound;
    public Vector2 startPosition;                  // 시작 위치
}