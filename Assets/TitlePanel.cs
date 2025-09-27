using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public enum TitlePanelObj
{
    TitilePlayBtn
}
public class TitlePanel : MonoBehaviour
{

    private Dictionary<TitlePanelObj, GameObject> titlePanelObjs;


    private void Awake()
    {
        titlePanelObjs = Util.MapEnumChildObjects<TitlePanelObj, GameObject>(this.gameObject);
    }
    void Start()
    {
        titlePanelObjs[TitlePanelObj.TitilePlayBtn]
            .GetComponent<UnityEngine.UI.Button>()
            .onClick.AddListener(() =>
            {
                SceneManager.LoadScene("Main");
            });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
