using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UT.MailSample {
    public class EmailSender : MonoBehaviour {
        
        [Header("SMTP Settings for Sample Sending")]
        public string EmailAddress = "";
        public string EmailAccount = "";
        public string Password = "";
        public string SmtpServer = "";
        public int SmtpPort = 0;
        public bool EnableSSL = true;
        public AdvancedController advancedController;

        [Header("UI Controls")]
        public InputField To;
        public InputField Cc;
        public InputField Bcc;
        public InputField Subject;
        public InputField Body;
        public Toggle attachMultiplePalettes;
        public Button SendButton;
        public Text emailSender;

        // Demonstrates the email sending feature.
        public void Send() {
            if (!IsSmtpConfigured()) { Debug.LogError(CantSendText); return; }
            
            using (var message = PrepareMailMessage()) {
                Debug.Log("Sending \"" + message.Subject + "\"...");

                // Handles the result of sending
                UT.Mail.ResultHandler onSent = (mailMessage, success, errorMessage) => {
                    if (success) { Debug.Log("Successfully sent \"" + mailMessage.Subject + "\""); } 
                    else { Debug.LogError("Failed to send \"" + mailMessage.Subject + "\": " + errorMessage); }
                };

                if (SmtpPort > 0) {
                    // Email Account is omitted?
                    if (!string.IsNullOrEmpty(EmailAccount)) {
                        UT.Mail.Send(message, SmtpServer, SmtpPort, EmailAddress, EmailAccount, Password, EnableSSL, onSent);
                    } else {
                        UT.Mail.Send(message, SmtpServer, SmtpPort, EmailAddress, Password, EnableSSL, onSent);
                    }
                } else {
                    // SmtpPort is omitted?
                    if (!string.IsNullOrEmpty(EmailAccount)) {
                        UT.Mail.Send(message, SmtpServer, EmailAddress, EmailAccount, Password, EnableSSL, onSent);
                    } else {
                        UT.Mail.Send(message, SmtpServer, EmailAddress, Password, EnableSSL, onSent);
                    }
                }
            }
            advancedController.hideEmailPanel();
        }

        // Builds UT.MailMessage which is then can be used for composing or sending.
        public UT.MailMessage PrepareMailMessage() {
            Save();

            var mailMessage = new UT.MailMessage()
                .SetSubject(Subject.text)
                .SetBody(Body.text)
                .SetBodyHtml(true);

            foreach (string email in ToEmailsList(To.text))  { mailMessage.AddTo(email); }
            foreach (string email in ToEmailsList(Cc.text))  { mailMessage.AddCC(email); }
            foreach (string email in ToEmailsList(Bcc.text)) { mailMessage.AddBcc(email); }

            if (attachMultiplePalettes.isOn) { 
                advancedController.SaveAllCSVPalettes();
                foreach(string path in advancedController.getPalettePaths()) { mailMessage.AddAttachment(path); }
            } else { 
                advancedController.SaveCSVPalette();
                string path = advancedController.getPalettePath();
                if(path != null) { mailMessage.AddAttachment(path); } 
                else { Debug.Log("No palette selected! Please select a palette before emailing."); }
            }

            return mailMessage;
        }

        // Initializes the sample UI.
        private void Start() {
            Debug.Assert(To != null, "Please specify \"To\"!");
            Debug.Assert(Cc != null, "Please specify \"Cc\"!");
            Debug.Assert(Bcc != null, "Please specify \"Bcc\"!");
            Debug.Assert(Subject != null, "Please specify \"Subject\"!");
            Debug.Assert(Body != null, "Please specify \"Body\"!");
            Debug.Assert(SendButton != null, "Please specify \"SendButton\"!");
            Load();

            emailSender.text = "Sender address: " + EmailAddress;

            var moreButton = MoreButton.FindInstance();
            if (moreButton != null) {
                moreButton.MenuItems = new MoreButton.PopupMenuItem[] {new MoreButton.PopupMenuItem("EXIT", () => Application.Quit())};
            }

            SendButton.onClick.AddListener(Send);

            if (!IsSmtpConfigured()) { SendButton.GetComponentInChildren<Text>().text = CantSendText; }
        } 
        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) { advancedController.hideEmailPanel(); }
        }
        private void OnApplicationQuit() { Save(); }

    #if UNITY_EDITOR
        private void Reset() {
            this.To = null;
            this.Cc = null;
            this.Bcc = null;
            this.Subject = null;
            this.Body = null;
            this.SendButton = null;           
            OnValidate();
        }
        private void OnValidate() {
            FindObjectIfNotSet<InputField>("To", ref this.To);
            FindObjectIfNotSet<InputField>("Cc", ref this.Cc);
            FindObjectIfNotSet<InputField>("Bcc", ref this.Bcc);
            FindObjectIfNotSet<InputField>("Subject", ref this.Subject);
            FindObjectIfNotSet<InputField>("Body", ref this.Body);
            FindObjectIfNotSet<Button>("Send", ref this.SendButton);
        }
        private void FindObjectIfNotSet<T>(string name, ref T control) {
            if (control == null) {
                GameObject go = GameObject.Find(name);
                if (go != null) {
                    control = go.GetComponentInChildren<T>();
                    if (control != null) { UnityEditor.EditorUtility.SetDirty(this); }
                }
            }
        }
    #endif

        private bool IsSmtpConfigured() {
            return !string.IsNullOrEmpty(SmtpServer) && !string.IsNullOrEmpty(EmailAddress) && !string.IsNullOrEmpty(Password);
        }
        private static bool CheckValidEmailList(InputField field) {
            var incorrect = field.transform.Find("Incorrect");
            if (string.IsNullOrEmpty(field.text)) {
                incorrect.gameObject.SetActive(false);
                return true;
            } else {
                bool correct = EmailRegex.IsMatch(field.text);
                incorrect.gameObject.SetActive(!correct);
                return correct;
            }
        }
        private void SaveInputField(string fieldName, InputField field) {
            PlayerPrefs.SetString("UTMailSample." + fieldName, field.text);
        }
        private void LoadInputField(string fieldName, InputField field, string defaultValue) {
            field.text = PlayerPrefs.GetString("UTMailSample." + fieldName, defaultValue);
        }
        private void SaveToggle(string fieldName, Toggle toggle) {
            PlayerPrefs.SetInt("UTMailSample." + fieldName, toggle.isOn ? 1 : 0);
        }
        private void LoadToggle(string fieldName, Toggle toggle) {
            toggle.isOn = PlayerPrefs.GetInt("UTMailSample." + fieldName, toggle.isOn ? 1 : 0) != 0;
        }
        private void Save() {
            SaveInputField("To", To);
            SaveInputField("Cc", Cc);
            SaveInputField("Bcc", Bcc);
            SaveInputField("Subject", Subject);
            SaveInputField("Body", Body);
            PlayerPrefs.Save();
        }
        private void Load() {
            LoadInputField("To", To, DefaultTo);
            LoadInputField("Cc", Cc, DefaultCC);
            LoadInputField("Bcc", Bcc, DefaultBcc);
            LoadInputField("Subject", Subject, DefaultSubject);
            LoadInputField("Body", Body, DefaultBody);
        }
        private static string[] ToEmailsList(string list) {
            if (string.IsNullOrEmpty(list)) {
                return new string[0];
            } else {
                string[] result = list.Split(',');
                for (int i = 0; i < result.Length; ++i) { result[i] = result[i].Trim(); }
                return result;
            }
        }
        private const string DefaultTo = "utmail-test@universal-tools.com";
        private const string DefaultCC = "";
        private const string DefaultBcc = "";
        private const string DefaultSubject = "Hi from UTMail!";
        private const string DefaultBody = "This is a test message. Seems to work well, as you're reading it!";
        private static readonly Regex EmailRegex = new Regex(@"^(([\w\.\-]+)@([\w\-]+)((\.\w+)+)\s*,*\s*)*$");
        private const string CantSendText = "Configure GameObject \"UTMail Sample\" to send!";
    }
}