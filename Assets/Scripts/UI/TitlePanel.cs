using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TitlePanel : MonoBehaviour
{
    private enum TitlePanelObj
    {
        TitilePlayBtn
    }

    private Dictionary<TitlePanelObj, GameObject> titlePanelObjs;

    private void Awake()
    {
        titlePanelObjs = Util.MapEnumChildObjects<TitlePanelObj, GameObject>(this.gameObject);
    }
    private void Start()
    {
        titlePanelObjs[TitlePanelObj.TitilePlayBtn].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>{SceneManager.LoadScene("Main");});
    }
}
