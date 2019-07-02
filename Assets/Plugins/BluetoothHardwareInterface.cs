using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

#if UNITY_2018_3_OR_NEWER
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
#endif

public class BluetoothLEHardwareInterface
{
	public enum CBCharacteristicProperties
	{
		CBCharacteristicPropertyBroadcast = 0x01,
		CBCharacteristicPropertyRead = 0x02,
		CBCharacteristicPropertyWriteWithoutResponse = 0x04,
		CBCharacteristicPropertyWrite = 0x08,
		CBCharacteristicPropertyNotify = 0x10,
		CBCharacteristicPropertyIndicate = 0x20,
		CBCharacteristicPropertyAuthenticatedSignedWrites = 0x40,
		CBCharacteristicPropertyExtendedProperties = 0x80,
		CBCharacteristicPropertyNotifyEncryptionRequired = 0x100,
		CBCharacteristicPropertyIndicateEncryptionRequired = 0x200,
	};

	public enum ScanMode
	{
		LowPower = 0,
		Balanced = 1,
		LowLatency = 2
	}

	public enum ConnectionPriority
	{
		LowPower = 0,
		Balanced = 1,
		High = 2,
	}

	public enum iOSProximity
	{
		Unknown = 0,
		Immediate = 1,
		Near = 2,
		Far = 3,
	}

	public struct iBeaconData
	{
		public string UUID;
		public int Major;
		public int Minor;
		public int RSSI;
		public int AndroidSignalPower;
		public iOSProximity iOSProximity;
	}

#if UNITY_ANDROID
	public enum CBAttributePermissions
	{
		CBAttributePermissionsReadable = 0x01,
		CBAttributePermissionsWriteable = 0x10,
		CBAttributePermissionsReadEncryptionRequired = 0x02,
		CBAttributePermissionsWriteEncryptionRequired = 0x20,
	};
#else
	public  enum CBAttributePermissions
	{
		CBAttributePermissionsReadable = 0x01,
		CBAttributePermissionsWriteable = 0x02,
		CBAttributePermissionsReadEncryptionRequired = 0x04,
		CBAttributePermissionsWriteEncryptionRequired = 0x08,
	};
#endif

#if UNITY_IPHONE || UNITY_TVOS
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLELog (string message);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEInitialize (bool asCentral, bool asPeripheral);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEDeInitialize ();
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEPauseMessages (bool isPaused);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEScanForPeripheralsWithServices (string serviceUUIDsString, bool allowDuplicates, bool rssiOnly, bool clearPeripheralList);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERetrieveListOfPeripheralsWithServices (string serviceUUIDsString);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEStopScan ();

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEConnectToPeripheral (string name);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEDisconnectPeripheral (string name);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEReadCharacteristic (string name, string service, string characteristic);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEWriteCharacteristic (string name, string service, string characteristic, byte[] data, int length, bool withResponse);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLESubscribeCharacteristic (string name, string service, string characteristic);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEUnSubscribeCharacteristic (string name, string service, string characteristic);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEDisconnectAll ();

#if !UNITY_TVOS
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEScanForBeacons (string proximityUUIDsString);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEStopBeaconScan ();

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEPeripheralName (string newName);

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLECreateService (string uuid, bool primary);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERemoveService (string uuid);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERemoveServices ();

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLECreateCharacteristic (string uuid, int properties, int permissions, byte[] data, int length);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERemoveCharacteristic (string uuid);
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLERemoveCharacteristics ();

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEStartAdvertising ();
	
	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEStopAdvertising ();

	[DllImport ("__Internal")]
	private static extern void _iOSBluetoothLEUpdateCharacteristicValue (string uuid, byte[] data, int length);
#endif
#elif UNITY_ANDROID
	static AndroidJavaObject _android = null;
#endif


	private static BluetoothDeviceScript bluetoothDeviceScript;

