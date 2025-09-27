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
    // ����: MonoBehaviour monoBehaviourforInvoke = new MonoBehaviour();  // �� ������
    private TimerRunner _timerRunner; // ������ �ݺ� ȣ���� ���� ����

    // Start is called before the first frame update
    public void Awake()
    {
        //GameMode���� ���ǵ� ĳ���� �����յ��� SO �� ������ ���� ���� ����
        foreach (var character in ManagerObject.instance.resourceManager.gameModeData.Result.Characters)
        {
            GameObject go = MonoBehaviour.Instantiate(character.characterPrefab, character.characterStartPosition, Quaternion.identity);
            if (go.CompareTag("Player")) player = go;
            else if (go.CompareTag("Enemy")) enemy = go;
        }

        //�ػ�
        Screen.SetResolution(1600, 900, false);

        //BGM ���
        ManagerObject.instance.audioM.PlayBGM(ManagerObject.instance.resourceManager.gameModeData.Result.BGM);

        gameLeftTime = ManagerObject.instance.resourceManager.gameModeData.Result.GameTime;

        if (_timerRunner == null)
        {
            var go = new GameObject("__GameModeTimer");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _timerRunner = go.AddComponent<TimerRunner>();
        }

        _timerRunner.StartRepeating(flowTime, 1f);
        // _timerRunner.StartRepeatingRealtime(flowTime, 1f); // �� timeScale ���� ����
        playerComp = player.GetComponent<Player>();
        // Other initialization code...



        mapOtherAction();
    }
    private void mapOtherAction()
    {
        ManagerObject.instance.actionManager.useSkill = playerComp.prepareSkill; // InputManager�� attack �̺�Ʈ�� Attack �޼��� ����
        ManagerObject.instance.actionManager.leftRightMove = playerComp.move; // InputManager�� leftRightMove �̺�Ʈ�� Move �޼��� ����
        ManagerObject.instance.actionManager.idle = () => { playerComp.setState(CharacterStateEnum.Idle); };
        ManagerObject.instance.actionManager.getCastingSkill = () => playerComp.castingSkill;

        ManagerObject.instance.actionManager.endGame = endGame; // ActionManager�� endGame �̺�Ʈ�� endGame �޼��� ����


        ManagerObject.instance.actionManager.mainSceneInput = () =>
        {


            if (ManagerObject.instance.actionManager.getCastingSkill() != Skill.Attack) return; //�Ϲ� ���� �� �ٸ� ��ų ĳ���� ���̶�� ��� �����ӵ� X
                                                                                                // ��ų �Է� �켱
            if (Input.GetKeyDown(KeyCode.Q)) { ManagerObject.instance.actionManager.useSkill?.Invoke(Skill.Skill1); return; }
            if (Input.GetKeyDown(KeyCode.W)) { ManagerObject.instance.actionManager.useSkill?.Invoke(Skill.Skill2); return; }
            if (Input.GetKeyDown(KeyCode.E)) { ManagerObject.instance.actionManager.useSkill?.Invoke(Skill.Skill3); return; }
            if (Input.GetKeyDown(KeyCode.R)) { ManagerObject.instance.actionManager.useSkill?.Invoke(Skill.Skill4); return; }
            if (Input.GetKeyDown(KeyCode.Space)) { ManagerObject.instance.actionManager.useSkill?.Invoke(Skill.Skill5); return; }

            // �̵�/Idle
            float moveX = Input.GetAxisRaw("Horizontal");
            if (moveX != 0f) { ManagerObject.instance.actionManager.leftRightMove?.Invoke(moveX); return; }

            ManagerObject.instance.actionManager.idle?.Invoke();

        };
    }


    private void flowTime()
    {
        gameLeftTime--;

        ManagerObject.instance.actionManager.setGameTime?.Invoke(gameLeftTime); //TimePanel�� �ð��� �����ϴ� ��������Ʈ ȣ��
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

        if (resultStateEnum == ResultStateEnum.Victory) ManagerObject.instance.audioM.PlayAudioClip(ManagerObject.instance.resourceManager.gameModeData.Result.VictoryMusic, 4.0f);
        else ManagerObject.instance.audioM.PlayAudioClip(ManagerObject.instance.resourceManager.gameModeData.Result.DefeatMusic, 1.0f);

        Time.timeScale = 0f; //���� �Ͻ�����
        ManagerObject.instance.audioM.StopBGM(); //BGM ����
        ManagerObject.instance.actionManager.gameResultUI?.Invoke(resultStateEnum); //ResultPanel�� UI�� �����ϴ� ��������Ʈ ȣ��
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
}


