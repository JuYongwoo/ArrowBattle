using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

// JYW �� ��ũ��Ʈ�� ���� ������ �����մϴ�.
public enum ResultStateEnum
{
    Victory,
    Defeat
}

public class GameMode
{
    private GameModeDataSO gameModeData;
    static public Action<int> setGameTime; //TimePanel�� �ð��� �����ϴ� ��������Ʈ
    private int gameLeftTime = 99;

    public Action<ResultStateEnum> gameResultUI; //ResultPanel�� UI�� �����ϴ� ��������Ʈ

    // ����: MonoBehaviour monoBehaviourforInvoke = new MonoBehaviour();  // �� ������
    private TimerRunner _timerRunner; // ������ �ݺ� ȣ���� ���� ����

    // Start is called before the first frame update
    public void OnAwake()
    {
        gameModeData = Addressables.LoadAssetAsync<GameModeDataSO>("GameModeData").WaitForCompletion(); // GameModeDataSO �ε�
        //�ش� ������ ������� ������ ����
    }

    public void OnStart()
    {
        //GameMode���� ���ǵ� ĳ���� �����յ��� SO �� ������ ���� ���� ����
        foreach (var character in gameModeData.Characters)
        {
            MonoBehaviour.Instantiate(character.characterPrefab, character.characterStartPosition, Quaternion.identity);
        }

        //�ػ�
        Screen.SetResolution(1600, 900, false);

        //BGM ���
        ManagerObject.audioM.PlayBGM(gameModeData.BGM);

        gameLeftTime = gameModeData.GameTime;

        if (_timerRunner == null)
        {
            var go = new GameObject("__GameModeTimer");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _timerRunner = go.AddComponent<TimerRunner>();
        }

        _timerRunner.StartRepeating(flowTime, 1f);
        // _timerRunner.StartRepeatingRealtime(flowTime, 1f); // �� timeScale ���� ����
    }

    private void flowTime()
    {
        gameLeftTime--;

        setGameTime?.Invoke(gameLeftTime); //TimePanel�� �ð��� �����ϴ� ��������Ʈ ȣ��
        if (gameLeftTime <= 0)
        {
            endGame(ResultStateEnum.Defeat); //�ð� ����� �й�
        }
    }

    public void endGame(ResultStateEnum resultStateEnum)
    {
        // �ݺ� ����
        if (_timerRunner != null)
            _timerRunner.StopRepeating();

        if(resultStateEnum == ResultStateEnum.Victory) ManagerObject.audioM.PlayAudioClip(gameModeData.VictoryMusic, 4.0f);
        else ManagerObject.audioM.PlayAudioClip(gameModeData.DefeatMusic, 1.0f);

        Time.timeScale = 0f; //���� �Ͻ�����
        ManagerObject.audioM.StopBGM(); //BGM ����
        gameResultUI?.Invoke(resultStateEnum); //ResultPanel�� UI�� �����ϴ� ��������Ʈ ȣ��
    }
}

public sealed class TimerRunner : MonoBehaviour
{
    private Coroutine _loop;

    public void StartRepeating(Action callback, float intervalSeconds)
    {
        StopRepeating();
        _loop = StartCoroutine(Co_Repeat(callback, intervalSeconds));
    }

    public void StartRepeatingRealtime(Action callback, float intervalSeconds)
    {
        StopRepeating();
        _loop = StartCoroutine(Co_RepeatRealtime(callback, intervalSeconds));
    }

    public void StopRepeating()
    {
        if (_loop != null)
        {
            StopCoroutine(_loop);
            _loop = null;
        }
    }

    private IEnumerator Co_Repeat(Action callback, float interval)
    {
        var wait = new WaitForSeconds(interval); // timeScale ���� ����
        while (true)
        {
            callback?.Invoke();
            yield return wait;
        }
    }

    private IEnumerator Co_RepeatRealtime(Action callback, float interval)
    {
        while (true)
        {
            callback?.Invoke();
            yield return new WaitForSecondsRealtime(interval); // timeScale ����
        }
    }
}
