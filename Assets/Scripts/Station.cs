using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OscJack;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class Station : MonoBehaviour
{
    [Header("Tello 常數")]
    public string telloHostIP = "192.168.10.1";
    public int telloHostPort = 8889;
    public string RecieveTelloIP = "0.0.0.0";
    public int RecieveHostPort = 8890;

    [Header("Arduino 常數")]
    public ArduinoInteractive arduino;

    [Header("GUI Log")]
    public Text GUILog;
    public Text RecieveLog;
    public int maxMessageLine = 20;

    [Header("Systems")]
    public OSCAdapter oscAdapter;

    [Header("Tello Params")]
    public int TelloStep = 20;

    UdpClient telloClient;
    Thread telloReceiveThread;
    UdpClient udpServer;
    Queue<string> LogString = new Queue<string>();
    Queue<string> RecieveLogString = new Queue<string>();

    void Start()
    {
        //飛行器連接
        UDPConnect();

        //飛行器接收端
        TelloServerStart();

        //OSC接收端
        CommandsScribe();
    }

    void CommandsScribe(){
        oscAdapter.OnTelloCommand += TelloCommand;
        oscAdapter.OnArduinoCommand += ArduinoSend;
    } 

    public void TelloServerStart(){
        udpServer = new UdpClient(RecieveHostPort);

        // 使用執行緒開始接收資料
        telloReceiveThread = new Thread(ServerLoopReceiveData);
        telloReceiveThread.Start();
    }

    void ServerLoopReceiveData()
    {
        try
        {
            while (true)
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);

                byte[] receivedData = udpServer.Receive(ref clientEndPoint);
                string receivedMessage = Encoding.UTF8.GetString(receivedData);

                MaxLogMsg(RecieveLogString, $"接收到來自 {clientEndPoint.Address}:{clientEndPoint.Port} 的訊息: {receivedMessage}");
                RecieveLog.text = GetLogStrings(RecieveLogString);
            }
        }
        catch (SocketException ex)
        {
            // 在執行緒中捕獲例外狀況
            MaxLogMsg(RecieveLogString, $"錯誤: {ex.Message}");
        }
    }

    private void OnDestroy() {
        telloReceiveThread.Abort();
    }

    private void TelloCommand(string mess)
    {
        MaxLogMsg(LogString, $"TelloSend: {mess}");
        GUILog.text = GetLogStrings(LogString);

        //UDPConnect();
        if(!IsUdpConnected()){
            UDPConnect();
            MaxLogMsg(LogString, "UDP 重新連接");
        }
        
        try {
            byte[] cmd = Encoding.UTF8.GetBytes(mess);
            telloClient.Send(cmd, cmd.Length);
        }
        catch (System.Exception e) {
            Debug.Log("Tello Send error: " + e);
        }
    }

    private void ArduinoSend(string msg){
        MaxLogMsg(LogString, $"ArduinoSend: {msg}");
        GUILog.text = GetLogStrings(LogString);
        arduino.SendData(msg);
    }

    void UDPConnect(){
        try {
            telloClient = new UdpClient();
            telloClient.Connect(telloHostIP, telloHostPort);
        }
        catch (System.Exception e) {
            Debug.Log("UDP error: " + e);
        }
    }

    bool IsUdpConnected(){
        try {
            telloClient.Client.Send(new byte[] {0}, 0, 0);
            return true;
        }
        catch {
            return false;
        }
    }

    void MaxLogMsg(Queue<string> str, string msg){
        str.Enqueue(msg);
        if(str.Count > maxMessageLine)
            str.Dequeue();

        Debug.Log(msg);
    }

    string GetLogStrings(Queue<string> str){
        string s = "";
        foreach (var item in str)
        {
            s = item + "\n" + s;
        }
        return s;
    }

    void Update()
    {
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

        if (Input.GetKeyDown(KeyCode.Z))
        {
            TelloCommand(TelloCommands.up+" "+TelloStep);
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            TelloCommand(TelloCommands.down+" "+TelloStep);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            TelloCommand(TelloCommands.forward+" "+TelloStep);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            TelloCommand(TelloCommands.back+" "+TelloStep);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            TelloCommand(TelloCommands.left+" "+TelloStep);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            TelloCommand(TelloCommands.right+" "+TelloStep);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TelloCommand(TelloCommands.cw+" "+TelloStep);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            TelloCommand(TelloCommands.ccw+" "+TelloStep);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            TelloCommand(TelloCommands.stop);
        }
        if (Input.GetKeyDown(KeyCode.Home))
        {
            TelloCommand("sdk?");
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