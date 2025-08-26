using UnityEngine;


[CreateAssetMenu(fileName = "NewCharacterStatData", menuName = "Game/CharacterStatData")]
public class CharacterStatDataSO : ScriptableObject //이 SO를 이용하여 플레이어와 적의 스탯 데이터를 할당
{
    public float CharacterHP;
    public float CharacterMoveSpeed;

}