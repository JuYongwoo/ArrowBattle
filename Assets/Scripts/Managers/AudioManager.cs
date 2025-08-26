using System.Collections.Generic;
using UnityEngine;

public class AudioManager
{

    private AudioSource skillAudioSource;
    private AudioSource bgmAudioSource;
    //private Dictionary<Skills, AudioClip> skillSoundsMap;

    public void OnAwake()
    {
        //skillSoundsMap = Util.MapEnumToAddressablesByLabels<Skills, AudioClip>("Sound");

        GameObject target = ManagerObject.instance.gameObject;
        skillAudioSource = target.AddComponent<AudioSource>();
        bgmAudioSource = target.AddComponent<AudioSource>(); // �̸� �����ؼ� �� �� �и�
        bgmAudioSource.loop = true;
        bgmAudioSource.playOnAwake = false;
    }


/*    public void PlaySkillSound(Skills sound, float volume = 1f)
    {
        skillAudioSource.PlayOneShot(skillSoundsMap[sound], volume);
    }
*/

    public void PlayAudioClip(AudioClip sound, float volume = 1f)
    {
        skillAudioSource.PlayOneShot(sound, volume);
    }



    public void PlayBGM(AudioClip BGM, float volume = 1f)
    {
        if (bgmAudioSource.clip == BGM && bgmAudioSource.isPlaying)
            return; // �̹� ��� ���̸� �ٽ� ��� �� ��

        bgmAudioSource.clip = BGM;
        bgmAudioSource.volume = volume;
        bgmAudioSource.Play();
    }

    public void StopBGM()
    {
        if (bgmAudioSource.isPlaying)
            bgmAudioSource.Stop();
    }

}
