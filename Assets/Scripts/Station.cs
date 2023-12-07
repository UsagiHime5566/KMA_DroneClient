using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OscJack;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TelloLib;

public class Station : MonoBehaviour
{
    [Header("Arduino 常數")]
    public ArduinoInteractive arduino;

    [Header("GUI Log")]
    public Text TXT_Command;
    public Text TXT_TelloStats;
    public Text TXT_Battery;
    public int maxMessageLine = 20;

    [Header("Systems")]
    public OSCAdapter oscAdapter;
    Queue<string> Log_Command = new Queue<string>();

    System.Action threadPass;

    void Start()
    {
        //OSC接收端
        CommandsScribe();

        Tello.onUpdate += Tello_onUpdate;
    }

    void CommandsScribe(){
        //oscAdapter.OnTelloCommand += TelloCommand;
        oscAdapter.OnArduinoCommand += ArduinoSend;

        //keyboardFly.OnKeyboardEvent += TelloCommand;
    } 

    void Tello_onUpdate(int cmdId)
	{
        threadPass += () => {
            TXT_TelloStats.text = "" + Tello.state;
            TXT_Battery.text = string.Format("Battery {0} %", ((TelloLib.Tello.state != null) ? ("" + TelloLib.Tello.state.batteryPercentage) : " - "));
        };
	}

    void ArduinoSend(string msg){
        DebugLogUI($"ArduinoSend: {msg}", TXT_Command, Log_Command);
        arduino.SendData(msg);
    }

    void DebugLogUI(string msg, Text txt, Queue<string> str){
        str.Enqueue(msg);
        if(str.Count > maxMessageLine)
            str.Dequeue();

        string s = "";
        foreach (var item in str)
        {
            s = item + "\n" + s;
        }
        
        txt.text = s;

        //Debug.Log(msg);
    }

    void Update(){
        if(threadPass != null){
            threadPass.Invoke();
            threadPass = null;
        }
    }
}


public class TelloCommands
{
    public static string takeoff = "takeoff";
    public static string land = "land";
    public static string command = "command";
    public static string stop = "stop";
    public static string rc = "rc";             //with 4 param
    public static string up = "up";             //with 1 param
    public static string down = "down";         //with 1 param
    public static string forward = "forward";  //with 1 param
    public static string back = "back";        //with 1 param
    public static string left = "left";        //with 1 param
    public static string right = "right";      //with 1 param
    public static string cw = "cw";             //with 1 param
    public static string ccw = "ccw";           //with 1 param
    public static string speed = "speed";      //with 1 param
    public static string[] noParamCommand = new string[] { takeoff, land, command, stop };
    public static string[] withParamCommand = new string[] { rc, up, down, forward, back, left, right, cw, ccw };
}

public class ArduinoCommands
{
    public static string on1 = "1on";
    public static string on2 = "2on";
    public static string off1 = "1off";
    public static string off2 = "2off";
    public static string con = "con";
    public static string coff = "coff";
    public static string close = "close";
    public static string[] noParamCommand = new string[] { on1, on2, off1, off2, con, coff, close };
}