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


    private void Awake()
    {
        TimePanelmap = Util.MapEnumChildObjects<TimePanelEnum, GameObject>(this.gameObject);
        ManagerObject.instance.actionManager.setGameTimeUI -= setTime;
        ManagerObject.instance.actionManager.setGameTimeUI += setTime;
    }

    private void OnDestroy()
    {
        ManagerObject.instance.actionManager.setGameTimeUI -= setTime;

    }

    private void setTime(int time)
    {
        string timeString = $"{time}";
        TimePanelmap[TimePanelEnum.TimeTxt].GetComponent<Text>().text = timeString;
    }
}
