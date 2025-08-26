using UnityEngine;
using UnityEngine.AddressableAssets;


// JYW
// 이 스크립트는 게임 시작 정보인 GameModeDataSO를 로드하고 SO에 정의된 캐릭터들을 씬에 생성하는 역할을 합니다.

public class GameMode : MonoBehaviour
{

    private GameModeDataSO gameModeData;
    // Start is called before the first frame update

    private void Awake()
    {
        gameModeData = Addressables.LoadAssetAsync<GameModeDataSO>("GameModeData").WaitForCompletion(); //Addressables에서 GameModeDataSO 로드


    }
    private void Start()
    {
        //GameMode에서 정의된 캐릭터들을 SO 속 정보에 따라 씬에 생성
        foreach (var character in gameModeData.Characters)
        {
            Instantiate(character.characterPrefab, character.characterStartPosition, Quaternion.identity);
        }
    }

}
