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
    public Toggle TG_backSendData;
    public int maxMessageLine = 20;

    [Header("Systems")]
    public float retryPowerOn = 60;
    public float retryPowerOff = 5;
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

    bool aPush = false;
    bool bPush = false;

    bool sendBackendData = false;

    void Start()
    {
        MemorySetting();
        //OSC接收端
        CommandsScribe();

        Tello.onConnection += Tello_onConnection;
        Tello.onUpdate += Tello_onUpdate;

        Tello.startConnecting();

        StartCoroutine(TelloCommandLoop());
    }

    public void ArduinoInit(){
        
        StartCoroutine(_arduinoInit());

        IEnumerator _arduinoInit(){
            DebugLogUI($"ArduinoSend: {ArduinoCommands.off1} (Auto Reset)", TXT_Command, Log_Command);
            arduino.SendData(ArduinoCommands.off1);
            aPush = false;

            yield return new WaitForSeconds(2);

            DebugLogUI($"ArduinoSend: {ArduinoCommands.off2} (Auto Reset)", TXT_Command, Log_Command);
            arduino.SendData(ArduinoCommands.off2);
            bPush = false;

            yield return new WaitForSeconds(1);

            DebugLogUI($"ArduinoSend: {ArduinoCommands.coff} (Auto Reset)", TXT_Command, Log_Command);
            arduino.SendData(ArduinoCommands.coff);
            yield return new WaitForSeconds(1);
        }
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
        TG_backSendData.onValueChanged.AddListener(x => {
            sendBackendData = x;
            SystemConfig.Instance.SaveData("backendData", x);
        });
        TG_backSendData.isOn = SystemConfig.Instance.GetData<bool>("backendData", false);

        INP_StationIndex.onValueChanged.AddListener(x => {
            StationIndex = Convert.ToInt32(x);
            SystemConfig.Instance.SaveData("station", StationIndex);

            FieldInfo field1 = typeof(OscPropertySender).GetField("_oscAddress", BindingFlags.NonPublic | BindingFlags.Instance);
            field1?.SetValue(batterySender, $"/battery{StationIndex}");
            
            FieldInfo field2 = typeof(OscPropertySender).GetField("_oscAddress", BindingFlags.NonPublic | BindingFlags.Instance);
            field2?.SetValue(statSender, $"/stat{StationIndex}");
        });
        if(sendBackendData){
            batterySender.enabled = true;
            statSender.enabled = true;
        }

        var memIndex = SystemConfig.Instance.GetData<int>("station", -1);
        INP_StationIndex.text = memIndex.ToString();
    }

    void CommandsScribe(){
        oscAdapter.OnTelloSDKCommand += TelloCommandSDK;
        oscAdapter.OnArduinoCommand += ArduinoSend;
        oscAdapter.OnComputerCommand += ComputerControl;

        arduino.OnArduinoInitialzed += ArduinoInit;
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
            NewCommandCome();
            DebugLogUI($"TelloSend: {msg}", TXT_Command, Log_Command);
            return;
        }
        if(msg.Contains(TelloSDKCommands.land)){
            Tello.land();
            NewCommandCome();
            DebugLogUI($"TelloSend: {msg}", TXT_Command, Log_Command);
            return;
        }
        if(msg.Contains(TelloSDKCommands.stay)){
            lx = 0;
            ly = 0;
            rx = 0;
            ry = 0;
            NewCommandCome();
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

            NewCommandCome();

            if(x != 0 || y != 0 || z != 0 || r != 0){
                DebugLogUI($"TelloSend: {msg}", TXT_Command, Log_Command);
            }

            //Tello.controllerState.setAxis(lx, ly, rx, ry);
            //旋轉,上下,左右,前後
            //Tello.controllerState.setAxis(r, y, x, z);

        } catch {}
    }

    public void ArduinoSend(string msg){

        if(msg == ArduinoCommands.on1){
            if(bPush == true){
                DebugLogUI($"ArduinoSend: {msg} but 2 is on, skip..", TXT_Command, Log_Command);
            } else {
                aPush = true;
                arduino.SendData(msg);
                DebugLogUI($"ArduinoSend: {msg}", TXT_Command, Log_Command);
            }
            return;
        }
        if(msg == ArduinoCommands.on2){
            if(aPush == true){
                DebugLogUI($"ArduinoSend: {msg} but 1 is on, skip..", TXT_Command, Log_Command);
            } else {
                bPush = true;
                arduino.SendData(msg);
                DebugLogUI($"ArduinoSend: {msg}", TXT_Command, Log_Command);
            }
            return;
        }
        if(msg == ArduinoCommands.off1){
            aPush = false;
            arduino.SendData(msg);
            DebugLogUI($"ArduinoSend: {msg}", TXT_Command, Log_Command);
            return;
        }
        if(msg == ArduinoCommands.off2){
            bPush = false;
            arduino.SendData(msg);
            DebugLogUI($"ArduinoSend: {msg}", TXT_Command, Log_Command);
            return;
        }
        if(msg == ArduinoCommands.con){
            arduino.SendData(msg);
            DebugLogUI($"ArduinoSend: {msg}", TXT_Command, Log_Command);
            return;
        }
        if(msg == ArduinoCommands.coff){
            arduino.SendData(msg);
            DebugLogUI($"ArduinoSend: {msg}", TXT_Command, Log_Command);
            return;
        }
        if(msg == ArduinoCommands.turnpower){
            StartCoroutine(TurnPower());
            DebugLogUI($"ArduinoSend: {msg}", TXT_Command, Log_Command);
            return;
        }
        if(msg == ArduinoCommands.poweron){
            StartCoroutine(PowerOn());
            DebugLogUI($"ArduinoSend: {msg}", TXT_Command, Log_Command);
            return;
        }
        if(msg == ArduinoCommands.poweroff){
            StartCoroutine(PowerOff());
            DebugLogUI($"ArduinoSend: {msg}", TXT_Command, Log_Command);
            return;
        }

        //其他不須防呆指令
        DebugLogUI($"ArduinoSend: {msg}", TXT_Command, Log_Command);
        arduino.SendData(msg);
    }

    void ComputerControl(string msg){
        if(msg == ComputerCommands.restart){
            StartCoroutine(RestartProgram());
            DebugLogUI($"Computer: {msg}", TXT_Command, Log_Command);
            return;
        }
    }

    IEnumerator RestartProgram(){
        GenerateRestartSequence();
        yield return new WaitForSeconds(1);
        Application.Quit();
    }

    void GenerateRestartSequence()
    {
#if !UNITY_EDITOR
            string exePath = System.IO.Path.GetDirectoryName(Application.dataPath);
            string batName = exePath + "/" + "temp.bat";
            var file = System.IO.File.Open(batName, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite);
            var writer = new System.IO.StreamWriter(file);
            writer.WriteLine("@echo off");
            writer.WriteLine("echo !!!");
            writer.WriteLine("echo Wait for system prepare...");
            writer.WriteLine("ping 127.0.0.1 -n 3 -w 1000");
            writer.WriteLine("cd /D " + exePath);
            writer.WriteLine("shutdown -r -t 1");
            //writer.WriteLine(Application.productName + ".exe");
            writer.Flush();
            file.Close();
            System.Diagnostics.Process.Start("temp.bat");
#endif
    }

    

    IEnumerator TurnPower(){
        arduino.SendData(ArduinoCommands.pon);
        yield return new WaitForSeconds(0.3f);
        arduino.SendData(ArduinoCommands.poff);
    }

    IEnumerator PowerOn(){
        yield return null;

        if(Tello.connected == false){
            yield return TurnPower();
        }

        // yield return new WaitForSeconds(retryPowerOn);

        // if(Tello.connected == false){
        //     yield return TurnPower();
        // }
    }

    IEnumerator PowerOff(){
        yield return null;

        if(Tello.connected == true){
            yield return TurnPower();
        }

        yield return new WaitForSeconds(retryPowerOff);

        if(Tello.connected == true){
            yield return TurnPower();
        }

        yield return new WaitForSeconds(retryPowerOff);

        if(Tello.connected == true){
            yield return TurnPower();
        }
    }

    public void DebugLogUI(string msg){
        DebugLogUI($"Keyboard Send: {msg}", TXT_Command, Log_Command);
    }

    public void NewCommandCome(){
        noCommandTime = 0;
    }

    public void SetMainAxis(float _lx, float _ly, float _rx, float _ry){
        lx = _lx;
        ly = _ly;
        rx = _rx;
        ry = _ry;
        NewCommandCome();
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
            if(Tello.state != null){
                if(Tello.state.flying == true){
                    DebugLogUI($"TelloSend (No Command): axis 0 0 0 0", TXT_Command, Log_Command);
                }
            }
        }
    }

    IEnumerator TelloCommandLoop(){
        WaitForSeconds wait = new WaitForSeconds(0.33f);
        while(true){
            if(Tello.state != null){
                if(Tello.state.flying == true){
                    Tello.controllerState.setAxis(lx, ly, rx, ry);
                }
            }
            yield return wait; 
        }
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
    public static string pon = "pon";
    public static string poff = "poff";
    public static string con = "con";
    public static string coff = "coff";

    //mixed command
    public static string turnpower = "turnpower";
    public static string poweron = "poweron";
    public static string poweroff = "poweroff";
    public static string[] noParamCommand = new string[] { on1, on2, off1, off2, pon, poff, con, coff, turnpower, poweron, poweroff };
}

public class ComputerCommands
{
    public static string restart = "restart";
    public static string[] noParamCommand = new string[] { restart };
}