	public static void Log (string message)
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLELog (message);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothLog", message);
#endif
		}
	}

	public static BluetoothDeviceScript Initialize (bool asCentral, bool asPeripheral, Action action, Action<string> errorAction)
	{
		bluetoothDeviceScript = null;

#if UNITY_2018_3_OR_NEWER
#if UNITY_ANDROID
		if (!Permission.HasUserAuthorizedPermission (Permission.CoarseLocation))
			Permission.RequestUserPermission (Permission.CoarseLocation);
#endif
#endif

		GameObject bluetoothLEReceiver = GameObject.Find("BluetoothLEReceiver");
		if (bluetoothLEReceiver == null)
		{
			bluetoothLEReceiver = new GameObject("BluetoothLEReceiver");

			bluetoothDeviceScript = bluetoothLEReceiver.AddComponent<BluetoothDeviceScript>();
			if (bluetoothDeviceScript != null)
			{
				bluetoothDeviceScript.InitializedAction = action;
				bluetoothDeviceScript.ErrorAction = errorAction;
			}
		}

		GameObject.DontDestroyOnLoad (bluetoothLEReceiver);

		if (Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.SendMessage ("OnBluetoothMessage", "Initialized");
		}
		else
		{
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEInitialize (asCentral, asPeripheral);
#elif UNITY_ANDROID
			if (_android == null)
			{
				AndroidJavaClass javaClass = new AndroidJavaClass ("com.shatalmic.unityandroidbluetoothlelib.UnityBluetoothLE");
				_android = javaClass.CallStatic<AndroidJavaObject> ("getInstance");
			}

			if (_android != null)
				_android.Call ("androidBluetoothInitialize", asCentral, asPeripheral);
#endif
		}

		return bluetoothDeviceScript;
	}
	
	public static void DeInitialize (Action action)
	{
		if (bluetoothDeviceScript != null)
			bluetoothDeviceScript.DeinitializedAction = action;

		if (Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.SendMessage ("OnBluetoothMessage", "DeInitialized");
		}
		else
		{
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEDeInitialize ();
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothDeInitialize");
#endif
		}
	}

	public static void FinishDeInitialize ()
	{
		GameObject bluetoothLEReceiver = GameObject.Find("BluetoothLEReceiver");
		if (bluetoothLEReceiver != null)
			GameObject.Destroy(bluetoothLEReceiver);
	}

	public static void BluetoothEnable (bool enable)
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE || UNITY_TVOS
			//_iOSBluetoothLELog (message);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothEnable", enable);
#endif
		}
	}

	public static void BluetoothScanMode (ScanMode scanMode)
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE || UNITY_TVOS
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothScanMode", (int)scanMode);
#endif
		}
	}

	public static void BluetoothConnectionPriority (ConnectionPriority connectionPriority)
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE || UNITY_TVOS
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothConnectionPriority", (int)connectionPriority);
#endif
		}
	}

	public static void PauseMessages (bool isPaused)
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEPauseMessages (isPaused);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothPause", isPaused);
#endif
		}
	}

	// scanning for beacons requires that you know the Proximity UUID
	public static void ScanForBeacons (string[] proximityUUIDs, Action<iBeaconData> actionBeaconResponse)
	{
		if (proximityUUIDs != null && proximityUUIDs.Length >= 0)
		{
			if (!Application.isEditor)
			{
				if (bluetoothDeviceScript != null)
					bluetoothDeviceScript.DiscoveredBeaconAction = actionBeaconResponse;

				string proximityUUIDsString = null;

				if (proximityUUIDs != null && proximityUUIDs.Length > 0)
				{
					proximityUUIDsString = "";

					foreach (string proximityUUID in proximityUUIDs)
						proximityUUIDsString += proximityUUID + "|";

					proximityUUIDsString = proximityUUIDsString.Substring (0, proximityUUIDsString.Length - 1);
				}

#if UNITY_IPHONE
				_iOSBluetoothLEScanForBeacons (proximityUUIDsString);
#elif UNITY_ANDROID
				if (_android != null)
					_android.Call ("androidBluetoothScanForBeacons", proximityUUIDsString);
#endif
			}
		}
	}

	public static void ScanForPeripheralsWithServices (string[] serviceUUIDs, Action<string, string> action, Action<string, string, int, byte[]> actionAdvertisingInfo = null, bool rssiOnly = false, bool clearPeripheralList = true, int recordType = 0xFF)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
			{
				bluetoothDeviceScript.DiscoveredPeripheralAction = action;
				bluetoothDeviceScript.DiscoveredPeripheralWithAdvertisingInfoAction = actionAdvertisingInfo;

				if (bluetoothDeviceScript.DiscoveredDeviceList != null)
					bluetoothDeviceScript.DiscoveredDeviceList.Clear ();
			}

			string serviceUUIDsString = null;

			if (serviceUUIDs != null && serviceUUIDs.Length > 0)
			{
				serviceUUIDsString = "";

				foreach (string serviceUUID in serviceUUIDs)
					serviceUUIDsString += serviceUUID + "|";

				serviceUUIDsString = serviceUUIDsString.Substring (0, serviceUUIDsString.Length - 1);
			}

