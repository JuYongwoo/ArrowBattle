using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimePanel : MonoBehaviour
{
    private enum TimePanelEnum
    {
        TimeTxt
    }
    private Dictionary<TimePanelEnum, GameObject> TimePanelmap;
    private int time = 99;


    private void Awake()
    {
        TimePanelmap = Util.mapDictionaryInChildren<TimePanelEnum, GameObject>(this.gameObject);
        InvokeRepeating("flowTime", 1f, 1f);
    }
    void Start()
    {

    }

    private void flowTime()
    {
        time--;
        string timeString = $"{time}";
        TimePanelmap[TimePanelEnum.TimeTxt].GetComponent<Text>().text = timeString;
    }
}
