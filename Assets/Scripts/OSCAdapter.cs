using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSCAdapter : MonoBehaviour
{
    public Action<string> OnTelloRawCommand;
    public Action<string> OnArduinoCommand;
    public Action<string> OnTelloSDKCommand;
    public Action<string> OnComputerCommand;

    public void OnCmdCome(string msg){
        Debug.Log($"Come OSC cmd: {msg}");

        foreach (var cmd in TelloCommands.noParamCommand)
        {
            if(msg == cmd){
                OnTelloRawCommand?.Invoke(cmd);
                break;
            }
        }

        foreach (var cmd in TelloCommands.withParamCommand)
        {
            if(msg.Contains(cmd))
            {
                OnTelloRawCommand?.Invoke(msg);
                break;
            }
        }

        foreach (var cmd in TelloSDKCommands.commands)
        {
            if(msg.Contains(cmd))
            {
                OnTelloSDKCommand?.Invoke(msg);
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

        foreach (var cmd in ComputerCommands.noParamCommand)
        {
            if(msg == cmd){
                OnComputerCommand?.Invoke(cmd);
                break;
            }
        }
    }
}