#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEScanForPeripheralsWithServices (serviceUUIDsString, (actionAdvertisingInfo != null), rssiOnly, clearPeripheralList);
#elif UNITY_ANDROID
			if (_android != null)
			{
				if (serviceUUIDsString == null)
					serviceUUIDsString = "";

				_android.Call ("androidBluetoothScanForPeripheralsWithServices", serviceUUIDsString, rssiOnly, recordType);
			}
#endif
		}
	}
	
	public static void RetrieveListOfPeripheralsWithServices (string[] serviceUUIDs, Action<string, string> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
			{
				bluetoothDeviceScript.RetrievedConnectedPeripheralAction = action;
				
				if (bluetoothDeviceScript.DiscoveredDeviceList != null)
					bluetoothDeviceScript.DiscoveredDeviceList.Clear ();
			}
			
			string serviceUUIDsString = serviceUUIDs.Length > 0 ? "" : null;
			
			foreach (string serviceUUID in serviceUUIDs)
				serviceUUIDsString += serviceUUID + "|";
			
			// strip the last delimeter
			serviceUUIDsString = serviceUUIDsString.Substring (0, serviceUUIDsString.Length - 1);
			
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLERetrieveListOfPeripheralsWithServices (serviceUUIDsString);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothRetrieveListOfPeripheralsWithServices", serviceUUIDsString);
#endif
		}
	}

	public static void StopScan ()
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEStopScan ();
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothStopScan");
#endif
		}
	}

	public static void StopBeaconScan ()
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE
			_iOSBluetoothLEStopBeaconScan ();
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothStopBeaconScan");
#endif
		}
	}

	public static void DisconnectAll ()
	{
		if (!Application.isEditor) {
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEDisconnectAll ();
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothDisconnectAll");
#endif
		}
	}

	public static void ConnectToPeripheral (string name, Action<string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string> disconnectAction = null)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
			{
				bluetoothDeviceScript.ConnectedPeripheralAction = connectAction;
				bluetoothDeviceScript.DiscoveredServiceAction = serviceAction;
				bluetoothDeviceScript.DiscoveredCharacteristicAction = characteristicAction;
				bluetoothDeviceScript.ConnectedDisconnectPeripheralAction = disconnectAction;
			}

#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEConnectToPeripheral (name);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothConnectToPeripheral", name);
#endif
		}
	}
	
	public static void DisconnectPeripheral (string name, Action<string> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.DisconnectedPeripheralAction = action;
			
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEDisconnectPeripheral (name);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidBluetoothDisconnectPeripheral", name);
#endif
		}
	}

	public static void ReadCharacteristic (string name, string service, string characteristic, Action<string, byte[]> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
			{
				if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateCharacteristicValueAction[name] = new Dictionary<string, Action<string, byte[]>>();

#if UNITY_IPHONE || UNITY_TVOS
				bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [characteristic] = action;
#elif UNITY_ANDROID
				bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [FullUUID (characteristic).ToLower ()] = action;
#endif
			}

#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEReadCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidReadCharacteristic", name, service, characteristic);
#endif
		}
	}
	
	public static void WriteCharacteristic (string name, string service, string characteristic, byte[] data, int length, bool withResponse, Action<string> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.DidWriteCharacteristicAction = action;
			
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEWriteCharacteristic (name, service, characteristic, data, length, withResponse);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidWriteCharacteristic", name, service, characteristic, data, length, withResponse);
#endif
		}
	}
	
	public static void SubscribeCharacteristic (string name, string service, string characteristic, Action<string> notificationAction, Action<string, byte[]> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
			{
				name = name.ToUpper ();
				service = service.ToUpper ();
				characteristic = characteristic.ToUpper ();
				
#if UNITY_IPHONE || UNITY_TVOS
				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] = new Dictionary<string, Action<string>> ();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] [characteristic] = notificationAction;

				if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] = new Dictionary<string, Action<string, byte[]>> ();
				bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [characteristic] = action;
#elif UNITY_ANDROID
				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] = new Dictionary<string, Action<string>> ();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction [name] [FullUUID (characteristic).ToLower ()] = notificationAction;

				if (!bluetoothDeviceScript.DidUpdateCharacteristicValueAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] = new Dictionary<string, Action<string, byte[]>> ();
				bluetoothDeviceScript.DidUpdateCharacteristicValueAction [name] [FullUUID (characteristic).ToLower ()] = action;
#endif
			}

