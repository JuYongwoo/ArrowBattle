using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.ResourceManagement.AsyncOperations;


public class CharacterStatManager //Player�� Enemy ��� �� Ŭ������ ��ü�� ������ �ְ� �ȴ�.
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

    public CurrentStat Current; //��� ��ġ�� ���ؾ��ϹǷ� readonly ��� X



    public static Action<float, float> OnRefreshHPBar;

    //TODO JYW �÷��̾�� ������ ���� ��ų�� ��ϵ� ���⼭ ���簡��

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
