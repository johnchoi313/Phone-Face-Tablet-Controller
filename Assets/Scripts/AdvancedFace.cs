using UnityEngine; 
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;

using Crosstales.RTVoice;
using Crosstales.RTVoice.Model;
using TechTweaking.Bluetooth;
using OpenCvSharp;

public class AdvancedFace : MonoBehaviour {

    //face and speech control variables
    private float eyeBlend = 0f;
    public float blendSpeed = 1f;
    private int previousExpression = 0;
    private Color faceColor = new Color(0.4f, 1, 1);
    private Color currentFaceColor = new Color(0.4f, 1, 1);

    //blink variables
    private float blinkTimer = 1f;
    private float blinkSize = 1f;
    public float blinkSpeed = 4f;
    public float blinkDelay = 4f;

    //lipsync variables
    public float gain = 100;
    public float lipSpeed = 1;
    private float lipPitch = 0;
    public float lipStretch = 1;
    public float lipLerpSpeed = 1;
    public enum LipMode { migo, hugo };
    public LipMode lipMode = LipMode.migo;

    public GameObject face;
    public LineRenderer line;
    public EyeWarp leftEyeWarp;
    public EyeWarp rightEyeWarp;
    public SkinnedMeshRenderer leftEye;
    public SkinnedMeshRenderer rightEye;
    
    //JSON Variables
    private string jsonStr = "";
    public string speech = "";
    public int expression = 0;
    public float pitch = 1;
    public float rate = 1;

    //Bluetooth Variables
    public BluetoothConnection bluetoothReceiver;

    //UDP IP Variables
    public UDPReceiver udpReceiver;

    //Tank Variables
    public TankController tankController;

    //Chatlog variables
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
    public void updateBlinking() {
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
                Speaker.Speak(speech, null, null, true, rate, pitch, 1); //rate = 0-3, pitch = 0-2, volume = 0-1 
                if(chatlog) { chatlog.userSpoke(speech); } 
                //Blink
                eyeBlend = 0f;
            } catch (Exception e) { Debug.LogException(e, this); }
        
        }
    }

    //control face
    void setLipSync() {
        if (line != null) {
            float[] spectrum = new float[64];
            AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);

            //Get sum of audio analysis
            float A = 0;
            for (int i = 0; i < 64; i++) { A += spectrum[i] * gain; }
            A /= 64;
            lipPitch = Mathf.Lerp(lipPitch, A, Time.deltaTime * lipLerpSpeed);

            //Show FFT on line renderers
            if (lipMode == LipMode.migo) {
                for (int i = 0; i < line.positionCount; i++) {
                    float pos = Mathf.Sin(Time.time * lipSpeed + i * lipStretch) * lipPitch;
                    line.SetPosition(i, new Vector3(i - line.positionCount / 2, pos, 0));
                }
            }
            if (lipMode == LipMode.hugo) {
                line.transform.localScale = new Vector3(line.transform.localScale.x, lipPitch, line.transform.localScale.z);
            }
        }
    }

    //controls face color
    public void setFaceColor() {
        currentFaceColor = Color.Lerp(currentFaceColor, faceColor, Time.deltaTime * blendSpeed);
        face.GetComponent<Renderer>().material.SetColor("_Color", currentFaceColor);
    }

    //controls face expression
    public void setExpression() {
        if(leftEye && rightEye) { 
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
        }
    }
}

[System.Serializable] 
public class FaceData {
    public int p = 100;     //pitch
    public int ra = 100;    //rate
    public int e = 0;     //expression
    public string s = ""; //speech
    public int r = 0;     //red
    public int g = 0;     //green
    public int b = 0;     //blue
}

[System.Serializable] 
public class TankSpeed {
    public int x = 0;
    public int y = 0;
}
