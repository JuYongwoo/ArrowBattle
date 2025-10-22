using System.Collections;
using UnityEngine;

public class LoadingScene : MonoBehaviour
{

    private void Start()
    {
        StartCoroutine(ChangeScene());
    }

    private IEnumerator ChangeScene()
    {
        yield return new WaitForSeconds(2f);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
    }

}