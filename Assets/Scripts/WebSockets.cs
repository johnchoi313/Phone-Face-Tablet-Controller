using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class WebSockets : MonoBehaviour {

    /*

    public string serverURL = "ws://localhost:8765";
    public string id = "1234";
    private string msg;

    public SurveyQuestionnaire surveyQuestionnaire;
    private JSONObject surveyResults;
    
    private bool connected = false;

    public bool connectOnStart = false;
    public InputField serverURLInput;
    public InputField userIDInput;
    public Text ConnectButtonText;
    
    public SaraController sara;
    public WebGLSpeaker webGLSpeaker;

    private WebSocket w;

    void Start() {
        //Autofill and autostart settings
        if (serverURLInput != null && serverURL != null && serverURL.Length > 0) { serverURLInput.text = serverURL; }
        if (userIDInput != null && id != null && id.Length > 0) { userIDInput.text = id; }
        if (connectOnStart) { StartCoroutine(TalkToPythonMUF()); }
    }

    //Receiving messages from websocket 
    IEnumerator TalkToPythonMUF() {
        w = new WebSocket(new Uri(serverURL));
        yield return StartCoroutine(w.Connect());

        if(ConnectButtonText != null) { ConnectButtonText.text = "Disconnect"; }
        
        connected = true;

        //Repeat forever while alive.
        while (true) {

            //Attempt to send a chat message to Python / MUF server if available:
            if (id != null && id.Length > 0 &&
                msg != null && msg.Length > 0) {
                //Format into JSON with ID and MSG tags.
                JSONObject toPythonMUF = new JSONObject();
                toPythonMUF.AddField("userid", id);
                toPythonMUF.AddField("message", msg);
                //Finally send it to the server.
                w.SendString(toPythonMUF.ToString());
                msg = null;
            }
            //Attempt to send a survey results message to Python / MUF server if finished:
            else if (id != null && id.Length > 0 &&
                     surveyResults != null) {
                //Format into JSON with ID and MSG tags.
                JSONObject toPythonMUF = new JSONObject();
                toPythonMUF.AddField("userid", id);
                toPythonMUF.AddField("survey", surveyResults);
                //Finally send it to the server.
                w.SendString(toPythonMUF.ToString());
                surveyResults = null;
            }

            //Wait until the reply is received:
            string reply = w.RecvString();
            if (reply != null) {
                Debug.Log("Websocket Received: " + reply);
                if (surveyQuestionnaire.currentSlide == 3) {
                    sara.addMessage(reply);
                    webGLSpeaker.resetMicButton();
                }
            }
            if (w.error != null) {
                Debug.LogWarning("Error: " + w.error);
                break;
            }
            yield return 0;
        }
        w.Close();
    }

    //Close the websocket if open before quitting.
    void OnApplicationQuit() {
        if (w != null) { w.Close(); }
    }

    //Public function to connect to Python MUF Websocket Server
    public void ConnectToPythonMUF() {
        //If not connected, attempt connection
        if (!connected) {
            StartCoroutine(TalkToPythonMUF());
        }
        //If connected, disconnect
        else {
            w.Close();
            connected = false;
            if (ConnectButtonText != null) { ConnectButtonText.text = "Connect"; }
        }
    }

    //Public function wrapper to send message to websocket server
    public void sendMessage(string message) {
        if (message != null && message.Length > 0) { msg = message; }
    }

    //Public function wrapper to send survey results to websocket server
    public void sendSurveyResults() {
        JSONObject results = surveyQuestionnaire.getSurveyResults();
        if (results != null) {surveyResults = results; }
    }

    //Public function wrapper to set ID for sending message
    public void setUserID(string userid) {
        if (userid != null && userid.Length > 0) {
            id = userid;
            surveyQuestionnaire.createValidationCode(id);
        }
    }
    //Public function wrapper to login with ID
    public void loginWithUserID() {
        if (id != null && id.Length > 0) {
            surveyQuestionnaire.NextSlide();
        } else {
            surveyQuestionnaire.AMTIDWarning();
        }
    }

    //Public function wrapper to set server URL 
    public void setServerURL(string url) {
        if (url != null && url.Length > 0) { serverURL = url; }
    }

    */

}
