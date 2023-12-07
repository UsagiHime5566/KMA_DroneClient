using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TelloLib;

public class KeyboardFlyV2 : MonoBehaviour
{
    void Awake(){
        Tello.onConnection += Tello_onConnection;
    }

    void Tello_onConnection(Tello.ConnectionState newState)
	{
		//Debug.Log("Tello_onConnection : " + newState);
		if (newState == Tello.ConnectionState.Connected) {
            Tello.queryAttAngle();
            Tello.setMaxHeight(50);
		}
	}

    void Start()
    {
        Tello.startConnecting();
    }

    void OnApplicationQuit()
	{
		Tello.stopConnecting();
	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
			Tello.takeOff();
		} else if (Input.GetKeyDown(KeyCode.Alpha2)) {
			Tello.land();
		}

		float lx = 0f;
		float ly = 0f;
		float rx = 0f;
		float ry = 0f;

		if (Input.GetKey(KeyCode.W)) {
			ry = 1;
		}
		if (Input.GetKey(KeyCode.S)) {
			ry = -1;
		}
		if (Input.GetKey(KeyCode.D)) {
			rx = 1;
		}
		if (Input.GetKey(KeyCode.A)) {
			rx = -1;
		}
		if (Input.GetKey(KeyCode.Z)) {
			ly = 1;
		}
		if (Input.GetKey(KeyCode.C)) {
			ly = -1;
		}
		if (Input.GetKey(KeyCode.E)) {
			lx = 1;
		}
		if (Input.GetKey(KeyCode.Q)) {
			lx = -1;
		}
		Tello.controllerState.setAxis(lx, ly, rx, ry);
    }
}
