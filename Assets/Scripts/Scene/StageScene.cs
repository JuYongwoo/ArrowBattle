using System;
using System.Collections;
using UnityEngine;

public class StageScene : MonoBehaviour
{
    GameObject player; //현재 씬의 플레이어
    GameObject enemy; //현재 씬의 적



    private int gameLeftTime = 99;
    private TimerRunner _timerRunner;

    public void Awake()
    {
        
        loadstage(1); //지금은 GetStageDatabyStageID(1) 로 스테이지 1만 실행, 클리어 시 GetStageDatabyStageID(2)로 스테이지 정보 불러오도록 함

        if (_timerRunner == null)
        {
            var go = new GameObject("__GameModeTimer");
            DontDestroyOnLoad(go);
            _timerRunner = go.AddComponent<TimerRunner>();
        }

        _timerRunner.StartRepeating(flowTime, 1f);
        // _timerRunner.StartRepeatingRealtime(flowTime, 1f); //timeScale 무시


        ManagerObject.instance.actionManager.EndGameEvent -= endGame; // ActionManager의 endGame 이벤트에 endGame 메서드 구독
        ManagerObject.instance.actionManager.EndGameEvent += endGame;

    }

    private void OnDestroy()
    {
        ManagerObject.instance.actionManager.EndGameEvent -= endGame;

    }

    private void flowTime()
    {
        gameLeftTime--;

        ManagerObject.instance.actionManager.OnSetGameTimeUI(gameLeftTime); //TimePanel의 시간을 세팅하는 델리게이트 호출
        if (gameLeftTime <= 0)
        {
            endGame(ResultStateEnum.Defeat); //시간 종료로 패배
        }
    }

    public void loadstage(int stageID)
    {
        //스테이지 별 SO에서 정의된 캐릭터ID를 토대로 캐릭터 ID 별 SO 속 정보에 따라 씬에 생성
        foreach (var spawnCharacters in ManagerObject.instance.resourceManager.gameModeData.Result.GetStageDatabyStageID(stageID).characterTypeEnum)
        {
            CharacterStatData stat = ManagerObject.instance.resourceManager.characterDatas.Result.GetCharacterDataById(spawnCharacters);
            GameObject go = MonoBehaviour.Instantiate(stat.characterPrefab, stat.startPosition, Quaternion.identity);
            if (go.CompareTag("Player")) player = go;
            else if (go.CompareTag("Enemy")) enemy = go;
        }

        //BGM 재생
        ManagerObject.instance.audioM.PlayAudioClip(ManagerObject.instance.resourceManager.gameModeData.Result.GetStageDatabyStageID(stageID).bgm, 0.2f, true);

        gameLeftTime = ManagerObject.instance.resourceManager.gameModeData.Result.GetStageDatabyStageID(stageID).gameTime;
    }

    public void endGame(ResultStateEnum resultStateEnum)
    {
        // 반복 중지
        if (_timerRunner != null)
            _timerRunner.StopRepeating();

        if (resultStateEnum == ResultStateEnum.Victory) ManagerObject.instance.audioM.PlayAudioClip(ManagerObject.instance.resourceManager.gameModeData.Result.victoryMusic, 0.3f, false);
        else ManagerObject.instance.audioM.PlayAudioClip(ManagerObject.instance.resourceManager.gameModeData.Result.defeatMusic, 0.2f, false);

        Time.timeScale = 0f; //게임 일시정지
        ManagerObject.instance.audioM.StopAudioClip(ManagerObject.instance.resourceManager.gameModeData.Result.GetStageDatabyStageID(1).bgm); //BGM 정지
        ManagerObject.instance.actionManager.OnGameResultUI(resultStateEnum); //ResultPanel의 UI를 세팅하는 델리게이트 호출
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


