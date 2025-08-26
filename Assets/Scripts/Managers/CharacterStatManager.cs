using System;
using UnityEngine.AddressableAssets;


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

    public CharacterStatManager(string key)
    {
        var playerData = Addressables.LoadAssetAsync<CharacterStatDataSO>(key).WaitForCompletion();

        Current = new CurrentStat(playerData);

    }

    public void deltaHP(float delta)
    {
        Current.CurrentHP += delta;
        OnRefreshHPBar?.Invoke(Current.CurrentHP, Current.MaxHP);
    }

}
