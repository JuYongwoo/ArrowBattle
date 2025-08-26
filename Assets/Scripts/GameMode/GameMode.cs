using UnityEngine;
using UnityEngine.AddressableAssets;

// JYW 이 스크립트는 게임 시작을 수행합니다.

public class GameMode : MonoBehaviour
{

    private GameModeDataSO gameModeData;
    private AudioClip BGM;

    // Start is called before the first frame update

    private void Awake()
    {
        gameModeData = Addressables.LoadAssetAsync<GameModeDataSO>("GameModeData").WaitForCompletion(); // GameModeDataSO 로드
        BGM = Addressables.LoadAssetAsync<AudioClip>("BGM").WaitForCompletion();
        //해당 정보를 기반으로 게임을 세팅


    }
    private void Start()
    {
        //GameMode에서 정의된 캐릭터 프리팹들을 SO 속 정보에 따라 씬에 생성
        foreach (var character in gameModeData.Characters)
        {
            Instantiate(character.characterPrefab, character.characterStartPosition, Quaternion.identity);
        }

        //해상도
        Screen.SetResolution(1600, 900, false);

        //BGM 재생
        ManagerObject.audioM.PlayBGM(BGM);
    }

}
