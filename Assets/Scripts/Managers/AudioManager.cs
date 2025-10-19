using System.Collections.Generic;
using UnityEngine;

public class AudioManager
{
    //private Dictionary<Skills, AudioClip> skillSoundsMap;

    private Dictionary<AudioClip, AudioSource> audioSources = new Dictionary<AudioClip, AudioSource>();


    public void PlayAudioClip(AudioClip ac, float volume, bool isLoop)
    {
        if (!audioSources.ContainsKey(ac)) //�������� �ʴ� ����� Ŭ�� ���� �ҽ�
        {
            KeyValuePair<AudioClip, AudioSource> removeCandi = new KeyValuePair<AudioClip, AudioSource>(null, null);



            //���� ����Ѵٴ� ���� �����ε� ����� �� O, �����ִ� ������ҽ��� �̿��ϵ��� ��ųʸ��� key�� �ٲ��ֵ��� �Ѵ�.
            foreach(var pair in audioSources)
            {
                if (!pair.Value.isPlaying) //�����ִ� ������ҽ��� �ִ°�
                {
                    removeCandi = new KeyValuePair<AudioClip, AudioSource>(pair.Key, pair.Value); //���ο� �����Ŭ�� & �̹� ������ ������ҽ� ���
                    break; // �����ִ� ù ������ҽ��� �����ϰ� ���´�.
                }
            }


            if(removeCandi.Key != null) //���� �ִ� ������ҽ��� ã�Ҵ�
            {
                audioSources.Remove(removeCandi.Key); //���� ��ųʸ� ����(���� ������Ʈ�� �������� ����)
                audioSources.Add(ac, removeCandi.Value); //Ű�� �ٲ㼭 ����Ѵ�
            }
            else//���� �ִ� ������ҽ��� ��ã�Ҵ�.
            {
                var src = ManagerObject.instance.gameObject.AddComponent<AudioSource>(); //���� ����
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                src.priority = 128;
                audioSources.Add(ac, src); //���� ���� ��ųʸ� �߰�
            }

        }

        if (isLoop)
        {
            var s = audioSources[ac];
            if (s.isPlaying && s.clip == ac) return; // �̹� ���� ������ ��� ���̸� ����
        }
        audioSources[ac].volume = volume;
        audioSources[ac].loop = isLoop;

        audioSources[ac].Stop();
        audioSources[ac].clip = ac;
        audioSources[ac].Play();

    }

    public void StopAudioClip(AudioClip ac)
    {
        if (audioSources.ContainsKey(ac))
        {
            audioSources[ac].Stop();
        }
    }



}
