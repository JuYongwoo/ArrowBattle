using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JYW.ArrowBattle.Managers;

namespace JYW.ArrowBattle.UI
{
    public class TimePanel : MonoBehaviour
    {
        private enum TimePanelEnum
        {
            TimeTxt
        }
        private Dictionary<TimePanelEnum, GameObject> TimePanelmap;

        private void Awake()
        {
            TimePanelmap = Util.MapEnumChildObjects<TimePanelEnum, GameObject>(gameObject);
            ManagerObject.instance.eventManager.SetGameTimeUIEvent -= SetTime;
            ManagerObject.instance.eventManager.SetGameTimeUIEvent += SetTime;
        }

        private void OnDestroy()
        {
            ManagerObject.instance.eventManager.SetGameTimeUIEvent -= SetTime;

        }

        private void SetTime(int time)
        {
            string timeString = $"{time}";
            TimePanelmap[TimePanelEnum.TimeTxt].GetComponent<Text>().text = timeString;
        }
    }
}
