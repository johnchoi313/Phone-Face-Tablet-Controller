using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Chatlog : MonoBehaviour {

	public Text chatlog;
	private string SLASH;
	public GameObject UI;

	public void userSpoke(string input) { chatlog.text += "USER: input\r\n\r\n"; }
	public void robotSpoke(string input) { chatlog.text += "ROBOT: input\r\n\r\n"; }
	
	public void showHide() { UI.SetActive(!UI.activeSelf); }

	public void Start() {
		SLASH = (Application.platform == RuntimePlatform.Android)?"/":"\\";
	}

	public void saveChatlog(string path) {
		StreamWriter writer = new StreamWriter(path, false); //true to append, false to overwrite
        writer.WriteLine(chatlog.text);
    	writer.Close();
	}

	void OnApplicationQuit() {
		if(chatlog.text != null && chatlog.text.Length > 0) {
			string filename = "CHAT " + System.DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss")+".txt";
			string path = Application.persistentDataPath + SLASH + "Chatlogs" + SLASH + filename;
			//string path = Application.streamingAssetsPath + SLASH + "Chatlogs" + SLASH + filename;
			saveChatlog(path);
		}
	}
}
