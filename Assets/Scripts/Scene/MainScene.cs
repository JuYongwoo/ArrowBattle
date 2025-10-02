using System;
using System.Collections;
using UnityEngine;

public class MainScene : MonoBehaviour
{
    GameObject player;
    Player playerComp;
    GameObject enemy;



    private int gameLeftTime = 99;
    private TimerRunner _timerRunner; // 안전한 반복 호출을 위한 러너

    public void Awake()
    {
        //GameMode에서 정의된 캐릭터 프리팹들을 SO 속 정보에 따라 씬에 생성
        foreach (var character in ManagerObject.instance.resourceManager.gameModeData.Result.Characters)
        {
            GameObject go = MonoBehaviour.Instantiate(character.characterPrefab, character.characterStartPosition, Quaternion.identity);
            if (go.CompareTag("Player")) player = go;
            else if (go.CompareTag("Enemy")) enemy = go;
        }


        //BGM 재생
        ManagerObject.instance.audioM.PlayAudioClip(ManagerObject.instance.resourceManager.gameModeData.Result.BGM, 0.2f, true);

        gameLeftTime = ManagerObject.instance.resourceManager.gameModeData.Result.GameTime;

        if (_timerRunner == null)
        {
            var go = new GameObject("__GameModeTimer");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _timerRunner = go.AddComponent<TimerRunner>();
        }

        _timerRunner.StartRepeating(flowTime, 1f);
        // _timerRunner.StartRepeatingRealtime(flowTime, 1f); // ← timeScale 무시 버전
        playerComp = player.GetComponent<Player>();
        // Other initialization code...


        ManagerObject.instance.actionManager.endGame -= endGame; // ActionManager의 endGame 이벤트에 endGame 메서드 구독
        ManagerObject.instance.actionManager.endGame += endGame;

    }

    private void OnDestroy()
    {
        ManagerObject.instance.actionManager.endGame -= endGame;

    }

    private void flowTime()
    {
        gameLeftTime--;

        ManagerObject.instance.actionManager.setGameTimeUIM(gameLeftTime); //TimePanel의 시간을 세팅하는 델리게이트 호출
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

        if (resultStateEnum == ResultStateEnum.Victory) ManagerObject.instance.audioM.PlayAudioClip(ManagerObject.instance.resourceManager.gameModeData.Result.VictoryMusic, 0.3f, false);
        else ManagerObject.instance.audioM.PlayAudioClip(ManagerObject.instance.resourceManager.gameModeData.Result.DefeatMusic, 0.2f, false);

        Time.timeScale = 0f; //게임 일시정지
        ManagerObject.instance.audioM.StopAudioClip(ManagerObject.instance.resourceManager.gameModeData.Result.BGM); //BGM 정지
        ManagerObject.instance.actionManager.gameResultUIM(resultStateEnum); //ResultPanel의 UI를 세팅하는 델리게이트 호출
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


