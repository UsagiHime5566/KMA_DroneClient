using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardArduino : MonoBehaviour
{
    public Button BTN_Aon;
    public Button BTN_Aoff;
    public Button BTN_Bon;
    public Button BTN_Boff;
    public Button BTN_Power;
    void Start()
    {
        BTN_Aon.onClick.AddListener(() => {
            Station.instance.ArduinoSend(ArduinoCommands.on1);
        });
        BTN_Aoff.onClick.AddListener(() => {
            Station.instance.ArduinoSend(ArduinoCommands.off1);
        });

        BTN_Bon.onClick.AddListener(() => {
            Station.instance.ArduinoSend(ArduinoCommands.on2);
        });
        BTN_Boff.onClick.AddListener(() => {
            Station.instance.ArduinoSend(ArduinoCommands.off2);
        });

        BTN_Power.onClick.AddListener(() => {
            Station.instance.ArduinoSend(ArduinoCommands.turnpower);
        });
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Insert)){
            Station.instance.ArduinoSend(ArduinoCommands.turnpower);
        }
        if(Input.GetKeyDown(KeyCode.PageUp)){
            Station.instance.ArduinoSend(ArduinoCommands.con);
        }
        if(Input.GetKeyDown(KeyCode.PageDown)){
            Station.instance.ArduinoSend(ArduinoCommands.coff);
        }
    }
}
