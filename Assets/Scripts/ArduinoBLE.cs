using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;

public class ArduinoBLE : MonoBehaviour {
	public string DeviceName = "JDY-16";
	public string ServiceUUID = "FFE0";
	public string Characteristic = "FFE1";

	public Text bluetoothStatus;
	
	enum States { None, Scan, Connect, Subscribe, Unsubscribe, Disconnect }

	private float _timeout = 0f;
	private States _state = States.None;
	private bool _connected = false;
	private bool _found = false;

	private int _scanCount = 0;

	public float sleep = 0.5f;
	private float sleeper = 0f;
  private string message = "";

	// this is our JDY-16 device
	private string _JDY16;

	void Reset () {
		_state = States.None;
		_connected = false;
		_found = false;
		_JDY16 = null;
		_timeout = 0f;
		_scanCount = 0;
	}

	void StartProcess () {
		bluetoothStatus.text = "Initializing...";

		Reset();
		BluetoothLEHardwareInterface.Initialize (true, false, () 
			=> { bluetoothStatus.text = "Initialized"; _state = States.Scan; } ,
			(error) => { BluetoothLEHardwareInterface.Log ("Error: " + error); });
	}

	// Use this for initialization
	void Start () {
		bluetoothStatus.text = "";
		StartProcess ();
	}
	
	// Update is called once per frame
	void Update () {

		if (_timeout > 0f) { _timeout -= Time.deltaTime; }
		
		else { _timeout = 0.5f;

			switch (_state) {
				case States.None: break;
				case States.Scan: scanForDevices(); break;
				case States.Connect: connectToDevice(); break;
				case States.Subscribe: subscribeToDevice(); break;
				case States.Unsubscribe: unsubscribeToDevice(); break;
				case States.Disconnect: disconnectToDevice(); break;
			}

		}

		if(sleeper > 0) { sleeper -= Time.deltaTime; }
		else { 
			sleeper = sleep; 
			SendString(message);
		}

	}

	void scanForDevices() {
		char bar = ' ';
	  switch(_scanCount % 4) { case 0: bar = '|'; break; case 1: bar = '/'; break; case 2: bar = '-'; break; case 3: bar = '\\'; break; }
	  bluetoothStatus.text = "Scanning... [" + bar + "]";
		_scanCount++;
		
		BluetoothLEHardwareInterface.ScanForPeripheralsWithServices (null, (address, name) => {
			if (name.Contains(DeviceName)) {
				BluetoothLEHardwareInterface.StopScan();
				bluetoothStatus.text = "Found device!";
				_state = States.Connect;
				_JDY16 = address;
				_found = true;
			}
		}, null, false, false);
	}

	void connectToDevice() {
		bluetoothStatus.text = "Connecting...";

		BluetoothLEHardwareInterface.ConnectToPeripheral (_JDY16, null, null, (address, serviceUUID, characteristicUUID) => {
			if (IsEqual (serviceUUID, ServiceUUID)) {
				if (IsEqual (characteristicUUID, Characteristic)) {
					_connected = true;
					_state = States.Subscribe;
					bluetoothStatus.text = "Connected!";
				}
			}
		}, (disconnectedAddress) => {
			BluetoothLEHardwareInterface.Log("Device disconnected: " + disconnectedAddress);
			bluetoothStatus.text = "Disconnected!";
			StartProcess();
		});
	}

  void subscribeToDevice() {
		bluetoothStatus.text = "Subscribing...";

		BluetoothLEHardwareInterface.SubscribeCharacteristicWithDeviceAddress (_JDY16, ServiceUUID, Characteristic, null, (address, characteristicUUID, bytes) => {
			bluetoothStatus.text = "" + Encoding.UTF8.GetString (bytes);
		});
		
		_state = States.None;
		bluetoothStatus.text = "All set!";
	}

	void unsubscribeToDevice() {
		BluetoothLEHardwareInterface.UnSubscribeCharacteristic(_JDY16, ServiceUUID, Characteristic, null);
		_state = States.Disconnect;
	}

  void disconnectToDevice() {
		if (_connected) {
			BluetoothLEHardwareInterface.DisconnectPeripheral (_JDY16, (address) => {
				BluetoothLEHardwareInterface.DeInitialize (() => { _connected = false; _state = States.None; });
			});
		} 
		else { BluetoothLEHardwareInterface.DeInitialize (() => { _state = States.None; }); }		
  }
	
	string FullUUID (string uuid) { return "0000" + uuid + "-0000-1000-8000-00805F9B34FB"; }
	bool IsEqual(string uuid1, string uuid2) {
		return (true);
		//if (uuid1.Length == 4) { uuid1 = FullUUID (uuid1); }
		//if (uuid2.Length == 4) { uuid2 = FullUUID (uuid2); }
		//return (uuid1.ToUpper().Equals(uuid2.ToUpper()));
	}

	public void sendMessage(string input) { message = input; }
	void SendString(string value) {
		if(_connected && value != null && value.Length > 0) {
			var data = Encoding.UTF8.GetBytes (value);
			BluetoothLEHardwareInterface.WriteCharacteristic (_JDY16, ServiceUUID, Characteristic, data, data.Length, false, (characteristicUUID) => { BluetoothLEHardwareInterface.Log ("Write Succeeded"); });
		}
	}
}