#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLESubscribeCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidSubscribeCharacteristic", name, service, characteristic);
#endif
		}
	}
	
	public static void SubscribeCharacteristicWithDeviceAddress (string name, string service, string characteristic, Action<string, string> notificationAction, Action<string, string, byte[]> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
			{
				name = name.ToUpper ();
				service = service.ToUpper ();
				characteristic = characteristic.ToUpper ();

#if UNITY_IPHONE || UNITY_TVOS
				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][characteristic] = notificationAction;

				if (!bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string, byte[]>>();
				bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name][characteristic] = action;
#elif UNITY_ANDROID
				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][FullUUID (characteristic).ToLower ()] = notificationAction;
				
				if (!bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string, byte[]>>();
				bluetoothDeviceScript.DidUpdateCharacteristicValueWithDeviceAddressAction[name][FullUUID (characteristic).ToLower ()] = action;
#endif
			}
			
#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLESubscribeCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidSubscribeCharacteristic", name, service, characteristic);
#endif
		}
	}

	public static void UnSubscribeCharacteristic (string name, string service, string characteristic, Action<string> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
			{
				name = name.ToUpper ();
				service = service.ToUpper ();
				characteristic = characteristic.ToUpper ();
				
#if UNITY_IPHONE || UNITY_TVOS
				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][characteristic] = null;

				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] = new Dictionary<string, Action<string>> ();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][characteristic] = null;
#elif UNITY_ANDROID
				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name] = new Dictionary<string, Action<string, string>>();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicWithDeviceAddressAction[name][FullUUID (characteristic).ToLower ()] = null;
				
				if (!bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction.ContainsKey (name))
					bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name] = new Dictionary<string, Action<string>> ();
				bluetoothDeviceScript.DidUpdateNotificationStateForCharacteristicAction[name][FullUUID (characteristic).ToLower ()] = null;
#endif
			}

#if UNITY_IPHONE || UNITY_TVOS
			_iOSBluetoothLEUnSubscribeCharacteristic (name, service, characteristic);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidUnsubscribeCharacteristic", name, service, characteristic);
#endif
		}
	}

	public static void PeripheralName (string newName)
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE
			_iOSBluetoothLEPeripheralName (newName);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidPeripheralName", newName);
#endif
		}
	}

	public static void CreateService (string uuid, bool primary, Action<string> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.ServiceAddedAction = action;

#if UNITY_IPHONE
			_iOSBluetoothLECreateService (uuid, primary);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidCreateService", uuid, primary);
#endif
		}
	}
	
	public static void RemoveService (string uuid)
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE
			_iOSBluetoothLERemoveService (uuid);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidRemoveService", uuid);
#endif
		}
	}

	public static void RemoveServices ()
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE
			_iOSBluetoothLERemoveServices ();
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidRemoveServices");
#endif
		}
	}

	public static void CreateCharacteristic (string uuid, CBCharacteristicProperties properties, CBAttributePermissions permissions, byte[] data, int length, Action<string, byte[]> action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.PeripheralReceivedWriteDataAction = action;

#if UNITY_IPHONE
			_iOSBluetoothLECreateCharacteristic (uuid, (int)properties, (int)permissions, data, length);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidCreateCharacteristic", uuid, (int)properties, (int)permissions, data, length);
#endif
		}
	}

	public static void RemoveCharacteristic (string uuid)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.PeripheralReceivedWriteDataAction = null;

#if UNITY_IPHONE
			_iOSBluetoothLERemoveCharacteristic (uuid);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidRemoveCharacteristic", uuid);
#endif
		}
	}

	public static void RemoveCharacteristics ()
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE
			_iOSBluetoothLERemoveCharacteristics ();
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidRemoveCharacteristics");
#endif
		}
	}
	
	public static void StartAdvertising (Action action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.StartedAdvertisingAction = action;

#if UNITY_IPHONE
			_iOSBluetoothLEStartAdvertising ();
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidStartAdvertising");
#endif
		}
	}
	
	public static void StopAdvertising (Action action)
	{
		if (!Application.isEditor)
		{
			if (bluetoothDeviceScript != null)
				bluetoothDeviceScript.StoppedAdvertisingAction = action;

#if UNITY_IPHONE
			_iOSBluetoothLEStopAdvertising ();
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidStopAdvertising");
#endif
		}
	}
	
	public static void UpdateCharacteristicValue (string uuid, byte[] data, int length)
	{
		if (!Application.isEditor)
		{
#if UNITY_IPHONE
			_iOSBluetoothLEUpdateCharacteristicValue (uuid, data, length);
#elif UNITY_ANDROID
			if (_android != null)
				_android.Call ("androidUpdateCharacteristicValue", uuid, data, length);
#endif
		}
	}
	
	public static string FullUUID (string uuid)
	{
		if (uuid.Length == 4)
			return "0000" + uuid + "-0000-1000-8000-00805f9b34fb";
		return uuid;
	}
}
