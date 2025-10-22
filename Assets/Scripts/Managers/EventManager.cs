using System;
using UnityEngine;

namespace JYW.ArrowBattle.Managers
{

    public class EventManager
    {
        public event Action<float> LeftRightMoveEvent;
        public event Action<SkillType> UseSkillEvent;
        public event Action SetIdleEvent;
        public event Func<SkillType> GetCastingSkillEvent;


        public event Action<int, float> CooldownUIEvent;

        public event Action<ResultStateEnum> EndGameEvent;


        public event Action<int> SetGameTimeUIEvent; //TimePanel의 시간을 세팅하는 델리게이트
        public event Action<ResultStateEnum> GameResultUIEvent; //ResultPanel의 UI를 세팅하는 델리게이트

        public event Action<float, float> SetEnemyHPInUIEvent;
        public event Action<float, float> SetPlayerHPInUIEvent;

        public event Action<AudioClip, float, bool> PlayAudioClipEvent;
        public event Action<AudioClip> StopAudioClipEvent;
        public event Action StopAllAudioClipEvent;
        public event Action<float> SetMasterVolumeEvent;

        public void OnLeftRightMove(float a)
        {
            LeftRightMoveEvent?.Invoke(a);
        }

        public void OnUseSkill(SkillType a)
        {
            UseSkillEvent?.Invoke(a);
        }

        public void OnSetIdle()
        {
            SetIdleEvent?.Invoke();
        }

        public SkillType OnGetCastingSkill()
        {
            return GetCastingSkillEvent?.Invoke() ?? SkillType.Skill1;
        }

        public void OnCooldownUI(int a, float b)
        {
            CooldownUIEvent?.Invoke(a, b);
        }

        public void OnEndGame(ResultStateEnum a)
        {
            EndGameEvent?.Invoke(a);
        }

        public void OnSetGameTimeUI(int a)
        {
            SetGameTimeUIEvent?.Invoke(a);
        }

        public void OnGameResultUI(ResultStateEnum a)
        {
            GameResultUIEvent?.Invoke(a);
        }

        public void OnSetEnemyHPInUI(float a, float b)
        {
            SetEnemyHPInUIEvent?.Invoke(a, b);
        }

        public void OnSetPlayerHPInUI(float a, float b)
        {
            SetPlayerHPInUIEvent?.Invoke(a, b);
        }

        public void OnPlayAudioClip(AudioClip ac, float volume, bool isLoop)
        {
            PlayAudioClipEvent?.Invoke(ac, volume, isLoop);
        }

        public void OnStopAudioClip(AudioClip ac)
        {
            StopAudioClipEvent?.Invoke(ac);
        }

        public void OnStopAllAudioClip()
        {
            StopAllAudioClipEvent?.Invoke();
        }

        public void OnSetMasterVolume(float vol)
        {
            SetMasterVolumeEvent?.Invoke(vol);

        }
    }
}