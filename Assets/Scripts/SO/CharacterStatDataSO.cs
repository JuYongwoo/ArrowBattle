using UnityEngine;

namespace JYW.ArrowBattle.SO
{

    [CreateAssetMenu(fileName = "NewCharacterStatData", menuName = "Game/CharacterStatData")]
    public class CharacterStatDataSO : ScriptableObject //이 SO를 이용하여 플레이어와 적의 스탯 데이터를 할당
    {
        [SerializeField]
        private CharacterStatData[] characterStatDatas;

        public CharacterStatData GetCharacterDataById(CharacterTypeEnumByTag characterTypeEnum)
        {
            foreach (var data in characterStatDatas)
            {
                if (data.CharacterTypeEnum == characterTypeEnum)
                    return data;
            }
            return null;
        }
    }

    [System.Serializable]
    public class CharacterStatData
    {
        [SerializeField]
        private CharacterTypeEnumByTag characterTypeEnum;
        [SerializeField]
        private GameObject characterPrefab;
        [SerializeField]
        private float maxHP;
        [SerializeField]
        private float currentHP;
        [SerializeField]
        private float currentMoveSpeed;
        [SerializeField]
        private AudioClip hitSound;
        [SerializeField]
        private Vector2 startPosition;                  // 시작 위치

        public CharacterTypeEnumByTag CharacterTypeEnum => characterTypeEnum;
        public GameObject CharacterPrefab => characterPrefab;
        public float MaxHP => maxHP;
        public float CurrentHP => currentHP;
        public float CurrentMoveSpeed => currentMoveSpeed;
        public AudioClip HitSound => hitSound;
        public Vector2 StartPosition => startPosition;
    }
}