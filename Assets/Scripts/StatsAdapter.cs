using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TelloLib;

public class StatsAdapter : MonoBehaviour
{
    public string CurrentBattery => currentBattery();
    public string CurrentStat => currentStat();
    public string currentBattery(){
        return string.Format("Battery: {0} %", (Tello.state != null) ? ("" + Tello.state.batteryPercentage) : " - ");
    }
    
    public string currentStat(){
        return $"height:{Tello.state.height}";
    }
}
