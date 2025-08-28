using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

// JYW 이 스크립트는 게임 시작을 수행합니다.
public enum ResultStateEnum
{
    Victory,
    Defeat
}

public class GameMode
{
    private GameModeDataSO gameModeData;
    static public Action<int> setGameTime; //TimePanel의 시간을 세팅하는 델리게이트
    private int gameLeftTime = 99;

    public Action<ResultStateEnum> gameResultUI; //ResultPanel의 UI를 세팅하는 델리게이트

    // 기존: MonoBehaviour monoBehaviourforInvoke = new MonoBehaviour();  // ← 금지됨
    private TimerRunner _timerRunner; // 안전한 반복 호출을 위한 러너

    // Start is called before the first frame update
    public void OnAwake()
    {
        gameModeData = Addressables.LoadAssetAsync<GameModeDataSO>("GameModeData").WaitForCompletion(); // GameModeDataSO 로드
        //해당 정보를 기반으로 게임을 세팅
    }

    public void OnStart()
    {
        //GameMode에서 정의된 캐릭터 프리팹들을 SO 속 정보에 따라 씬에 생성
        foreach (var character in gameModeData.Characters)
        {
            MonoBehaviour.Instantiate(character.characterPrefab, character.characterStartPosition, Quaternion.identity);
        }

        //해상도
        Screen.SetResolution(1600, 900, false);

        //BGM 재생
        ManagerObject.audioM.PlayBGM(gameModeData.BGM);

        gameLeftTime = gameModeData.GameTime;

        if (_timerRunner == null)
        {
            var go = new GameObject("__GameModeTimer");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _timerRunner = go.AddComponent<TimerRunner>();
        }

        _timerRunner.StartRepeating(flowTime, 1f);
        // _timerRunner.StartRepeatingRealtime(flowTime, 1f); // ← timeScale 무시 버전
    }

    private void flowTime()
    {
        gameLeftTime--;

        setGameTime?.Invoke(gameLeftTime); //TimePanel의 시간을 세팅하는 델리게이트 호출
        if (gameLeftTime <= 0)
        {
            endGame(ResultStateEnum.Defeat); //시간 종료로 패배
        }
    }

    public void endGame(ResultStateEnum resultStateEnum)
    {
        // 반복 중지
        if (_timerRunner != null)
            _timerRunner.StopRepeating();

        if(resultStateEnum == ResultStateEnum.Victory) ManagerObject.audioM.PlayAudioClip(gameModeData.VictoryMusic, 4.0f);
        else ManagerObject.audioM.PlayAudioClip(gameModeData.DefeatMusic, 1.0f);

        Time.timeScale = 0f; //게임 일시정지
        ManagerObject.audioM.StopBGM(); //BGM 정지
        gameResultUI?.Invoke(resultStateEnum); //ResultPanel의 UI를 세팅하는 델리게이트 호출
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
        var wait = new WaitForSeconds(interval); // timeScale 영향 받음
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
            yield return new WaitForSecondsRealtime(interval); // timeScale 무시
        }
    }
}
