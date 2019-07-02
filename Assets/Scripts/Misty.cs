using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Crosstales.RTVoice;
using Crosstales.RTVoice.Model;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

using CI.HttpClient;

public class Misty : MonoBehaviour {

    //Misty Variables
    public string IP = "192.168.1.4";
    public InputField IPField;
    //LED Variables
    public int red, green, blue;
    //Image Variables
    public string imagename;
    //Head Variables
    public int pitch, roll, yaw;
    //Track Variables
    public int left, right;
    public int linearVelocity, angularVelocity, timeMS;
    //Audio Variables
    public string audioname;
    private string audiopath;
    private string SLASH;
    public string ttsSpeech;
    public AudioSource ttsAudio;
    
    //--- Change IP Port Initialization ---//
    public void Start() {
        SLASH = (Application.platform == RuntimePlatform.Android)?"/":"\\";
        audiopath = Application.persistentDataPath + SLASH + "tts";

        IP = PlayerPrefs.GetString("UDPAddress", "127.0.0.1");
        if(IPField) { IPField.text = IP; }
    }
    public void setAddress(string address) { IP = address; PlayerPrefs.SetString("UDPAddress", address); }
    
    //--- Change LED ---//
    //requests.post('http://'+self.ip+'/api/led',json={"Red": red,"Green": green,"Blue": blue})
    public void ChangeLED(int r, int g, int b) { red = r; green = g; blue = b;
        newHttpClient("http://"+IP+"/api/led", "{Red:" + red + ",Green:" + green + ",Blue:" + blue + "}");
    }

    //--- Change Image ---//
    //Angry.jpg, Concerned.jpg, Confused.jpg, confused2.jpg, Content.jpg, Groggy.jpg, Happy.jpg, Love.jpg, Sad.jpg, Unamused.jpg, Waking.jpg;
    //requests.post('http://'+self.ip+'/api/images/display',json={'FileName': image_name ,'TimeOutSeconds': 5,'Alpha': 1})
    public void ChangeImage(string filename) { imagename = filename;
        newHttpClient("http://"+IP+"/api/images/display", "{Filename:\"" + imagename + "\",TimeOutSeconds:5,Alpha:1}");
    }

    //--- Play Audio ---//
    //requests.post('http://'+self.ip+'/api/audio/play',json={"AssetId": file_name})
    public void PlayAudio(string filename) { audioname = filename;
        newHttpClient("http://"+IP+"/api/audio/play", "{AssetId:\"" + audioname + "\"}");
    }

    //--- Save Audio ---//
    //requests.post('http://'+self.ip+'/api/audio',json={"FileName": "tts.wav", "DataAsByteArrayString": "34,88,90,49,56,...", "ImmediatelyApply": True, "OverwriteExisting": True})
    public void SaveAudio() { 
        string path = audiopath + ".wav";
        byte[] bytes = File.ReadAllBytes(path);
        int[] bytesAsInts = bytes.Select(x=>(int)x).ToArray();
        string bytesAsString = string.Join(",", Array.ConvertAll(bytesAsInts, x => x.ToString()));
        newHttpClient("http://"+IP+"/api/audio", "{Filename:\"tts.wav\", DataAsByteArrayString:\"" + bytesAsString + "\",ImmediatelyApply:\"True\",OverwriteExisting:\"True\"}");   
    }
    public void OnEnable() { Speaker.OnSpeakAudioGenerationComplete += onSpeakAudioGenerationComplete; }
    public void OnDisable() { Speaker.OnSpeakAudioGenerationComplete -= onSpeakAudioGenerationComplete; }
    private void onSpeakAudioGenerationComplete(Crosstales.RTVoice.Model.Wrapper wrapper) {
        SaveAudio(); Debug.Log("Speech generated: " + wrapper);
    }
    
    //--- Say TTS ---//
    //rate = 0-3, pitch = 0-2, volume = 0-1
    public void SayTTS(string speech, float rate = 1, float pitch = 1, bool speak = false) { ttsSpeech = speech;
        if(speech != null && speech.Length > 0) { 
            //Speaker.Generate(speech, "tts", null, rate, pitch, 1); 
            Speaker.Speak(speech, ttsAudio, null, speak, 1, 1, 1, audiopath); 
        }        
        Debug.Log("Saved speech: " + speech);   
    }

    //--- Move Head ---//
    //requests.post('http://'+self.ip+'/api/head',json={"Pitch": pitch, "Roll": roll, "Yaw": yaw, "Velocity": velocity})
    public void MoveHead(int p, int r, int y) { pitch = p; roll = r; yaw = y;
        newHttpClient("http://"+IP+"/api/head", "{Pitch:"+pitch+",Roll:"+roll+",Yaw:"+yaw+"}");
    }
    
    //--- Drive Track ---//
    //requests.post('http://'+self.ip+'/api/drive/track',json={"LeftTrackSpeed": left_track_speed,"RightTrackSpeed": right_track_speed})
    public void DriveTrack(int l, int r) { left = l; right = r;    
        newHttpClient("http://"+IP+"/api/drive/track", "{LeftTrackSpeed:\""+left+"\",RightTrackSpeed:\""+right+"\"}");
    }

    //--- Drive Time ---//
    //requests.post('http://'+self.ip+'/api/drive/track',json={"LeftTrackSpeed": left_track_speed,"RightTrackSpeed": right_track_speed})
    public void DriveTime(int l, int a, int t) { linearVelocity = l; angularVelocity = a; timeMS = t;  
        newHttpClient("http://"+IP+"/api/drive/time", "{LinearVelocity:\""+linearVelocity+"\",AngularVelocity:\""+angularVelocity+"\", TimeMS:\""+timeMS+"\"}");
    }
    
    //--- Stop ---//
    //requests.post('http://'+self.ip+'/api/drive/stop')
    public void Stop() {   
        newHttpClient("http://"+IP+"/api/drive/stop", "");
    }

    //HTTP API quick helper function
    public void newHttpClient(string URI, string json) {
        if(json != null && json.Length > 0) { Debug.Log(json); }    
        new HttpClient().Post(new System.Uri(URI), new StringContent(json), HttpCompletionOption.AllResponseContent, (response) => {
            #pragma warning disable 0219
            if(response != null) { 
                string responseData = response.ReadAsString(); 
                Debug.Log(responseData);
            }
            #pragma warning restore 0219
        });   
    }
}
