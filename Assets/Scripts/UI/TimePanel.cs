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
    private int timeforUI = 99;


    private void Awake()
    {
        TimePanelmap = Util.MapEnumChildObjects<TimePanelEnum, GameObject>(this.gameObject);
        ManagerObject.instance.actionManager.setGameTime = setTime;
    }
    void Start()
    {

    }


    private void setTime(int time)
    {
        this.timeforUI = time;
        string timeString = $"{time}";
        TimePanelmap[TimePanelEnum.TimeTxt].GetComponent<Text>().text = timeString;
    }
}
