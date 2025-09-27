using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class MainScene : MonoBehaviour
{
    // Other methods and variables...
    GameObject player;
    Player playerComp;
    GameObject enemy;



    private int gameLeftTime = 99;
    // 기존: MonoBehaviour monoBehaviourforInvoke = new MonoBehaviour();  // ← 금지됨
    private TimerRunner _timerRunner; // 안전한 반복 호출을 위한 러너

    // Start is called before the first frame update
    public void Awake()
    {
        //GameMode에서 정의된 캐릭터 프리팹들을 SO 속 정보에 따라 씬에 생성
        foreach (var character in ManagerObject.instance.resourceManager.gameModeData.Result.Characters)
        {
            GameObject go = MonoBehaviour.Instantiate(character.characterPrefab, character.characterStartPosition, Quaternion.identity);
            if (go.CompareTag("Player")) player = go;
            else if (go.CompareTag("Enemy")) enemy = go;
        }

        //해상도
        Screen.SetResolution(1600, 900, false);

        //BGM 재생
        ManagerObject.instance.audioM.PlayBGM(ManagerObject.instance.resourceManager.gameModeData.Result.BGM);

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



        mapOtherAction();
    }
    private void mapOtherAction()
    {
        ManagerObject.instance.actionManager.useSkill = playerComp.prepareSkill; // InputManager의 attack 이벤트에 Attack 메서드 구독
        ManagerObject.instance.actionManager.leftRightMove = playerComp.move; // InputManager의 leftRightMove 이벤트에 Move 메서드 구독
        ManagerObject.instance.actionManager.idle = () => { playerComp.setState(CharacterStateEnum.Idle); };
        ManagerObject.instance.actionManager.getCastingSkill = () => playerComp.castingSkill;

        ManagerObject.instance.actionManager.endGame = endGame; // ActionManager의 endGame 이벤트에 endGame 메서드 구독


        ManagerObject.instance.actionManager.mainSceneInput = () =>
        {


            if (ManagerObject.instance.actionManager.getCastingSkill() != Skill.Attack) return; //일반 공격 외 다른 스킬 캐스팅 중이라면 어떠한 움직임도 X
                                                                                                // 스킬 입력 우선
            if (Input.GetKeyDown(KeyCode.Q)) { ManagerObject.instance.actionManager.useSkill?.Invoke(Skill.Skill1); return; }
            if (Input.GetKeyDown(KeyCode.W)) { ManagerObject.instance.actionManager.useSkill?.Invoke(Skill.Skill2); return; }
            if (Input.GetKeyDown(KeyCode.E)) { ManagerObject.instance.actionManager.useSkill?.Invoke(Skill.Skill3); return; }
            if (Input.GetKeyDown(KeyCode.R)) { ManagerObject.instance.actionManager.useSkill?.Invoke(Skill.Skill4); return; }
            if (Input.GetKeyDown(KeyCode.Space)) { ManagerObject.instance.actionManager.useSkill?.Invoke(Skill.Skill5); return; }

            // 이동/Idle
            float moveX = Input.GetAxisRaw("Horizontal");
            if (moveX != 0f) { ManagerObject.instance.actionManager.leftRightMove?.Invoke(moveX); return; }

            ManagerObject.instance.actionManager.idle?.Invoke();

        };
    }


    private void flowTime()
    {
        gameLeftTime--;

        ManagerObject.instance.actionManager.setGameTime?.Invoke(gameLeftTime); //TimePanel의 시간을 세팅하는 델리게이트 호출
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

        if (resultStateEnum == ResultStateEnum.Victory) ManagerObject.instance.audioM.PlayAudioClip(ManagerObject.instance.resourceManager.gameModeData.Result.VictoryMusic, 4.0f);
        else ManagerObject.instance.audioM.PlayAudioClip(ManagerObject.instance.resourceManager.gameModeData.Result.DefeatMusic, 1.0f);

        Time.timeScale = 0f; //게임 일시정지
        ManagerObject.instance.audioM.StopBGM(); //BGM 정지
        ManagerObject.instance.actionManager.gameResultUI?.Invoke(resultStateEnum); //ResultPanel의 UI를 세팅하는 델리게이트 호출
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


