using UnityEngine;
using UnityEngine.AddressableAssets;

// JYW �� ��ũ��Ʈ�� ���� ������ �����մϴ�.

public class GameMode : MonoBehaviour
{

    private GameModeDataSO gameModeData;
    private AudioClip BGM;

    // Start is called before the first frame update

    private void Awake()
    {
        gameModeData = Addressables.LoadAssetAsync<GameModeDataSO>("GameModeData").WaitForCompletion(); // GameModeDataSO �ε�
        BGM = Addressables.LoadAssetAsync<AudioClip>("BGM").WaitForCompletion();
        //�ش� ������ ������� ������ ����


    }
    private void Start()
    {
        //GameMode���� ���ǵ� ĳ���� �����յ��� SO �� ������ ���� ���� ����
        foreach (var character in gameModeData.Characters)
        {
            Instantiate(character.characterPrefab, character.characterStartPosition, Quaternion.identity);
        }

        //�ػ�
        Screen.SetResolution(1600, 900, false);

        //BGM ���
        ManagerObject.audioM.PlayBGM(BGM);
    }

}
