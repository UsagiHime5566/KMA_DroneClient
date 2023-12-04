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
    [Header("OSC 變數")]
    public int oscServerPort = 25501;

    [Header("Tello 常數")]
    public string telloHostIP = "192.168.10.1";
    public int telloHostPort = 8889;
    public string RecieveTelloIP = "0.0.0.0";
    public int RecieveHostPort = 8890;

    [Header("GUI Log")]
    public Text GUILog;
    public Text RecieveLog;
    public int maxMessageLine = 20;

    [Header("Tello Params")]
    public int TelloStep = 20;

    UdpClient telloClient;
    UdpClient udpServer;
    OscServer oscServer;
    Queue<string> LogString = new Queue<string>();
    Queue<string> RecieveLogString = new Queue<string>();


    void Start()
    {
        UDPConnect();
        TelloServerStart();
    }

    public void OscServerStart(){
        oscServer = new OscServer(oscServerPort);

        oscServer.MessageDispatcher.AddCallback(
            "/test", // OSC address
            (string address, OscDataHandle data) => {
                Debug.Log(string.Format("({0}, {1})",
                    data.GetElementAsFloat(0),
                    data.GetElementAsFloat(1)));
            }
        );
    }

    public void TelloServerStart(){
        udpServer = new UdpClient(RecieveHostPort);

        // 使用執行緒開始接收資料
        Thread receiveThread = new Thread(ServerLoopReceiveData);
        receiveThread.Start();
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
    public static string rc = "rc";
    public static string up = "up";             //with 1 param
    public static string down = "down";         //with 1 param
    public static string forward = "forward";  //with 1 param
    public static string back = "back";        //with 1 param
    public static string left = "left";        //with 1 param
    public static string right = "right";      //with 1 param
    public static string cw = "cw";      //with 1 param
    public static string ccw = "ccw";      //with 1 param
    public static string speed = "speed";      //with 1 param
    
}