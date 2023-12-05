using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSCAdapter : MonoBehaviour
{
    public Action<string> OnTelloCommand;
    public Action<string> OnArduinoCommand;

    public void OnCmdCome(string msg){
        Debug.Log($"Come OSC cmd: {msg}");

        foreach (var cmd in TelloCommands.noParamCommand)
        {
            if(msg == cmd){
                OnTelloCommand?.Invoke(cmd);
                break;
            }
        }

        foreach (var cmd in TelloCommands.withParamCommand)
        {
            if(msg.Contains(cmd))
            {
                OnTelloCommand?.Invoke(msg);
                break;
            }
        }

        foreach (var cmd in ArduinoCommands.noParamCommand)
        {
            if(msg == cmd){
                OnArduinoCommand?.Invoke(cmd);
                break;
            }
        }
    }
}
