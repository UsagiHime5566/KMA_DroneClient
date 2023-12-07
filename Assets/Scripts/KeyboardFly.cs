using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardFly : MonoBehaviour
{
    public Action<string> OnKeyboardEvent;

    [Header("每次移動量")]
    public int TelloStep = 10;

    [Header("指令傳送間隔")]
    public float sendCommandSpeed = 0.33f;

    public string currentRcCommand;
    bool commandBusy = false;
    float waitTime = 0;
    void Start()
    {
        
    }

    void TelloCommand(string cmd){
        // if(commandBusy == true)
        //     return;

        // OnKeyboardEvent?.Invoke(cmd);
        // commandBusy = true;
        // waitTime = 0;

        OnKeyboardEvent?.Invoke(cmd);
    }

    void SetCurrentRcCommand(string cmd, int left_right, int forward_backward, int up_down, int yaw){
        currentRcCommand = $"{cmd} {left_right} {forward_backward} {up_down} {yaw}";
    }

    // Update is called once per frame
    void Update()
    {
        
        if(waitTime < sendCommandSpeed){
            waitTime += Time.deltaTime;
            if(waitTime >= sendCommandSpeed){
                waitTime = 0;
                TelloCommand(currentRcCommand);
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TelloCommand(TelloCommands.takeoff);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TelloCommand(TelloCommands.land);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TelloCommand(TelloCommands.command);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            TelloCommand(TelloCommands.stop);
        }
        if (Input.GetKeyDown(KeyCode.Home))
        {
            TelloCommand("sdk?");
            Debug.Log($"Send Sdk?");
        }
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            TelloCommand("battery?");
            Debug.Log($"Send battery?");
        }

        // Update Rc Command
        currentRcCommand = $"rc 0 0 0 0";
        if (Input.GetKey(KeyCode.Z))
        {
            SetCurrentRcCommand(TelloCommands.rc, 0, 0, TelloStep, 0);
        }
        if (Input.GetKey(KeyCode.C))
        {
            SetCurrentRcCommand(TelloCommands.rc, 0, 0, -TelloStep, 0);
        }
        if (Input.GetKey(KeyCode.W))
        {
            SetCurrentRcCommand(TelloCommands.rc, 0, TelloStep, 0, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            SetCurrentRcCommand(TelloCommands.rc, 0, -TelloStep, 0, 0);
        }
        if (Input.GetKey(KeyCode.A))
        {
            SetCurrentRcCommand(TelloCommands.rc, -TelloStep, 0, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            SetCurrentRcCommand(TelloCommands.rc, TelloStep, 0, 0, 0);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            SetCurrentRcCommand(TelloCommands.rc, 0, 0, 0, TelloStep);
        }
        if (Input.GetKey(KeyCode.E))
        {
            SetCurrentRcCommand(TelloCommands.rc, 0, 0, 0, -TelloStep);
        }


        // if (Input.GetKeyDown(KeyCode.UpArrow))
        // {
        //     //ArduinoSend("1on\n");
        //     ArduinoSend("1on");
        // }
        // if (Input.GetKeyDown(KeyCode.DownArrow))
        // {
        //     //ArduinoSend("1off\n");
        //     ArduinoSend("1off");
        // }
    }
}
