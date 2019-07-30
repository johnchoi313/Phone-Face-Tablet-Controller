using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
    //Arm Variables
    public int leftArm, rightArm;
    //Track Variables
    public int left, right;
    public int linearVelocity, angularVelocity, timeMS;
    //Audio Variables
    public string audioname;
    private string audiopath;
    private string SLASH;
    public string ttsSpeech;
    
    //--- Change IP Port Initialization ---//
    public void Start() {
        SLASH = (Application.platform == RuntimePlatform.Android)?"/":"\\";
        audiopath = Application.persistentDataPath + SLASH;
        Debug.Log("Local audio path: " + audiopath);

        IP = PlayerPrefs.GetString("UDPAddress", "127.0.0.1");
        if(IPField) { IPField.text = IP; }
    }
    public void setAddress(string address) { IP = address; PlayerPrefs.SetString("UDPAddress", address); }
    
    //--- Change LED ---// (red = 0-255 | green = 0-255 | blue = 0-255)
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
        newHttpClient("http://"+IP+"/api/audio/play", "{AssetId:\"" + audioname + "\",Volume:100}");
    }

    //--- Save Audio ---//
    //requests.post('http://'+self.ip+'/api/audio',json={"FileName": "tts.wav", "Data": "34,88,90,49,56,...", "ImmediatelyApply": True, "OverwriteExisting": True})
    public void SaveAudio(string filename) { audioname = filename;
        //Old method
        string path = audiopath + "tts.wav";
        byte[] bytes = File.ReadAllBytes(path);
        int[] bytesAsInts = bytes.Select(x=>(int)x).ToArray();
        string bytesAsString = string.Join(",", Array.ConvertAll(bytesAsInts, x => x.ToString()));
        //New method
        string base64String = Convert.ToBase64String(bytes);
        //Save data as text to see
        StreamWriter writer = new StreamWriter(audiopath + "tts.txt", false); //true to append, false to overwrite
        writer.WriteLine(base64String); 
        writer.Close();
        //Send it over!
        newHttpClient("http://"+IP+"/api/audio", "{FileName:\"" + filename + "\", Data:\"" + base64String + "\",ImmediatelyApply:true, OverwriteExisting:true}");   
    }
    
    //--- Say TTS (Unity Android Only) ---//
    //rate = 0-3, pitch = 0-2, volume = 0-1
    public void SayTTS(string speech, float rate = 1, float pitch = 1, bool speak = false) { ttsSpeech = speech;
        audioname = Regex.Replace(speech,"[^A-Za-z0-9]","").ToLower() + ".wav";
        PlayAudio(audioname); 
    }

    //--- Save TTS (Unity Android Only) ---//
    public void SaveTTS(string speech, float rate = 1, float pitch = 1, bool speak = false) { ttsSpeech = speech;
        if(speech != null && speech.Length > 0) { 
            Speaker.Speak(speech, null, null, speak, rate, pitch, 1, audiopath + "tts"); //rate = 0-3, pitch = 0-2, volume = 0-1
            audioname = Regex.Replace(speech,"[^A-Za-z0-9]","").ToLower() + ".wav";
        }        
        Debug.Log("Saved speech: " + speech);   
    }
    public void OnEnable() { Speaker.OnSpeakAudioGenerationComplete += onSpeakAudioGenerationComplete; }
    public void OnDisable() { Speaker.OnSpeakAudioGenerationComplete -= onSpeakAudioGenerationComplete; }
    private void onSpeakAudioGenerationComplete(Crosstales.RTVoice.Model.Wrapper wrapper) {
        Debug.Log("Speech generated: " + wrapper);
        SaveAudio(audioname); 
    }

    //--- Move Head ---// (pitch = -9.5 to 34.9 | roll = -43 to 43 | yaw = -90 to 90)
    //requests.post('http://'+self.ip+'/api/head',json={"Pitch": pitch, "Roll": roll, "Yaw": yaw, "Velocity": velocity})
    public void MoveHead(int p, int r, int y, int v = 50) { pitch = p; roll = r; yaw = y;
        newHttpClient("http://"+IP+"/api/head", "{Pitch:"+pitch+",Roll:"+roll+",Yaw:"+yaw+",Velocity:"+v+"}");
    }
    
    //--- Move Arm ---// (leftArmPosition = -180 to 0 | leftArmPosition = -180 to 0)
    //requests.post('http://'+self.ip+'/api/head',json={"Pitch": pitch, "Roll": roll, "Yaw": yaw, "Velocity": velocity})
    public void MoveArms(int l, int r, int v = 50) { leftArm = l; rightArm = r;
        newHttpClient("http://"+IP+"/api/arms/set", "{LeftArmPosition:"+leftArm+",RightArmPosition:"+rightArm+",LeftArmVelocity:"+v+",RightArmVelocity:"+v+"}");
    }
    
    //--- Drive Track ---// (left = -100 to 100 | right = -100 to 100)
    //requests.post('http://'+self.ip+'/api/drive/track',json={"LeftTrackSpeed": left_track_speed,"RightTrackSpeed": right_track_speed})
    public void DriveTrack(int l, int r) { left = l; right = r;    
        newHttpClient("http://"+IP+"/api/drive/track", "{LeftTrackSpeed:\""+left+"\",RightTrackSpeed:\""+right+"\"}");
    }

    //--- Drive Time ---// (left = -100 to 100 | right = -100 to 100)
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
        if(json != null && json.Length > 0) { Debug.Log("JSON sent: " + json); }    
        new HttpClient().Post(new System.Uri(URI), new StringContent(json), HttpCompletionOption.AllResponseContent, (response) => {
            //#pragma warning disable 0219
            if(response != null) { 
                string responseData = (response != null) ? response.ReadAsString() : "No response from HTTP target!"; 
                //string responseData = response.ReadAsString(); 
                Debug.Log("HTTP Response: " + responseData);
                //If we get an callback denoting upload successful, play it: (Because "ImmediatelyApply" doesn't work)
                //if(responseData == "{\"result\":[{\"name\":\"tts.wav\",\"systemAsset\":false}],\"status\":\"Success\"}") {
                //if(responseData.Length > 47 && responseData.Substring(responseData.Length - 47) == ".wav\",\"systemAsset\":false}],\"status\":\"Success\"}") { PlayAudio(audioname); }
                //if(responseData == {\"error\":\"Object reference not set to an instance of an object.\",\"status\":\"Failed\"}" ) { PlayAudio(audioname); }
                //IF we can't find TTS file pre-loaded onto Misty, make a new one and send it over.
                if(responseData == "{\"error\":\"Unable to find requested audio clip.\",\"status\":\"Failed\"}") { SaveTTS(ttsSpeech); }
            }
            //#pragma warning restore 0219
        });   
    }
}