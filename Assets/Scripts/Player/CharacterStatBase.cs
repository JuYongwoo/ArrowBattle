using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class CharacterStatBase : MonoBehaviour
{
    public CharacterStatManager stat;
    protected abstract string CharacterDataName { get; } // ex) "PlayerData", "EnemyData" //���� ����ϴ� SO ������ �̸��� �ٸ��� ������ �߻� ������Ƽ�� ����

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
