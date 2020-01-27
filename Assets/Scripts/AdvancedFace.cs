using UnityEngine; 
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;

using Crosstales.RTVoice;
using Crosstales.RTVoice.Model;
using TechTweaking.Bluetooth;
using OpenCvSharp;
using AUP;

public class AdvancedFace : MonoBehaviour {

    public enum ExpressionMode { BLENDSHAPE, TEXTURE };
    public ExpressionMode expressionMode = ExpressionMode.BLENDSHAPE; 

    [Header("Blendshape Expressions")]
    private float eyeBlend = 0f;
    public float blendSpeed = 1f;
    private int previousExpression = 0;
    
    [Header("Texture Expressions")]
    public List<TextureExpression> textureExpressions;
    
    [Header("Face Color")]
    private Color faceColor = new Color(0.4f, 1, 1);
    private Color currentFaceColor = new Color(0.4f, 1, 1);

    [Header("Blink Variables")]
    public float blinkSpeed = 4f;
    public float blinkDelay = 4f;
    private float blinkSize = 1f;
    private float blinkTimer = 1f;
    
    [Header("LipSync Variables")]
    public float gain = 100;
    public float lipMult = 1;
    public float lipSpeed = 1;
    private float lipPitch = 0;
    public float lipStretch = 1;
    public float lipLerpSpeed = 1;
    public enum LipMode { migo, hugo };
    public LipMode lipMode = LipMode.migo;
    public Vector3 mouthScale;

    [Header("GameObject Assignments")]
    public GameObject face;
    public LineRenderer line;
    public EyeWarp leftEyeWarp;
    public EyeWarp rightEyeWarp;
    public MeshRenderer textureMouth;
    public SkinnedMeshRenderer leftEye;
    public SkinnedMeshRenderer rightEye;
    
    [Header("JSON Variables")]
    public string speech = "";
    private string jsonStr = "";
    public int expression = 0;
    public float pitch = 1;
    public float rate = 1;

    [Header("Networking Variables")]
    public BluetoothConnection bluetoothReceiver;
    public TankController tankController;
    public MP3MusicPlayer mp3MusicPlayer;
    public YoutubePlayer youtubePlayer;
    public UDPReceiver udpReceiver;
    public Chatlog chatlog;

    // Update is called once per frame
    void Update() {
        //speech to text test
        //if (Input.GetKeyDown(KeyCode.Space)) { Speaker.Speak("Hello World"); }
    
        //Get UDP Data
        string udpMessage = udpReceiver.getMessage(); 
        if(udpMessage != null && udpMessage.Length > 0) {Debug.Log(udpMessage);}
        setFaceData(udpMessage);
        
        //Get Bluetooth Data
        string bluetoothMessage = bluetoothReceiver.getMessage();
        if(bluetoothMessage != null && bluetoothMessage.Length > 0) {Debug.Log(bluetoothMessage);}
        setFaceData(bluetoothMessage);

        //update FAM Face
        setExpression();
        setFaceColor();
        setLipSync();
        updateBlinking();
    }

    //set blink data
    private void updateBlinking() {
        if (blinkTimer > 0) {
            blinkTimer -= Time.deltaTime;
        } else {
            blinkTimer = UnityEngine.Random.Range(blinkDelay/2, blinkDelay);
            blinkSize = -1;
        }
        blinkSize = Mathf.Lerp(blinkSize,1,Time.deltaTime * blinkSpeed);
        leftEyeWarp.blink = blinkSize;
        rightEyeWarp.blink = blinkSize;
    }

    //set face data
    public void setFaceData(string json) {
        if(json == null) { return; }

        //Quick Blink message
        if(json.Equals("blink")) { blinkTimer = 0; return; }

        //Tank control message (Prefaced with a "t")
        if(json.Substring(0,1).Equals("t")) {
            TankSpeed tankSpeed = JsonUtility.FromJson<TankSpeed>(json.Substring(1));
            tankController.remoteControlTank(tankSpeed.x * 0.01f, tankSpeed.y * 0.01f);
            return;
        }

        //Full face message
        previousExpression = expression;
        FaceData faceData = JsonUtility.FromJson<FaceData>(json);

        if (faceData != null) {
            try {
                rate = faceData.ra * 0.01f;
                pitch = faceData.p * 0.01f;
                expression = faceData.e;
                speech = faceData.s;
                faceColor = new Color(faceData.r / 255f,
                                      faceData.g / 255f,
                                      faceData.b / 255f);
                
                Speaker.Silence();
                mp3MusicPlayer.StopMP3();
                
                youtubePlayer.Stop();
                youtubePlayer.HideLoading(); 
                youtubePlayer.OnVideoPlayerFinished();

                //If detect Youtube video, play Youtube video!
                string checkYoutubeUrl = speech; 
                if(youtubePlayer.TryNormalizeYoutubeUrlLocal(checkYoutubeUrl, out checkYoutubeUrl)) {
                    Debug.Log(checkYoutubeUrl);
                    youtubePlayer.ShowLoading();
                    youtubePlayer.Play(checkYoutubeUrl);                    
                //    youtubePlayer.PlayYoutubeVideo(checkYoutubeUrl);                    
                }
                //If detect MP3 music file, play MP3 from local file.
                else if(speech.Length > 4 && speech.Substring(speech.Length - 4).ToLower() == ".mp3") {
                    mp3MusicPlayer.PlayMP3FromFile(speech);
                } 
                //Otherwise play Speech-to-Text;
                else {
                    Speaker.Speak(speech, null, null, true, rate, pitch, 1); //rate = 0-3, pitch = 0-2, volume = 0-1 
                }
                if(chatlog) { chatlog.userSpoke(speech); } 

                //Blink
                eyeBlend = 0f;
            } catch (Exception e) { Debug.LogException(e, this); }
        }
    }

