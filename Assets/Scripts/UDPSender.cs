using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Net;

public class UDPSender : MonoBehaviour {
   
    private string senderMessage = null;
    public string IP = "127.0.0.1";
    public int senderPort;
    public bool autoStart = false;

    UdpClient sender;
    Thread senderThread;
    public int sleep = 50;
    bool sendingData = true;
    
    public Text ConnectButton;
    public InputField IPField;
    public InputField PortField;

    public void sendMessage(string message) { senderMessage = message; }
    public void setAddress(string address) { IP = address; PlayerPrefs.SetString("UDPAddress", address); }
    public void setPort(int port) { senderPort = port; PlayerPrefs.SetInt("UDPPort", senderPort); }
    public void setPort(string port) { 
        if(!int.TryParse(port, out senderPort)) { senderPort = 0; } 
        PlayerPrefs.SetInt("UDPPort", senderPort);
    }
    public void startSenderThread() {
        stopSenderThread();
        if(IP != null && IP.Length > 0 && senderPort > 0) {
            sendingData = true;
            if (senderThread != null && senderThread.IsAlive) { senderThread.Abort(); }
            senderThread = new Thread(new ThreadStart(SendData));
            senderThread.IsBackground = true;
            senderThread.Start();
            Debug.Log("Sender Started!");
            if(ConnectButton) {ConnectButton.text = "Restart";}
        }
    }
    public void stopSenderThread() {
        if (sender != null) { sender.Close(); }
        sendingData = false;
    }

    private void SendData() {
        while (sendingData) {
            if (senderMessage != null && senderMessage.Length > 0) {
                sender = new UdpClient();
                IPEndPoint sendEndPoint = new IPEndPoint(IPAddress.Parse(IP), senderPort);
                Debug.Log(senderMessage);
                byte[] data = Encoding.UTF8.GetBytes(senderMessage);
                sender.Send(data, data.Length, sendEndPoint);
                senderMessage = null;
                sender.Close();
            } 
            Thread.Sleep(sleep);
        }
    }

    void Start() {
        IP = PlayerPrefs.GetString("UDPAddress", "127.0.0.1");
        senderPort = PlayerPrefs.GetInt("UDPPort", 9000);
        if(IPField) { IPField.text = IP; } if(PortField) { PortField.text = "" + senderPort; }
        if(autoStart && IP != null && IP.Length > 0 && senderPort > 0) { startSenderThread(); }
    }
    void OnApplicationQuit() {
        Debug.Log("Closed application.");
        stopSenderThread();
    }
}