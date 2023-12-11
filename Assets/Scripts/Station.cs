using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OscJack;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TelloLib;
using System;
using Unity.VisualScripting;
using System.Reflection;

public class Station : HimeLib.SingletonMono<Station>
{
    [Header("Arduino 常數")]
    public ArduinoInteractive arduino;
    [Header("GUI Log")]
    public Text TXT_Command;
    public Text TXT_TelloStats;
    public Text TXT_Battery;
    public InputField INP_StationIndex;
    public int maxMessageLine = 20;

    [Header("Systems")]
    public float noCommandTimeLimit = 1;
    public int StationIndex;
    public OSCAdapter oscAdapter;
    public OscPropertySender batterySender;
    public OscPropertySender statSender;
    Queue<string> Log_Command = new Queue<string>();

    float noCommandTime = 0;
    System.Action threadPass;
    float lx = 0f;			//旋轉
    float ly = 0f;			//上下
    float rx = 0f;			//左右
    float ry = 0f;			//前後

    void Start()
    {
        MemorySetting();
        //OSC接收端
        CommandsScribe();

        Tello.onConnection += Tello_onConnection;
        Tello.onUpdate += Tello_onUpdate;

        Tello.startConnecting();
    }

    void Tello_onConnection(Tello.ConnectionState newState)
	{
		//Debug.Log("Tello_onConnection : " + newState);
		if (newState == Tello.ConnectionState.Connected) {
            Tello.queryAttAngle();
            Tello.setMaxHeight(50);
		}
	}

    void MemorySetting(){
        INP_StationIndex.onValueChanged.AddListener(x => {
            StationIndex = Convert.ToInt32(x);
            SystemConfig.Instance.SaveData("station", StationIndex);

            FieldInfo field1 = typeof(OscPropertySender).GetField("_oscAddress", BindingFlags.NonPublic | BindingFlags.Instance);
            field1?.SetValue(batterySender, $"/battery{StationIndex}");
            batterySender.enabled = true;
            
            FieldInfo field2 = typeof(OscPropertySender).GetField("_oscAddress", BindingFlags.NonPublic | BindingFlags.Instance);
            field2?.SetValue(statSender, $"/stat{StationIndex}");
            statSender.enabled = true;
        });
        var memIndex = SystemConfig.Instance.GetData<int>("station", -1);
        INP_StationIndex.text = memIndex.ToString();
    }

    void CommandsScribe(){
        oscAdapter.OnTelloSDKCommand += TelloCommandSDK;
        oscAdapter.OnArduinoCommand += ArduinoSend;

        //keyboardFly.OnKeyboardEvent += TelloCommand;
    } 

    void Tello_onUpdate(int cmdId)
	{
        threadPass += () => {
            TXT_TelloStats.text = "" + Tello.state;
            TXT_Battery.text = string.Format("Battery {0} %", (TelloLib.Tello.state != null) ? ("" + TelloLib.Tello.state.batteryPercentage) : " - ");
        };
	}

    void TelloCommandSDK(string msg){

        if(msg.Contains(TelloSDKCommands.takeoff)){
            Tello.takeOff();
            DebugLogUI($"TelloSend: {msg}", TXT_Command, Log_Command);
            return;
        }
        if(msg.Contains(TelloSDKCommands.land)){
            Tello.land();
            DebugLogUI($"TelloSend: {msg}", TXT_Command, Log_Command);
            return;
        }
        if(msg.Contains(TelloSDKCommands.stay)){
            lx = 0;
            ly = 0;
            rx = 0;
            ry = 0;
            DebugLogUI($"TelloSend: {msg}", TXT_Command, Log_Command);
            return;
        }
        try {
            string [] splte = msg.Split(" ");
            float.TryParse(splte[1], out float x);
            float.TryParse(splte[2], out float y);
            float.TryParse(splte[3], out float z);
            float.TryParse(splte[4], out float r);

            lx = x;
            ly = y;
            rx = z;
            ry = r;

            noCommandTime = 0;

            if(x != 0 || y != 0 || z != 0 || r != 0){
                DebugLogUI($"TelloSend: {msg}", TXT_Command, Log_Command);
            }

            //Tello.controllerState.setAxis(lx, ly, rx, ry);
            //旋轉,上下,左右,前後
            //Tello.controllerState.setAxis(r, y, x, z);

        } catch {}
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

        noCommandTime += Time.deltaTime;
        if(noCommandTime > noCommandTimeLimit){
            noCommandTime = 0;
            lx = 0;
            ly = 0;
            rx = 0;
            ry = 0;
        }

        Tello.controllerState.setAxis(lx, ly, rx, ry);
    }

    void OnApplicationQuit()
	{
		Tello.stopConnecting();
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
    public static string[] withParamCommand = new string[] { rc, up, down, forward, back, left, right, cw, ccw};
}

public class TelloSDKCommands
{
    public static string takeoff = "takeoff";
    public static string land = "land";
    public static string stay = "stay";
    public static string axis = "axis";
    public static string[] commands = new string[] { takeoff, land, stay, axis };
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