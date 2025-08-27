using UnityEngine;


    [System.Serializable]
public enum SkillProjectileMovingType
{
    Straight,   // ������
    Parabola,   // ������(2�� ������)
    SineWave,   // ���� ���̺�(Ⱦ����)
    ZigZag,     // �������(�ﰢ��)
    Boomerang,   // �պ�(Ÿ�� �������� ���ٰ� ���� ����)
    Rain
}

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Game/SkillData")]
public class SkillDataSO : ScriptableObject //�� SO�� �̿��Ͽ� �� ���� ��ų �����͸� �Ҵ�
{

    public GameObject skillProjectile; //��ų ����ü ������
    public Sprite skillIcon; //��ų ������ �̹���
    public AudioClip skillSound; //��ų ����
    public float skillDamage; //��ų ������
    public float skillCoolTime; //��ų ��Ÿ��
    public float skillCastingTime; //��ų ���� �ð�
    public float projectileSpeed;// ����ü �ӵ�

    public SkillProjectileMovingType skillProjectileMovingType; //��ų ����ü �̵� Ÿ��, ������, ������
    public float projectileCount;
    public bool isGuided;// �����ΰ�
    public bool isShowPortrait;// ��� �� ���� �ʻ�ȭ ����

}