using UnityEngine;

public struct CharacterStat
{
    public CharacterStat(CharacterStatData playerData)
    {
        this.MaxHP = playerData.MaxHP;
        this.CurrentHP = playerData.CurrentHP;
        this.CurrentMoveSpeed = playerData.CurrentMoveSpeed;
        this.HitSound = playerData.HitSound;
    }

    public float MaxHP;
    public float CurrentHP;
    public float CurrentMoveSpeed;
    public AudioClip HitSound;

}
public class CharacterStatManager //Player와 Enemy 모두 이 클래스의 객체를 가지도록
{


    public CharacterStat Current; //계속 수치가 변해야하므로 readonly 사용 X




    //TODO JYW 플레이어마다 가지고 있을 스킬들 목록도 여기서 존재가능

    public CharacterStatManager(CharacterTypeEnumByTag type)
    {
        var playerData = ManagerObject.instance.resourceManager.characterDatas.Result.GetCharacterDataById(type);

        Current = new CharacterStat(playerData);

    }

    public void DeltaHP(float delta)
    {
        Current.CurrentHP += delta;
    }

}
