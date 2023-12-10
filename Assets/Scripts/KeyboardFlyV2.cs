using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TelloLib;
using UnityEngine.UI;

public class KeyboardFlyV2 : MonoBehaviour
{
	public Toggle TG_EnableSelf;
    void Awake(){
        
    }
    void Start()
    {
        TG_EnableSelf.onValueChanged.AddListener(x => {
			this.enabled = x;
			SystemConfig.Instance.SaveData("UseKB", x);
		});
		TG_EnableSelf.isOn = SystemConfig.Instance.GetData<bool>("UseKB");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
			Tello.takeOff();
		} else if (Input.GetKeyDown(KeyCode.Alpha2)) {
			Tello.land();
		}

		float lx = 0f;			//旋轉
		float ly = 0f;			//上下
		float rx = 0f;			//左右
		float ry = 0f;			//前後

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
		//旋轉,上下,左右,前後
    }
}
