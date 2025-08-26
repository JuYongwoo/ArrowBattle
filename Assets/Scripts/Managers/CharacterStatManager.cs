using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.ResourceManagement.AsyncOperations;


public class CharacterStatManager //Player와 Enemy 모두 이 클래스의 객체를 가지고 있게 된다.
{
    public struct CurrentStat
    {
        public CurrentStat(CharacterStatDataSO playerData)
        {
            this.MaxHP = playerData.MaxHP;
            this.CurrentHP = playerData.CurrentHP;
            this.CurrentMoveSpeed = playerData.CurrentMoveSpeed;
        }

        public float MaxHP;
        public float CurrentHP;
        public float CurrentMoveSpeed;

    }

    public CurrentStat Current; //계속 수치가 변해야하므로 readonly 사용 X



    public static Action<float, float> OnRefreshHPBar;

    //TODO JYW 플레이어마다 가지고 있을 스킬들 목록도 여기서 존재가능

    public CharacterStatManager()
    {
        var playerData = Addressables.LoadAssetAsync<CharacterStatDataSO>("CharacterStatData").WaitForCompletion();

        Current = new CurrentStat(playerData);

    }

    public void deltaHP(float delta)
    {
        Current.CurrentHP += delta;
        OnRefreshHPBar?.Invoke(Current.CurrentHP, Current.MaxHP);
    }

}
