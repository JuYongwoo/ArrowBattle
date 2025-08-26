using UnityEngine;


[CreateAssetMenu(fileName = "NewSkillData", menuName = "Game/SkillData")]
public class SkillDataSO : ScriptableObject //�� SO�� �̿��Ͽ� �� ���� ��ų �����͸� �Ҵ�
{
    public string skillName; //��ų �̸�
    public float skillDamager; //��ų ������
    public float skillCooldown; //��ų ��Ÿ��
    public Sprite skillIcon; //��ų ������

}