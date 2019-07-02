using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TechTweaking.Bluetooth;

public class BluetoothConnection : MonoBehaviour {

  private const string UUID = "0acc9c7c-48e1-41d2-acaa-610d1a7b085e";

  private BluetoothDevice bluetoothDevice;
  private string receiverMessage = null;
  private bool isConnected = false;
  public bool isServer = false;
  public Text deviceNameText;
  public Text connectText;
 
  public void showBluetoothDevices () { BluetoothAdapter.showDevices(); }
  public void setBluetoothName(string s) { if (bluetoothDevice != null) { bluetoothDevice.Name = s; } } 
  
  public void disconnectBluetooth() { if (bluetoothDevice != null) { bluetoothDevice.close(); } }
  public void connectBluetooth() { if (bluetoothDevice != null) { bluetoothDevice.connect(); } }
  public void connectOrDisconnectBluetooth() { 
    if(!isConnected) { connectBluetooth(); }
    else { disconnectBluetooth(); } 
  } 

  public void sendMessage(string data) {
    if(bluetoothDevice != null && bluetoothDevice.IsConnected) {
      byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data + (char)10);
      bluetoothDevice.send(bytes);
    }
  }
  public string getMessage() {
    if (receiverMessage != null) {
      string message = receiverMessage;
      receiverMessage = null;
      return message;
    } else {
      return null;
    }
  }

  void Awake () {
    BluetoothAdapter.enableBluetooth(); //Force Enabling Bluetooth
    BluetoothAdapter.OnDeviceOFF += HandleOnDeviceOff;
    BluetoothAdapter.OnDevicePicked += HandleOnDevicePicked;
    BluetoothAdapter.OnClientRequest += HandleOnClientRequest;
    if(isServer) { BluetoothAdapter.startServer(UUID, 1000); }
  }
  void Update() {
    if(bluetoothDevice != null && bluetoothDevice.IsConnected && !isConnected) {
      if(connectText != null) { 
        connectText.text = isServer ? "Bluetooth connected" : "Connected"; 
      } 
      isConnected = true;
    } 
    if(bluetoothDevice == null || (!bluetoothDevice.IsConnected && isConnected)) {
      if(connectText != null) { 
        connectText.text = isServer ? "Bluetooth not connected" : "Not Connected"; 
      }
      isConnected = false;
    }
  }
  void OnDestroy () {
    BluetoothAdapter.OnDevicePicked -= HandleOnDevicePicked; 
    BluetoothAdapter.OnClientRequest -= HandleOnClientRequest;
  }
  void HandleOnDeviceOff (BluetoothDevice device) {
    if (!string.IsNullOrEmpty(device.Name)) {
      string msg = "Can't connect to " + device.Name + ", device maybe OFF";
      if(deviceNameText != null) { deviceNameText.text = msg; } 
      Debug.LogWarning(msg);
    } else if (!string.IsNullOrEmpty(device.Name)) {
      string msg = "Can't connect to " + device.MacAddress + ", device maybe OFF";
      if(deviceNameText != null) { deviceNameText.text = msg; } 
      Debug.LogWarning(msg);
    }
  }
  void HandleOnClientRequest (BluetoothDevice device) {
    bluetoothDevice = device;
    bluetoothDevice.setEndByte(10);
    if(isServer) { bluetoothDevice.ReadingCoroutine = ManageConnection; }
    bluetoothDevice.connect();
  }
  void HandleOnDevicePicked (BluetoothDevice device) {
    disconnectBluetooth();
    bluetoothDevice = device;
    bluetoothDevice.UUID = UUID; 
    bluetoothDevice.setEndByte(10);
    if(isServer) { bluetoothDevice.ReadingCoroutine = ManageConnection; }
    if(deviceNameText != null) { deviceNameText.text = bluetoothDevice.Name; }
    if(!isServer) { connectBluetooth(); }
  }
  IEnumerator ManageConnection(BluetoothDevice device) {
    while (device.IsReading) {
      if (device.IsDataAvailable) {
        byte[] msg = device.read();
        if (msg != null && msg.Length > 0) {
          receiverMessage = System.Text.ASCIIEncoding.ASCII.GetString(msg);
        }
      }
      yield return null;
    }
  }
}