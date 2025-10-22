using System;
using System.Collections;
using UnityEngine;

public class StageScene : MonoBehaviour
{
    //public GameObject player; //현재 씬의 플레이어
    //public GameObject enemy; //현재 씬의 적

    private int gameLeftTime = 99;
    private TimerRunner _timerRunner;

    private void Awake()
    {
        ManagerObject.instance.eventManager.EndGameEvent -= EndGame; // ActionManager의 endGame 이벤트에 endGame 메서드 구독
        ManagerObject.instance.eventManager.EndGameEvent += EndGame;

    }

    private void Start()
    {
        Loadstage(1); //지금은 GetStageDatabyStageID(1) 로 스테이지 1만 실행, 클리어 시 GetStageDatabyStageID(2)로 스테이지 정보 불러오도록 함

        if (_timerRunner == null)
        {
            var go = new GameObject("__GameModeTimer");
            DontDestroyOnLoad(go);
            _timerRunner = go.AddComponent<TimerRunner>();
        }

        _timerRunner.StartRepeating(FlowTime, 1f);
        // _timerRunner.StartRepeatingRealtime(flowTime, 1f); //timeScale 무시

    }

    private void OnDestroy()
    {
        ManagerObject.instance.eventManager.EndGameEvent -= EndGame;

    }

    private void FlowTime()
    {
        gameLeftTime--;

        ManagerObject.instance.eventManager.OnSetGameTimeUI(gameLeftTime); //TimePanel의 시간을 세팅하는 델리게이트 호출
        if (gameLeftTime <= 0)
        {
            EndGame(ResultStateEnum.Defeat); //시간 종료로 패배
        }
    }

    private void Loadstage(int stageID)
    {
        //스테이지 별 SO에서 정의된 캐릭터ID를 토대로 캐릭터 ID 별 SO 속 정보에 따라 씬에 생성
        foreach (var spawnCharacters in ManagerObject.instance.resourceManager.gameModeData.Result.GetStageDatabyStageID(stageID).CharacterTypeEnum)
        {
            CharacterStatData stat = ManagerObject.instance.resourceManager.characterDatas.Result.GetCharacterDataById(spawnCharacters);
            GameObject go = MonoBehaviour.Instantiate(stat.CharacterPrefab, stat.StartPosition, Quaternion.identity);
            //if (go.CompareTag("Player")) player = go;
            //else if (go.CompareTag("Enemy")) enemy = go;
        }

        //BGM 재생
        ManagerObject.instance.eventManager.OnPlayAudioClip(ManagerObject.instance.resourceManager.gameModeData.Result.GetStageDatabyStageID(stageID).Bgm, 0.2f, true);

        gameLeftTime = ManagerObject.instance.resourceManager.gameModeData.Result.GetStageDatabyStageID(stageID).GameTime;
    }

    private void EndGame(ResultStateEnum resultStateEnum)
    {
        ManagerObject.instance.eventManager.OnStopAllAudioClip();
        // 반복 중지
        if (_timerRunner != null)
            _timerRunner.StopRepeating();

        if (resultStateEnum == ResultStateEnum.Victory) ManagerObject.instance.eventManager.OnPlayAudioClip(ManagerObject.instance.resourceManager.gameModeData.Result.GetVictoryMusic(), 0.3f, false);
        else ManagerObject.instance.eventManager.OnPlayAudioClip(ManagerObject.instance.resourceManager.gameModeData.Result.GetDefeatMusic(), 0.2f, false);

        Time.timeScale = 0f; //게임 일시정지
        ManagerObject.instance.eventManager.OnGameResultUI(resultStateEnum); //ResultPanel의 UI를 세팅하는 델리게이트 호출
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
}


