using UnityEngine;


    [System.Serializable]
    public enum SkillProjectileMovingType
    {
        Parabola,
        Straight
    }

[CreateAssetMenu(fileName = "NewSkillData", menuName = "Game/SkillData")]
public class SkillDataSO : ScriptableObject //�� SO�� �̿��Ͽ� �� ���� ��ų �����͸� �Ҵ�
{

    public string skillName; //��ų �̸�
    public float skillDamage; //��ų ������
    public float skillCooldown; //��ų ��Ÿ��
    public AudioClip skillSound; //��ų ����
    public GameObject skillProjectile; //��ų ����ü ������
    public Sprite skillIcon; //��ų ������ �̹���


    public float projectileSpeed;// ����ü �ӵ�
    public bool isShowPortrait;// ��� �� ���� �ʻ�ȭ ����
    public bool isGuided;// �����ΰ�
    public SkillProjectileMovingType skillProjectileMovingType; //��ų ����ü �̵� Ÿ��, ������, ������

}