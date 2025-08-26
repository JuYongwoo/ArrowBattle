using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class CharacterStatBase : MonoBehaviour
{
    public CharacterStatManager stat;
    protected abstract string CharacterDataName { get; } // ex) "PlayerData", "EnemyData" //각자 사용하는 SO 파일의 이름이 다르기 때문에 추상 프로퍼티로 선언

    protected virtual void Awake()
    {
        stat = new CharacterStatManager(CharacterDataName);
    }
    public void getDamaged(float damageAmount)
    {
        stat.deltaHP(-damageAmount);
        ManagerObject.audioM.PlayAudioClip(stat.Current.HitSound);
    }
}
