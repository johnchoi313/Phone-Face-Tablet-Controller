using UnityEngine; 
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;

using Crosstales.RTVoice;
using Crosstales.RTVoice.Model;
using TechTweaking.Bluetooth;
using AUP;


public class FAMKeyboard : MonoBehaviour {

    //face and speech control variables
    private float eyeBlend = 0f;
    public float blendSpeed = 1f;
    private int previousExpression = 0;
    private Color faceColor = new Color(0.66f, 1.0f, 0.5f);
    private Color currentFaceColor = new Color(0.66f, 1.0f, 0.5f);

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
    private TextToSpeechPlugin textToSpeechPlugin;
    
    //JSON Variables
    private string jsonStr = "";
    public string speech = "";
    public int expression = 0;
    public int pitch = 100;

    //bluetooth receiver variables
    private const string UUID = "0acc9c7c-48e1-41d2-acaa-610d1a7b085e";
    private BluetoothDevice device;
    public Text dataToSend;

    //Initialization
    private void Awake() {
        BluetoothAdapter.enableBluetooth();//Force Enabling Bluetooth
        BluetoothAdapter.OnDevicePicked += HandleOnDevicePicked;
        BluetoothAdapter.OnClientRequest += HandleOnClientRequest;
        BluetoothAdapter.startServer(UUID, 1000); // Init server

        textToSpeechPlugin = TextToSpeechPlugin.GetInstance();
        textToSpeechPlugin.SetDebug(0);
        textToSpeechPlugin.Initialize();
    }

    // Update is called once per frame
    void Update() {

        //read keyboard input
        foreach (char c in Input.inputString) {
            jsonStr += c;
            if (c == '}') { 
                //if end of message, parse JSON
                Debug.Log(jsonStr);
                setFaceData(jsonStr);
                jsonStr = "";
            }
        }

        //speech to text test
        //if (Input.GetKeyDown(KeyCode.Space)) { Speaker.Speak("Hello World"); }
        
        //update FAM Face
        setExpression();
        setFaceColor();
        setLipSync();
        updateBlinking();
    }

    //Android Bluetooth Direct Connection
    void HandleOnClientRequest(BluetoothDevice device) {
        this.device = device;
        this.device.setEndByte(10);
        this.device.ReadingCoroutine = ManageConnection;
        this.device.connect();
    }
    void HandleOnDevicePicked(BluetoothDevice device) {
        this.device = device;
        this.device.UUID = UUID;
        device.setEndByte(10);
        device.ReadingCoroutine = ManageConnection;
    }
    void OnDestroy() {
        BluetoothAdapter.OnDevicePicked -= HandleOnDevicePicked;
        BluetoothAdapter.OnClientRequest -= HandleOnClientRequest;
        BluetoothAdapter.OnDevicePicked -= HandleOnDevicePicked;
    }
    //Manage Reading Coroutine
    IEnumerator ManageConnection(BluetoothDevice device) {
        while (device.IsReading) {
            if (device.IsDataAvailable) {
                byte[] msg = device.read();
                if (msg != null && msg.Length > 0) {
                    string content = System.Text.ASCIIEncoding.ASCII.GetString(msg);
                    setFaceData(content);
                }
            }
            yield return null;
        }
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
        previousExpression = expression;
        FaceData faceData = JsonUtility.FromJson<FaceData>(json);
        
        if (faceData != null) {
            try {
                pitch = faceData.p;
                expression = faceData.e;
                speech = faceData.s;
                faceColor = new Color(faceData.r / 255f,
                                      faceData.g / 255f,
                                      faceData.b / 255f);
                //textToSpeechPlugin.SetPitch(pitch); //(0-2)
                //textToSpeechPlugin.SetSpeechRate(speechRate); //(0-2)
                //textToSpeechPlugin.SpeakOut(speech, "id");
                Speaker.Speak(speech);
                eyeBlend = 0f;
            } catch (Exception e) {
                Debug.LogException(e, this);
            }
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

/*

[System.Serializable] 
public class FaceData {
    public int pitch = 0;
    public int expression = 0;
    public string speech = "";
    public float red = 0;
    public float green = 0;
    public float blue = 0;
}

*/