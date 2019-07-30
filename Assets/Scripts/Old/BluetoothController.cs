using UnityEngine;
using UnityEngine.UI;
using TechTweaking.Bluetooth;
using System.Collections;
using System.IO;

public class BluetoothController : MonoBehaviour {
  
  private const string UUID = "0acc9c7c-48e1-41d2-acaa-610d1a7b085e";
  private bool isConnected = false;
  private  BluetoothDevice device;

  public GameObject mainCanvas;
  public GameObject newFileCanvas;
  public InputField speechToText;
  public Text deviceNameText;
  public Text connectText;
  
  public Dropdown speechList;
  private int dropdownIndex = 0;
  public Dropdown fileList;
  private StreamWriter writer;
  private string currentFileName;
  private string path;
  
  private uint count = 0;
  private byte[] bytes;

  public string speech = "";
  public int volume = 100;
  public int pan = 90;
  public int tilt = 90;
  public int expression = 0;
  public int red = 169;
  public int green = 255;
  public int blue = 128;
  
  //initial bluetooth setup
	void Awake () {
    //initialize bluetooth
		BluetoothAdapter.enableBluetooth();//Force Enabling Bluetooth
    BluetoothAdapter.OnDeviceOFF += HandleOnDeviceOff;
    BluetoothAdapter.OnDevicePicked += HandleOnDevicePicked;
    BluetoothAdapter.OnClientRequest += HandleOnClientRequest;
    
    //populate DropDownFiles
    populateDropdownFiles();
	}
  
  //Android Bluetooth Direct Connection
  public void showDevices () { 
    BluetoothAdapter.showDevices ();
  }
  void HandleOnDeviceOff (BluetoothDevice dev) {
    if (!string.IsNullOrEmpty (dev.Name)) {
      deviceNameText.text = "Can't connect to " + dev.Name + ", device maybe OFF";
    } else if (!string.IsNullOrEmpty (dev.Name)) {
      deviceNameText.text = "Can't connect to " + dev.MacAddress + ", device maybe OFF";
    }
  }
  void HandleOnClientRequest (BluetoothDevice device) {
    this.device = device;
    this.device.setEndByte (10);
    this.device.connect();
  }
  void HandleOnDevicePicked (BluetoothDevice device) {
    this.device = device;
    if (!string.Equals(device.Name,"HC-06")) { this.device.UUID = UUID; }
    device.setEndByte (10);
    deviceNameText.text = device.Name;
  }
  void OnDestroy () {
    BluetoothAdapter.OnDevicePicked -= HandleOnDevicePicked; 
    BluetoothAdapter.OnClientRequest -= HandleOnClientRequest;
    BluetoothAdapter.OnDevicePicked -= HandleOnDevicePicked; 
  }

  //set bluetooth name
  public void setBluetoothName(string s) { if (device != null) { device.Name = s; } } 

  //connect button
  public void connect() { if (device != null) { device.connect(); } }
  public void disconnect() { if (device != null) { device.close(); } }

  //new file menu
  public void newFile() {
    mainCanvas.SetActive(false);
    newFileCanvas.SetActive(true);
  }
  public void setFileName(string s) { currentFileName = s + ".txt"; } 
  public void saveFile() {
    if(!string.IsNullOrEmpty(currentFileName)) {
      path = Application.persistentDataPath + "/" + currentFileName;
      speechList.options.Clear();
      saveSpeech();
    }
    mainCanvas.SetActive(true);
    newFileCanvas.SetActive(false);
    populateDropdownFiles();
    populateDropdownSpeech();
  }
  public void cancelSave() {
    mainCanvas.SetActive(true);
    newFileCanvas.SetActive(false);
    populateDropdownFiles();
    populateDropdownSpeech();
  }
  //select file
  public void setDropdownFile(int i) {
    path = Application.persistentDataPath + "/" + fileList.options[i].text;
    populateDropdownSpeech();
  }
  //populate dropdown files
  public void populateDropdownFiles() {
    DirectoryInfo directory = new DirectoryInfo(Application.persistentDataPath);
    fileList.options.Clear();
    FileInfo[] info = directory.GetFiles("*.txt"); 
    foreach(FileInfo file in info) {
      fileList.options.Add(new Dropdown.OptionData() {text = file.Name});
    }
  }
  
  //set speech
  public void setSpeech(string s) { speech = s; } 
  public void setDropdownSpeech(int i) { 
    dropdownIndex = i;
    string s = speechList.options[i].text;
    speechToText.text = s; speech = s; 
  } 
  public void addSpeech() { 
    speechList.options.Add(new Dropdown.OptionData() {text = speechToText.text});
    dropdownIndex = speechList.options.Count-1;
  } 
  public void removeSpeech() { 
    if(speechList.options.Count > 0 && 0 <= dropdownIndex && dropdownIndex < speechList.options.Count) {
      speechList.options.RemoveAt(dropdownIndex);
      dropdownIndex -= 1;
    }
  }
  public void saveSpeech() {
    if(path != null) {
      writer = new StreamWriter(path);
      for(int i = 0; i < speechList.options.Count; i++) {
        writer.WriteLine(speechList.options[i].text);
      }
      writer.Close();
    }
  }
  public void populateDropdownSpeech() {
    speechList.options.Clear();
    string[] loadedSpeech;
    if (File.Exists(path)) { 
      loadedSpeech = File.ReadAllLines(path); 
      foreach(string speech in loadedSpeech) {
        speechList.options.Add(new Dropdown.OptionData() {text = speech});
      }
    }
  }
  
  //set sliders
  public void setVolume(float v) { volume = (int)v; }
  public void setPan(float p) { pan = (int)p; }
  public void setTilt(float t) { tilt = (int)t; }

  //set RGB face color
  public void setColor(Color c) { 
    red = (int)(c.r*255); green = (int)(c.g*255); blue = (int)(c.b*255); 
  }
  
  //set toggle
  public void setNeutral(bool set)   { if(set){expression = 0;} }
  public void setSurprised(bool set) { if(set){expression = 1;} }
  public void setHappy(bool set)     { if(set){expression = 2;} }
  public void setSad(bool set)       { if(set){expression = 3;} }
  public void setConcerned(bool set) { if(set){expression = 4;} }

  //send data
  public void sendData() {
    if(device != null && device.IsConnected) {

      JSONObject json = new JSONObject();
      json.AddField("pan", pan);
      json.AddField("tilt", tilt);
      json.AddField("volume", volume);
      json.AddField("expression", expression);
      json.AddField("speech", speech);
      json.AddField("red", red);
      json.AddField("green", green);
      json.AddField("blue", blue);
      
      bytes = System.Text.Encoding.UTF8.GetBytes(json.ToString() + (char)10);
      device.send(bytes);
    }
  }

  //change connected status
  void Update() {
    if(device != null && device.IsConnected && !isConnected) {
      connectText.text = "Connected";
      isConnected = true;
    } 
    if(device == null || (!device.IsConnected && isConnected)) {
      connectText.text = "Connect";
      isConnected = false;
    }
  }

}
