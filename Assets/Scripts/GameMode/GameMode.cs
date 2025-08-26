using UnityEngine;
using UnityEngine.AddressableAssets;


// JYW �� ��ũ��Ʈ�� ���� ���� ������ GameModeDataSO�� �ε��ϰ� SO�� ���ǵ� ĳ���͵��� ���� �����ϴ� �ܼ��� ������ �մϴ�.

public class GameMode : MonoBehaviour
{

    private GameModeDataSO gameModeData;
    // Start is called before the first frame update

    private void Awake()
    {
        gameModeData = Addressables.LoadAssetAsync<GameModeDataSO>("GameModeData").WaitForCompletion(); // GameModeDataSO �ε�
        //�ش� ������ ������� ������ ����


    }
    private void Start()
    {
        //GameMode���� ���ǵ� ĳ���� �����յ��� SO �� ������ ���� ���� ����
        foreach (var character in gameModeData.Characters)
        {
            Instantiate(character.characterPrefab, character.characterStartPosition, Quaternion.identity);
        }
    }

}
