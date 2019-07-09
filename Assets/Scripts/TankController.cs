using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System.IO;
using System.Collections;

using TechTweaking.Bluetooth;

public class TankController : MonoBehaviour {

  public enum SendMode { SERIAL, BLUETOOTH, UDP, NONE, MISTY };
  public SendMode sendMode;

  public SimpleInputNamespace.Joystick joystick;
  public GameObject joystickUI;

  public GameObject serialPortObject;
  public SerialPortUtility.SerialPortUtilityPro serialPort = null;
  public BluetoothConnection bluetoothSender;
  public UDPSender udpSender;
  public Misty misty;

  public float remoteReceiveTimer = 0;
  public float remoteReceiveTime = 1;
  public float remoteX, remoteY = 0;

  public float serialTimer = 2f;
  public float serialTime = 0.3f;

  public void Update() {
    if(remoteReceiveTimer > 0) { remoteReceiveTimer -= Time.deltaTime; }

    //Update tank every time serial is ready to push another message
    if(serialTimer > 0) { serialTimer -= Time.deltaTime; }
    else { serialTimer = serialTime; 
      //If USB Serial, let incoming Bluetooth/UDP messages override joystick
      if(sendMode == SendMode.SERIAL) {
        if(remoteReceiveTimer > 0) { controlTank(remoteX, remoteY); } 
        else {controlTank(joystick.GetX(), joystick.GetY());}
      } 
      //If Bluetooth or UDP, send data from Joystick to face device.
      else if(sendMode == SendMode.BLUETOOTH || sendMode == SendMode.UDP) {
        controlTank(joystick.GetX(), joystick.GetY()); 
      } 
      //If Misty, send data from Joystick to Misty device.
      else if(sendMode == SendMode.MISTY) {
        if(new Vector2(joystick.GetX(),joystick.GetY()).magnitude > 0.05f) {
          misty.DriveTime((int)(joystick.GetY() * 100f), -(int)(joystick.GetX() * 100f), 1000);
        } else {
          misty.Stop();
        }
      }
    }
  }
  
  public void setSendMode(int mode) {
    switch(mode) {
      case 0: sendMode = SendMode.NONE; break;
      case 1: sendMode = SendMode.UDP; break;
      case 2: sendMode = SendMode.BLUETOOTH; break;
      case 3: sendMode = SendMode.SERIAL; if(serialPortObject) { serialPortObject.SetActive(true); } break;
      case 4: sendMode = SendMode.MISTY; break;
      default: sendMode = SendMode.NONE; break;
    }
  }

  public void remoteControlTank(float horizontal, float vertical) {
    remoteReceiveTimer = remoteReceiveTime;
    remoteX = horizontal; remoteY = vertical;
  }

  public void controlTank(float horizontal, float vertical) {
    JSONObject json = new JSONObject();

    if(sendMode == SendMode.SERIAL) {
      int left = (int)((vertical + horizontal) * 255);
      int right = (int)((vertical - horizontal) * 255);
      if(left > 255) { left = 255; } if(left < -255) { left = -255; }
      if(right > 255) { right = 255; } if(right < -255) { right = -255; }
      json.AddField("L", left); json.AddField("R", right);
      if(serialPort) { serialPort.Write(json.ToString() + "\r\n"); }
    } else {
      json.AddField("x", (int)(horizontal*100) ); json.AddField("y", (int)(vertical*100) );
      if(sendMode == SendMode.BLUETOOTH) { if(bluetoothSender) { bluetoothSender.sendMessage("t" + json.ToString() + "\r\n"); } }
      else if(sendMode == SendMode.UDP) { if(udpSender) { udpSender.sendMessage("t" + json.ToString() + "\r\n"); } }
    }
  }

  public void showHideJoystick() { joystickUI.SetActive(!joystickUI.activeSelf); } 

}


