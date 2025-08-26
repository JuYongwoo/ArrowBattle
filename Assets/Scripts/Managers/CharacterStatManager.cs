using System;
using UnityEngine;
using UnityEngine.AddressableAssets;


public class CharacterStatManager //Player�� Enemy ��� �� Ŭ������ ��ü�� ��������
{
    public struct CharacterStat
    {
        public CharacterStat(CharacterStatDataSO playerData)
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

    public CharacterStat Current; //��� ��ġ�� ���ؾ��ϹǷ� readonly ��� X



    public static Action<float, float> OnRefreshHPBar;

    //TODO JYW �÷��̾�� ������ ���� ��ų�� ��ϵ� ���⼭ ���簡��

    public CharacterStatManager(string key)
    {
        var playerData = Addressables.LoadAssetAsync<CharacterStatDataSO>(key).WaitForCompletion();

        Current = new CharacterStat(playerData);

    }

    public void deltaHP(float delta)
    {
        Current.CurrentHP += delta;
        OnRefreshHPBar?.Invoke(Current.CurrentHP, Current.MaxHP);
    }

}
