using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowLocalIP : MonoBehaviour {

  public Text IPtext;
  private float checkTimer = 0;
  public float checkTime = 10;

	// Update is called once per frame
	void Update () {
    if(checkTimer >= 0) {checkTimer -= Time.deltaTime;}
    else {checkTimer = checkTime; IPtext.text = LocalIPAddress();}
	}

  //Helper Function gets local IP Address
  public string LocalIPAddress() {
    IPHostEntry host;
    string localIP = "No local IP found";
    host = Dns.GetHostEntry(Dns.GetHostName());
    foreach (IPAddress ip in host.AddressList) {
        if (ip.AddressFamily == AddressFamily.InterNetwork) { localIP = ip.ToString(); break; }
    }
    return localIP;
  }
}