    //control face
    private void setLipSync() {
        //Get audio analysis
        float[] spectrum = new float[64];
        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
        //Get sum of audio analysis
        float A = 0;
        for (int i = 0; i < 64; i++) { A += spectrum[i] * gain; }
        A /= 64;
        lipPitch = Mathf.Lerp(lipPitch, A, Time.deltaTime * lipLerpSpeed);
        
        //Line Style Mouth
        if (line != null) {
            //Show FFT on line renderers
            if (lipMode == LipMode.migo) {
                for (int i = 0; i < line.positionCount; i++) {
                    float pos = Mathf.Sin(Time.time * lipSpeed + i * lipStretch) * lipPitch * lipMult;
                    line.SetPosition(i, new Vector3(i - line.positionCount / 2, pos, 0));
                }
            }
            if (lipMode == LipMode.hugo) {
                line.transform.localScale = new Vector3(line.transform.localScale.x, lipPitch * lipMult, line.transform.localScale.z);
            }
        }
        //Texture Style Mouth
        if(textureMouth != null) {
            textureMouth.transform.localScale = new Vector3(mouthScale.x, mouthScale.y + lipPitch * lipMult, mouthScale.z);   
        }
    }

    //controls face color
    private void setFaceColor() {
        currentFaceColor = Color.Lerp(currentFaceColor, faceColor, Time.deltaTime * blendSpeed);
        face.GetComponent<Renderer>().material.SetColor("_Color", currentFaceColor);
    }

    //controls face expression
    private void setExpression() {
        if(leftEye && rightEye) { 
            if(expressionMode == ExpressionMode.BLENDSHAPE) {
                //increase blend shape frame
                eyeBlend = Mathf.Lerp(eyeBlend, 100f, Time.deltaTime * blendSpeed);
                //remove all blendshapes
                for (int i = 0; i < 5; i++) {
                    leftEye.SetBlendShapeWeight(i, 0);
                    rightEye.SetBlendShapeWeight(i, 0);
                }
                if (previousExpression > 0 && expression != previousExpression) {
                    int sub = (previousExpression == 3) ? 0 : 1;
                    leftEye.SetBlendShapeWeight(previousExpression - sub, 100f - eyeBlend);
                    rightEye.SetBlendShapeWeight(previousExpression - 1, 100f - eyeBlend);
                }
                //set expression
                switch (expression) {
                    case 0: //Neutral;
                        break;
                    case 1: //Surprised; 
                        leftEye.SetBlendShapeWeight(0, eyeBlend);
                        rightEye.SetBlendShapeWeight(0, eyeBlend);
                        break;
                    case 2: //Happy; 
                        leftEye.SetBlendShapeWeight(1, eyeBlend);
                        rightEye.SetBlendShapeWeight(1, eyeBlend);
                        break;
                    case 3: //Sad; 
                        leftEye.SetBlendShapeWeight(3, eyeBlend);
                        rightEye.SetBlendShapeWeight(2, eyeBlend);
                        break;
                    case 4: //Concerned; 
                        leftEye.SetBlendShapeWeight(4, eyeBlend);
                        rightEye.SetBlendShapeWeight(4, eyeBlend);
                        break;
                    default: //Neutral
                        break;
                }
            } else if(expressionMode == ExpressionMode.TEXTURE) {
                switch (expression) {
                    //Neutral;
                    case 0: setTextureExpression(0); break;
                    //Surprised; 
                    case 1: setTextureExpression(1); break;
                    //Happy; 
                    case 2: setTextureExpression(2); break;
                    //Sad; 
                    case 3: setTextureExpression(3); break;
                    //Concerned; 
                    case 4: setTextureExpression(4); break;
                    //Neutral
                    default: setTextureExpression(0); break;
                }
            }
        }
    }
    public void setTextureExpression(int index) {
        if(!(0 <= index && index < textureExpressions.Count)) { Debug.LogWarning("Bad Texture Index out of range  at [" + index + "]!"); return; }
        leftEye.material.mainTexture = textureExpressions[index].eyeLeft;
        rightEye.material.mainTexture = textureExpressions[index].eyeRight;
        textureMouth.material.mainTexture = textureExpressions[index].mouth;
        leftEyeWarp.setSize(textureExpressions[index].eyeScale);
        rightEyeWarp.setSize(textureExpressions[index].eyeScale);
        mouthScale = textureExpressions[index].mouthScale;
    }
}

[System.Serializable] 
public class FaceData {
    public int p = 100;     //pitch
    public int ra = 100;    //rate
    public int e = 0;       //expression
    public string s = "";   //speech
    public int r = 0;       //red
    public int g = 0;       //green
    public int b = 0;       //blue
}

[System.Serializable] 
public class TankSpeed {
    public int x = 0;
    public int y = 0;
}

[System.Serializable] 
public class TextureExpression {
    public string name;
    public Texture mouth;
    public Texture eyeLeft;
    public Texture eyeRight;
    public Vector3 eyeScale = new Vector3(1,1,1);
    public Vector3 mouthScale = new Vector3(1,1,1);
}