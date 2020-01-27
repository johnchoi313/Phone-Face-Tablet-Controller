using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Crosstales.RTVoice;
using Crosstales.RTVoice.Model;
using PygmyMonkey.FileBrowser;
using TechTweaking.Bluetooth;
using SimpleFileBrowser;

public class WasedaController : MonoBehaviour {

  [Header("Connection Variables")]
  public UDPSender udpSender;
  public Toggle autoSendToggle;
  
  [Header("Palette Variables")]
  public GameObject paletteSelector;
  public GameObject paletteInstantiator;
  public InputField paletteTitle;
  public bool autoSaveLoadPalettes;
  private string defaultPalettePath;
  private List<Palette> palettes;
  private int paletteIndex;
  private string SLASH;
  
  [Header("Button Variables")]
  public GameObject buttonSelector;
  public GameObject buttonInstantiator;
  private PaletteButton revert;
  private int buttonIndex;

  [Header("Speech Variables")]
  public InputField speechSynthesis;
  public InputField buttonAnimation;
  public InputField buttonTitle;
  public InputField buttonGaze;
  public InputField voiceField; private string voice;
  public Dropdown agentIDDropdown; private int agentID; 
  public Dropdown buttonColor;
  public Dropdown buttonEmotion;
  public Dropdown shortcutDropdown;
  public Toggle localTTSToggle;
  public Slider speechPitch;
  public Slider speechRate;

  [Header("Summary Variables")]
  public List<ButtonPress> buttonPresses;
  
  [Header("Animation Settings")]
  public List<string> animations;
  
  [Header("Emotion Settings")]
  public List<string> emotions;
  [Header("Color Settings")]
  public List<ColorMap> colors;
  
  [Header("Summary Log Settings")]
  private float sessionDuration, buttonDuration;
  private bool sessionActive = false;
  private string timezone = "";

  [Header("Other Variables")]
  public GameObject androidSaveLoadBackground;

  ///***********************************************///
  ///***************UPDATE FUNCTIONS****************///
  ///***********************************************///
  public void SetLocalTTS(bool isOn) {
    localTTSToggle.isOn  = isOn; PlayerPrefs.SetInt("LocalTTS", isOn?1:0);
  }
  public void sayTTS() {
    if(speechSynthesis.text != null && speechSynthesis.text.Length > 0) {
      Speaker.Speak(speechSynthesis.text, null, null, true, speechRate.value, speechPitch.value, 1); //rate = 0-3, pitch = 0-2, volume = 0-1
    }                
  }
  
  //Checking for Keyboard Shortcuts
  void Update() {
    if(sessionActive) { sessionDuration += Time.deltaTime; }
    if(buttonSelector.activeSelf) {
      if(0 <= buttonIndex && buttonIndex < getCurrentPaletteButtonCount() && getCurrentPaletteButtonCount() > 0) {
        buttonDuration += Time.deltaTime;
      }
    }
    //checkKeyboardShortcuts();
  }
  private void checkKeyboardShortcuts() {
    int charKey = 0;
    if(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
      if(Input.GetKeyDown(KeyCode.A))      { charKey = 1; }
      else if(Input.GetKeyDown(KeyCode.B)) { charKey = 2; }
      else if(Input.GetKeyDown(KeyCode.C)) { charKey = 3; }
      else if(Input.GetKeyDown(KeyCode.D)) { charKey = 4; }
      else if(Input.GetKeyDown(KeyCode.E)) { charKey = 5; }
      else if(Input.GetKeyDown(KeyCode.F)) { charKey = 6; }
      else if(Input.GetKeyDown(KeyCode.G)) { charKey = 7; }
      else if(Input.GetKeyDown(KeyCode.H)) { charKey = 8; }
      else if(Input.GetKeyDown(KeyCode.I)) { charKey = 9; }
      else if(Input.GetKeyDown(KeyCode.J)) { charKey = 10; }
      else if(Input.GetKeyDown(KeyCode.K)) { charKey = 11; }
      else if(Input.GetKeyDown(KeyCode.L)) { charKey = 12; }
      else if(Input.GetKeyDown(KeyCode.M)) { charKey = 13; }
      else if(Input.GetKeyDown(KeyCode.N)) { charKey = 14; }
      else if(Input.GetKeyDown(KeyCode.O)) { charKey = 15; }
      else if(Input.GetKeyDown(KeyCode.P)) { charKey = 16; }
      else if(Input.GetKeyDown(KeyCode.Q)) { charKey = 17; }
      else if(Input.GetKeyDown(KeyCode.R)) { charKey = 18; }
      else if(Input.GetKeyDown(KeyCode.S)) { charKey = 19; }
      else if(Input.GetKeyDown(KeyCode.T)) { charKey = 20; }
      else if(Input.GetKeyDown(KeyCode.U)) { charKey = 21; }
      else if(Input.GetKeyDown(KeyCode.V)) { charKey = 22; }
      else if(Input.GetKeyDown(KeyCode.W)) { charKey = 23; }
      else if(Input.GetKeyDown(KeyCode.X)) { charKey = 24; }
      else if(Input.GetKeyDown(KeyCode.Y)) { charKey = 25; }
      else if(Input.GetKeyDown(KeyCode.Z)) { charKey = 26; }
      if(charKey > 0 && paletteIndex > -1) {
        for(int b = 0; b < getCurrentPaletteButtonCount(); b++) {
          if (palettes[paletteIndex].buttons[b].shortcut == charKey) { selectButton(b); sendMessage(); }
        }
      }
    }
  }

  ///***********************************************///
  ///***********INITIALIZATION FUNCTIONS************///
  ///***********************************************///

  //Initializing Palettes.
  void Start() {    
    //Get local timezone
    foreach(Match match in Regex.Matches(System.TimeZone.CurrentTimeZone.StandardName, "[A-Z]")) { timezone += match.Value; }
    //Check if anything is null
    if(!buttonTitle)      { Debug.LogWarning("buttonTitle is null!"); }
    if(!buttonColor)      { Debug.LogWarning("buttonColor is null!"); }
    if(!buttonEmotion)    { Debug.LogWarning("buttonEmotion is null!"); }
    if(!buttonAnimation)  { Debug.LogWarning("buttonAnimation is null!"); }
    if(!speechSynthesis)  { Debug.LogWarning("speechSynthesis is null!"); }
    if(!speechRate)       { Debug.LogWarning("speechRate is null!"); }
    if(!speechPitch)      { Debug.LogWarning("speechPitch is null!"); }
    if(!shortcutDropdown) { Debug.LogWarning("shortcutDropdown is null!"); }
  	SLASH = (Application.platform == RuntimePlatform.Android ||
             Application.platform == RuntimePlatform.OSXPlayer ||
             Application.platform == RuntimePlatform.OSXEditor)?"/":"\\";
    //defaultPalettePath = Application.streamingAssetsPath + "/Palettes";
    setVoice(PlayerPrefs.GetString("Voice", ""));
    agentIDDropdown.value = PlayerPrefs.GetInt("AgentID", 0);
    localTTSToggle.isOn = PlayerPrefs.GetInt("LocalTTS", 0) > 0?true:false;
    defaultPalettePath = Application.persistentDataPath; //SLASH+ "/Palettes";
    palettes = new List<Palette>();
    InitSimpleFileBrowser();
    initEmotionDropdown();
    initColorDropdown();
    //Optionally, load all palettes.
    if(autoSaveLoadPalettes) { LoadAllCSVPalettes(); }
  }
  //Initializing Emotions.
  public void initEmotionDropdown() {
    buttonEmotion.ClearOptions();
    buttonEmotion.AddOptions(emotions);
    if (buttonEmotion.options.Count > 0) { buttonEmotion.value = 0; }
    else { Debug.LogWarning("No emotions set! Please init emotions."); }
  }
  //Initializing Colors.
  public void initColorDropdown() {
    buttonColor.ClearOptions();
    List<string> colorOptions = new List<string>();
    foreach (ColorMap colorMap in colors) { colorOptions.Add(colorMap.name); }
    buttonColor.AddOptions(colorOptions);
    if (colorOptions.Count > 0) { buttonColor.value = 0; }
    else { Debug.LogWarning("No colors set! Please init colors."); }
  }
  //Initializing Save/Load Browser
  void InitSimpleFileBrowser() {  //Android only.
    SimpleFileBrowser.FileBrowser.SetFilters( true, new SimpleFileBrowser.FileBrowser.Filter("Palettes",".csv"));
    SimpleFileBrowser.FileBrowser.SetDefaultFilter( ".csv" );
    SimpleFileBrowser.FileBrowser.SingleClickMode = true;
    SimpleFileBrowser.FileBrowser.RequestPermission();
  }

  ///***********************************************///
  ///***************LOADING FUNCTIONS***************///
  ///***********************************************///
  public void LoadCSVPaletteButton() {        
    PygmyMonkey.FileBrowser.FileBrowser.OpenMultipleFilesPanel("Load CSV Palette(s)", getPalettePathFolder(), new string[] { "CSV" }, "Open", (bool canceled, string[] filePathArray) => { if (canceled) { return; }
      for (int i = 0; i < filePathArray.Length; i++) {
        if(filePathArray[i].Substring(filePathArray[i].Length - 4).ToLower().Equals(".csv")) {
          LoadCSVPalette(filePathArray[i]);
        } else { Debug.LogWarning("Failed to load \"filePathArray[i]\"! Only .csv files can be loaded!"); }
      }
    });
  }
  public void LoadAllCSVPalettes() {
    string[] filePaths = System.IO.Directory.GetFiles(getPalettePathFolder(), "*.csv");
    foreach(string filePath in filePaths) { LoadCSVPalette(filePath); }
  }
  public void LoadCSVPalette(string path) {
    //NO WEIRD FILE NAMES!
    string title = getPaletteNameFromFilePath(path);   
    //Note: Summary and Log files have %percent so it can be ignored.
    if(title.Contains("<") || title.Contains(">") || title.Contains(":") || title.Contains("\"") || title.Contains("%") || 
       title.Contains("|") || title.Contains("?") || title.Contains("*") || title.Contains(";")) {
      Debug.LogWarning("Tried to open file with restricted characters in name. Ignoring: " + title);
      return;
    }
    //Load files.
  	Debug.Log("Load: " + path);
    StreamReader reader = new StreamReader(path); 
    string encodedString = reader.ReadToEnd();//.ToLower();
    reader.Close();
    string[][] table = CsvParser2.Parse(encodedString);    
    LoadPalette(table, title);
    PlayerPrefs.SetString("PalettePath", getPalettePathFolder(path));
  }
  private void LoadPalette(string[][] table, string name = "Palette Title") {
    //Create a new palette:
    NewPalette();
    setPaletteTitle(name);
    //Add each line of CSV File
    if(table.Length <= 1) { Debug.LogWarning("Empty file ignored: " + name); return; }
    for(int i = 1; i < table.Length; i++) {
      //Create a new button
      NewButton();
      selectButton(getCurrentPaletteButtonCount()-1);
      //Set Button Title
      string title = (table.Length>i && table[i].Length>0 && table[i][0] != null) ? table[i][0] : "None";
      setButtonTitle(title); 
      //Set Button Color
      int color = 0; 
      if(table.Length>i && table[i].Length>1) {
        if(!int.TryParse(table[i][1], out color)) { 
          string text = (table[i][1] != null) ? table[i][1] : colors[0].name;
          color = getColorIndexFromString(text);        
        }
      }
      setButtonColor(color);
      //Set Button Emotion
      int emotion = 0; 
      if(table.Length>i && table[i].Length>2) {
        if(!int.TryParse(table[i][2], out emotion)) { 
          string text = (table[i][2] != null) ? table[i][2] : buttonEmotion.options[0].text;
          emotion = getEmotionIndexFromString(text);        
        }
      }
      setButtonEmotion(emotion); 
      //Set Button Speech
      string speech = (table.Length>i && table[i].Length>3 && table[i][3] != null) ? table[i][3] : "None";
      setSpeechSynthesis(speech); 
      //Set Button Rate
      float rate = 0; 
      if(table.Length>i && table[i].Length>4) {
        if(!float.TryParse(table[i][4], out rate)) { Debug.LogWarning("Table ["+i+"][4] must be float!"); }
      }
      setSpeechRate(rate);
      //Set Button Pitch
      float pitch = 0; 
      if(table.Length>i && table[i].Length>5) {
        if(!float.TryParse(table[i][5], out pitch)) { Debug.LogWarning("Table ["+i+"][5] must be float!"); }
      }
      setSpeechPitch(pitch);
      //Set Button Animation
      string animation = (table.Length>i && table[i].Length>6 && table[i][6] != null) ? table[i][6] : "";
      setButtonAnimation(animation); 
      //Set Button Gaze
      string gaze = (table.Length>i && table[i].Length>7 && table[i][7] != null) ? table[i][7] : "";
      setButtonGaze(gaze); 
      //Set Shortcut Key
      if(shortcutDropdown) {
        int key = 0; 
        if(table.Length>i && table[i].Length>7) {
          if(!int.TryParse(table[i][7], out key)) { Debug.LogWarning("Table ["+i+"][7] must be int!"); }
        }
        setShortcutKey(key); 
      }

      //Deselect when done
      selectButton(-1);
    }
  }
  public void SimpleFileBrowserLoad() {
    androidSaveLoadBackground.SetActive(true);
    StartCoroutine( SimpleFileBrowserLoadCoroutine() );
  }
  IEnumerator SimpleFileBrowserLoadCoroutine() {
    yield return SimpleFileBrowser.FileBrowser.WaitForLoadDialog( false, getPalettePathFolder(), "Load CSV Palette File", "Load" );
    if(SimpleFileBrowser.FileBrowser.Success) { LoadCSVPalette(SimpleFileBrowser.FileBrowser.Result); }
    androidSaveLoadBackground.SetActive(false);
  }

  ///**********************************************///
  ///***************SAVING FUNCTIONS***************///
  ///**********************************************///
  public void SaveCSVPaletteButton() {
    PygmyMonkey.FileBrowser.FileBrowser.SaveFilePanel("Save CSV Palette", "", getPalettePathFolder(), null, new string[] { "CSV" }, null, (bool canceled, string filePath) => { if (canceled) { return; }
      if(filePath.Length > 4 && !filePath.Substring(filePath.Length - 4).ToLower().Equals(".csv")) { filePath = filePath + ".csv"; }
      SaveCSVPalette(filePath);
    });
  }
  public void SaveAllCSVPalettes() {
    if (getPaletteCount() == 0) { Debug.Log("No palettes to save! Nothing saved."); }
    for(int i = 0; i < getPaletteCount(); i++) {
      selectPalette(i);
      SaveCSVPalette(getPalettePathFolder() + SLASH + palettes[i].title  + ".csv");
    }
  }
  public void SaveCSVPalette() {
    if(paletteSelector.activeSelf && 0 <= paletteIndex && paletteIndex < palettes.Count) {
      SaveCSVPalette(getPalettePathFolder() + SLASH + palettes[paletteIndex].title  + ".csv");
    } else { Debug.LogWarning("No palette selected! Nothing saved."); }
  }
  public void SaveCSVPalette(string path) {
    //DON'T SAVE FILE WITH WEIRD CHARACTERS NAME
    string title = getPaletteNameFromFilePath(path);   
    if(title.Contains("<") || title.Contains(">") || title.Contains(":") || title.Contains("\"") || title.Contains("%") || 
       title.Contains("|") || title.Contains("?") || title.Contains("*") || title.Contains(";")) {
      Debug.LogWarning("Tried to save file with restricted characters in name. Ignoring: " + title);
      return;
    }
    Debug.Log("Save: " + path);
    if(paletteSelector.activeSelf && 0 <= paletteIndex && paletteIndex < palettes.Count) {
      //Change Palette Name to Path Name
      setPaletteTitle(getPaletteNameFromFilePath(path));
      //Create CSV String
      string CSVString = "TITLE(text),COLOR,EMOTION,SPEECH(text),RATE(0.0-3.0),PITCH(0.0-2.0),ANIMATION(text),GAZE(text)"; 
      //Add button info to each row
      foreach(PaletteButton button in palettes[paletteIndex].buttons) {
        CSVString += "\r\n";
        CSVString += button.title + ",";
        CSVString += colors[button.color].name + ",";
        CSVString += emotions[button.emotion] + ",";
        CSVString += button.speech + ",";
        CSVString += button.rate.ToString("F2") + ",";
        CSVString += button.pitch.ToString("F2") + ",";
        CSVString += button.animation + ",";
        CSVString += button.gaze + ",";
        if(shortcutDropdown) { CSVString += button.shortcut; }
      }
      //Save Palette to file
      StreamWriter writer = new StreamWriter(path, false); //true to append, false to overwrite
      writer.WriteLine(CSVString); //Filename is palette name
      writer.Close();
      //Save PlayerPref
      PlayerPrefs.SetString("PalettePath", getPalettePathFolder(path));
    } else { Debug.LogWarning("No palette selected! Nothing saved."); }
  }

  public string getPalettePath() { 
    return (0 <= paletteIndex && paletteIndex < palettes.Count) ? (getPalettePathFolder() + SLASH + palettes[paletteIndex].title  + ".csv") : null; 
  }
  public string[] getPalettePaths() {
    string[] paths = new string[getPaletteCount()];
    for(int i = 0; i < getPaletteCount(); i++) { paths[i] = getPalettePathFolder() + SLASH + palettes[i].title  + ".csv"; }
    return paths;
  }
  public string getPalettePathFolder(string path = null) {
    if(path == null || path.LastIndexOf(SLASH) < 0) { return PlayerPrefs.GetString("PalettePath", defaultPalettePath); }
    //if(path == null || path.LastIndexOf(SLASH) < 0) { return defaultPalettePath; }
    return path.Substring(0, path.LastIndexOf(SLASH));
  }
  public string getPaletteNameFromFilePath(string path) {
    path = path.Substring(path.LastIndexOf(SLASH) + 1);
    return path.Substring(0, path.Length-4);
  }
  public void SimpleFileBrowserSave() {
    androidSaveLoadBackground.SetActive(true);
    StartCoroutine( SimpleFileBrowserSaveCoroutine() );
  }
  IEnumerator SimpleFileBrowserSaveCoroutine() {
    yield return SimpleFileBrowser.FileBrowser.WaitForSaveDialog( false, getPalettePathFolder(), "Save CSV Palette File", "Save" );
    if(SimpleFileBrowser.FileBrowser.Success) { SaveCSVPalette(SimpleFileBrowser.FileBrowser.Result); }
    androidSaveLoadBackground.SetActive(false);
  }
  ///***********************************************************///
  ///***************SUMMARY AND LOGGING FUNCTIONS***************///
  ///***********************************************************///
  public void startSession() { sessionActive = true; sessionDuration = 0; }
  public void stopSession() { sessionActive = false; SaveSummary(); }
  public void AddToLog(ButtonPress buttonPress) { string title = "%Log_" + System.DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
    string CSVString = "";
    if(!File.Exists(defaultPalettePath + SLASH + title)) { CSVString += "Timestamp,Response Time,Client ID,Palette Title,Speech,Goal,Subgoal,Proficiency" + "\r\n"; }
    CSVString += System.DateTime.Now.ToString("yyyy-MM-dd h:mm:sstt ") + timezone + ","; //Time Stamp
    CSVString += buttonDuration.ToString("n2") + "s" + ","; //Response time
    CSVString += palettes[paletteIndex].title + ","; //Title
    CSVString += buttonPress.speech + ","; //Speech
    //Write to file
    StreamWriter writer = new StreamWriter(defaultPalettePath + SLASH + title, true); //true to append, false to overwrite
    writer.WriteLine(CSVString); 
    writer.Close();
  }
  public void SaveSummary() { string title = "%Summary_" + System.DateTime.Now.ToString("yyyy-MM-dd") + ".csv";
    if(!paletteSelector.activeSelf) { Debug.Log("No palette selected. No summary saved."); return; }
    //Create CSV String Top
    string CSVString = "";
    if(!File.Exists(defaultPalettePath + SLASH + title)) { 
      CSVString += "Timestamp,Duration,Client ID,Palette Title,";
      CSVString += "\r\n";
    }
    CSVString += System.DateTime.Now.ToString("yyyy-MM-dd h:mm:sstt ") + timezone + ","; //Time Stamp
    CSVString += (int)sessionDuration/60 + "m" + sessionDuration.ToString("n2") + "s" + ","; //Duration
    CSVString += palettes[paletteIndex].title + ","; //Title
    //Save Palette to file
    StreamWriter writer = new StreamWriter(defaultPalettePath + SLASH + title, true); //true to append, false to overwrite
    writer.WriteLine(CSVString); //Filename is palette name
    writer.Close();
  }

  ///***********************************************///
  ///***************PALETTE FUNCTIONS***************///
  ///***********************************************///
  public int getPaletteCount() { 
    if(palettes != null) { return palettes.Count; } else { return -1; }
  }
  public void NewPalette() {
    GameObject newPalette = Instantiate(paletteInstantiator, Vector3.zero, Quaternion.identity);
    newPalette.transform.SetParent(paletteInstantiator.transform.parent, true);
    newPalette.transform.localScale = new Vector3(1,1,1);
    newPalette.GetComponent<RectTransform>().offsetMin = new Vector2(6, 0);
    newPalette.GetComponent<RectTransform>().offsetMax = new Vector2(0, 30);
    newPalette.GetComponent<RectTransform>().anchoredPosition = getPalettePositionByIndex(getPaletteCount());
    newPalette.SetActive(true);
    palettes.Add(new Palette(newPalette));
    selectPalette(getPaletteCount()-1);
  }
  public void DeleteAllPalettes() {
    for(int index = 0; index < getPaletteCount(); index++) { DeletePalette(0); }
  }
  public void DeletePalette() {
    DeletePalette(paletteIndex);
  }
  public void DeletePalette(int index) {
    if(paletteSelector.activeSelf) {
      if(0 <= index && index < getPaletteCount() && getPaletteCount() > 0) {
        //Delete all buttons associated under palette
        for(int i = 0; i < palettes[index].buttons.Count; i++) {          
          Destroy(palettes[paletteIndex].buttons[i].button);
        }
        selectButton(-1);
        //Delete associated palette
        Destroy(palettes[index].palette);
        palettes.RemoveAt(index);
        //Rearrange remaining palettes
        for(int i = 0; i < getPaletteCount(); i++) {
          palettes[i].palette.GetComponent<RectTransform>().anchoredPosition = getPalettePositionByIndex(i);
        }
        selectPalette(-1);
      } else { Debug.LogWarning("Bad palette index! Nothing deleted."); }
    } else { Debug.LogWarning("No palette selected! Nothing deleted."); }
  }
  public void selectPalette() {
    int index = getPaletteIndexByPosition(EventSystem.current.currentSelectedGameObject.GetComponent<RectTransform>().anchoredPosition);
    selectPalette(index, true);
  }
  public void selectPalette(int index, bool save = false) {
  	//Save previous palette if autosave is enabled.
  	if(save && autoSaveLoadPalettes && 0 <= paletteIndex && paletteIndex < getPaletteCount() && getPaletteCount() > 0) { SaveCSVPalette(); }
    //Select Palette.
    if(0 <= index && index < getPaletteCount() && getPaletteCount() > 0) {
      //Update Palette UI.
      paletteTitle.text = palettes[index].title;
      paletteSelector.GetComponent<RectTransform>().anchoredPosition = getPalettePositionByIndex(index) + new Vector2(0,3);
      paletteSelector.SetActive(true);
      //Hide all unselected palette buttons.
      if(paletteIndex > -1) {
        foreach(PaletteButton button in palettes[paletteIndex].buttons) { button.button.SetActive(false); }
      }
      //Show all selected palette buttons.
      paletteIndex = index;
      foreach(PaletteButton button in palettes[paletteIndex].buttons) { button.button.SetActive(true); }
      selectButton(-1);
    } 
    //Deselect Palette.
    else { paletteIndex = -1; paletteSelector.SetActive(false); }
  }
  public void setPaletteTitle(string text) { 
    //NO WEIRD CHARACTERS IN PALETTE TITLE!
    text = Sanitize(text);
    //Otherwise set palette title
    if(0 <= paletteIndex && paletteIndex < getPaletteCount() && getPaletteCount() > 0) {
      paletteTitle.text = text;
      palettes[paletteIndex].title = text; 
      palettes[paletteIndex].palette.name = text; 
      palettes[paletteIndex].palette.transform.GetChild(0).GetComponent<Text>().text = text;
    } else { Debug.LogWarning("Bad palette index! Title not changed."); }
  }
  private Vector2 getPalettePositionByIndex(int index) { return new Vector2(0, index * -31 - 3); }
  private int getPaletteIndexByPosition(Vector2 position) { return (((int)position.y + 3)/-31); }

  ///**********************************************///
  ///***************BUTTON FUNCTIONS***************///
  ///**********************************************///
  public int getCurrentPaletteButtonCount() { 
    if(palettes != null && palettes.Count > 0 && paletteIndex > -1) {    
      if(palettes[paletteIndex].buttons != null) { return palettes[paletteIndex].buttons.Count; }
    } return -1;
  }
  public void NewButton() {
    if(getPaletteCount() == 0) { NewPalette(); }
    GameObject paletteButton = Instantiate(buttonInstantiator, Vector3.zero, Quaternion.identity);
    paletteButton.transform.SetParent(buttonInstantiator.transform.parent, true);
    paletteButton.transform.localScale = new Vector3(1,1,1);
    paletteButton.GetComponent<Image>().color = colors[0].color;
    paletteButton.GetComponent<RectTransform>().anchoredPosition = getButtonPositionByIndex(getCurrentPaletteButtonCount());
    paletteButton.SetActive(true);
    palettes[paletteIndex].buttons.Add(new PaletteButton(paletteButton));
    selectButton(getCurrentPaletteButtonCount()-1);
  }
  public void CopyButton() {
    if(buttonSelector.activeSelf) {
      if(0 <= buttonIndex && buttonIndex < getCurrentPaletteButtonCount() && getCurrentPaletteButtonCount() > 0) {
        GameObject paletteButton = Instantiate(palettes[paletteIndex].buttons[buttonIndex].button, Vector3.zero, Quaternion.identity);
        paletteButton.transform.SetParent(buttonInstantiator.transform.parent, true);
        paletteButton.transform.localScale = new Vector3(1,1,1);
        paletteButton.GetComponent<RectTransform>().anchoredPosition = getButtonPositionByIndex(getCurrentPaletteButtonCount());
        paletteButton.SetActive(true);
        palettes[paletteIndex].buttons.Add(new PaletteButton(paletteButton));
        PaletteButton copiedPalette = palettes[paletteIndex].buttons[buttonIndex];
        selectButton(getCurrentPaletteButtonCount()-1);
        setButtonTitle(copiedPalette.title); 
        setButtonColor(copiedPalette.color);
        setButtonEmotion(copiedPalette.emotion); 
        setSpeechSynthesis(copiedPalette.speech); 
        setSpeechRate(copiedPalette.rate);
        setSpeechPitch(copiedPalette.pitch);
        setShortcutKey(copiedPalette.shortcut);
        setButtonAnimation(copiedPalette.animation);
        setButtonGaze(copiedPalette.gaze);

      } else { Debug.LogWarning("Bad button index! Nothing copied."); }
    } else { Debug.LogWarning("No button selected! Nothing copied."); }
  }
  public void DeleteButton() { DeleteButton(buttonIndex); }
  public void DeleteButton(int index) {
    if(buttonSelector.activeSelf) {
      if(0 <= index && index < getCurrentPaletteButtonCount() && getCurrentPaletteButtonCount() > 0) {
        Destroy(palettes[paletteIndex].buttons[index].button);
        palettes[paletteIndex].buttons.RemoveAt(index);
        for(int i = 0; i < getCurrentPaletteButtonCount(); i++) {
          palettes[paletteIndex].buttons[i].button.GetComponent<RectTransform>().anchoredPosition = getButtonPositionByIndex(i);
        }
        selectButton(-1);
      } else { Debug.LogWarning("Bad button index! Nothing deleted."); }
    } else { Debug.LogWarning("No button selected! Nothing deleted."); }
  }
  public void deselectButton() { selectButton(-1); }
  public void selectButton() {
    int index = getButtonIndexByPosition(EventSystem.current.currentSelectedGameObject.GetComponent<RectTransform>().anchoredPosition);
    selectButton(index, true, true);
  }
  public void selectButton(int index, bool autoSend = false, bool save = false) {
    //Save previous palette if autosave is enabled.
    if(save && autoSaveLoadPalettes && 0 <= paletteIndex && paletteIndex < getPaletteCount() && getPaletteCount() > 0 && palettes[paletteIndex].buttons.Count > 0) { SaveCSVPalette(); }
    //Select Button
    if(paletteIndex > -1) {
      if(0 <= index && index < getCurrentPaletteButtonCount() && getCurrentPaletteButtonCount() > 0) {
        PaletteButton button = palettes[paletteIndex].buttons[index];
        buttonIndex = index;
        buttonTitle.text = button.title;

        buttonColor.value = button.color;
        buttonEmotion.value = button.emotion;

        buttonAnimation.text = button.animation;
        buttonGaze.text = button.gaze;
        
        speechSynthesis.text = button.speech;
        speechRate.value = button.rate;
        speechPitch.value = button.pitch;
        if(shortcutDropdown) { shortcutDropdown.value = button.shortcut; }
        revert = new PaletteButton(buttonInstantiator, button.title, button.color, button.emotion, button.speech, button.rate, button.pitch);
        buttonSelector.GetComponent<RectTransform>().anchoredPosition = getButtonPositionByIndex(index) + new Vector2(-3,3);
        buttonSelector.SetActive(true);
        if(autoSend && autoSendToggle != null && autoSendToggle.isOn) {
          buttonPresses.Add(new ButtonPress(getSpeechSynthesis()));
          sendMessage();
        }
        buttonDuration = 0;
      } 
      //Deselect Button
      else {
        buttonIndex = -1;
        buttonTitle.text = "";
        
        buttonColor.value = 0;
        buttonEmotion.value = 0;
        speechRate.value = 1;
        speechPitch.value = 1;
        
        speechSynthesis.text = "";
        buttonAnimation.text = "";
        buttonGaze.text = "";
        
        if(shortcutDropdown) { shortcutDropdown.value = 0; }
        buttonSelector.SetActive(false);  
      }
    }
  }
  private Vector2 getButtonPositionByIndex(int index) { return new Vector2((index % 4) * 130 + 6, (index / 4) * -60); }
  private int getButtonIndexByPosition(Vector2 position) { return (((int)position.x -6) / 130) + (((int)position.y / -60) * 4); }

  ///**********************************************///
  ///***************SPEECH FUNCTIONS***************///
  ///**********************************************///
  private bool checkValidButton() {
    return (paletteSelector.activeSelf && 0 <= buttonIndex && buttonIndex < getCurrentPaletteButtonCount() && getCurrentPaletteButtonCount() > 0); 
  }
  public void revertButton() {
    if(buttonSelector.activeSelf) {
      if(checkValidButton()) {
        if(revert != null) {
          setButtonTitle(revert.title); 
          setButtonColor(revert.color);
          setButtonEmotion(revert.emotion); 
          setButtonAnimation(revert.animation);
          setButtonGaze(revert.gaze);
          setSpeechSynthesis(revert.speech); 
          setSpeechRate(revert.rate);
          setSpeechPitch(revert.pitch);
        } 
      } else { Debug.LogWarning("Bad button index! Nothing reverted."); }
    } else { Debug.LogWarning("No button selected! Nothing reverted."); }
  }
  public string getButtonTitle() { return buttonTitle.text; }
  public void setButtonTitle(string text) { 
    if(!checkValidButton()) { Debug.LogWarning("Bad button index! Button title not changed."); return; }
    text = text.Replace(";", "");
    buttonTitle.text = text;
    palettes[paletteIndex].buttons[buttonIndex].title = text; 
    palettes[paletteIndex].buttons[buttonIndex].button.name = text; 
    palettes[paletteIndex].buttons[buttonIndex].button.transform.GetChild(0).GetComponent<Text>().text = text; 
    //Set button title as speech text if empty
    if(palettes[paletteIndex].buttons[buttonIndex].speech == null ||
       palettes[paletteIndex].buttons[buttonIndex].speech.Length == 0) { setSpeechSynthesis(text); }
  }
  public int getButtonColor() { return buttonColor.value; }
  public Color32 getButtonColor32() { return colors[buttonColor.value].color; }
  public void setButtonColor(int index) { 
    if(!checkValidButton()) { Debug.LogWarning("Bad button index! Button color not changed."); return; }
    buttonColor.value = index;
    palettes[paletteIndex].buttons[buttonIndex].color = index;
    palettes[paletteIndex].buttons[buttonIndex].button.GetComponent<Image>().color = colors[index].color;
  }
  public int getButtonEmotion() { return buttonEmotion.value; }
  //public string getButtonEmotion() { return emotions[buttonEmotion.value]; }
  public void setButtonEmotion(int index) { 
    if(!checkValidButton())  {Debug.LogWarning("Bad button index! Button emotion not changed."); return; }
    buttonEmotion.value = index;
    palettes[paletteIndex].buttons[buttonIndex].emotion = index; 
  }
  public string getSpeechSynthesis() { return speechSynthesis.text; }
  public void setSpeechSynthesis(string text) { 
    if(!checkValidButton()) { Debug.LogWarning("Bad button index! Speech synthesis not changed."); return; }
    text = text.Replace(",", "");
    speechSynthesis.text = text;  
    palettes[paletteIndex].buttons[buttonIndex].speech = text;   
  }
  public string getButtonAnimation() { return buttonAnimation.text; }
  public void setButtonAnimation(string text) { 
    if(!checkValidButton()) { Debug.LogWarning("Bad button index! Button animation not changed."); return; }
    text = text.Replace(",", "");
    buttonAnimation.text = text;  
    palettes[paletteIndex].buttons[buttonIndex].animation = text;   
  }
  public float getSpeechRate() { return speechRate.value; }
  public void setSpeechRate(float value) { 
    if(!checkValidButton()) { Debug.LogWarning("Bad button index! Speech rate not changed."); return; }
    speechRate.value = value;
    palettes[paletteIndex].buttons[buttonIndex].rate = value;    
  }
  public float getSpeechPitch() { return speechPitch.value; }
  public void setSpeechPitch(float value) { 
    if(!checkValidButton()) { Debug.LogWarning("Bad button index! Speech pitch not changed."); return; }
    speechPitch.value = value;
    palettes[paletteIndex].buttons[buttonIndex].pitch = value;  
  }
  public int getShortcutKey() { return shortcutDropdown.value; }
  public void setShortcutKey(int index) { 
    if(!checkValidButton()) { Debug.LogWarning("Bad button index! Shortcut key not changed."); return; }
    palettes[paletteIndex].buttons[buttonIndex].shortcut = index; 
  }
  public string getButtonGaze() { return buttonGaze.text; }
  public void setButtonGaze(string text) { 
    if(!checkValidButton()) { Debug.LogWarning("Bad button index! Button gaze not changed."); return; }
    text = text.Replace(",", "");
    buttonGaze.text = text;  
    palettes[paletteIndex].buttons[buttonIndex].gaze = text;   
  }
  
  //---Goal/Subgoal/Proficiency String to Index---//
  private int getColorIndexFromString(string name) { name = Sanitize(name);
    for(int i = 0; i < colors.Count; i++) { if (name.Replace(" ","").ToLower() == colors[i].name.Replace(" ","").ToLower()) { return i; } }
    return 0;
  }
  private int getEmotionIndexFromString(string emotion) { emotion = Sanitize(emotion);
    for(int i = 0; i < buttonEmotion.options.Count; i++) { if (emotion.Replace(" ","").ToLower() == buttonEmotion.options[i].text.Replace(" ","").ToLower()) { return i; } }
    return 0;
  }
   
  //Setting target agent ID for multi agents 
  public int getAgentID() { return agentID; } 
  public void setAgentID(int i) { agentID = i; PlayerPrefs.SetInt("AgentID", i); }

  //Setting voice name
  public string getVoice() { return voice; } 
  public void setVoice(string text) { voice = text; voiceField.text = text; PlayerPrefs.SetString("Voice", voice); }

  ///**************************************************///
  ///***************CONNECTION FUNCTIONS***************///
  ///**************************************************///
  public void sendUDPMessage() { udpSender.sendMessage(getJSON()); }
  public void sendMessage() { sendUDPMessage();
    if(localTTSToggle.isOn) { sayTTS(); }
  }
  
  private string getJSON() {     
    //WASEDA Humanoid AI Controller
    Behaviour behaviour = new Behaviour();
    behaviour.agentID = getAgentID();
    behaviour.voice = getVoice();

    behaviour.speech = getSpeechSynthesis();
    behaviour.pitch = getSpeechPitch();
    behaviour.rate = getSpeechRate();
    
    behaviour.emotion =  emotions[getButtonEmotion()];
    behaviour.animation = getButtonAnimation();
    behaviour.gaze = getButtonGaze();

    string json = JsonUtility.ToJson(behaviour);
    Debug.Log(json); 
    return json;
  }

  private string Sanitize(string text) {
    text = text.Replace("<", ""); text = text.Replace(">", ""); text = text.Replace(":", ""); text = text.Replace("\"", ""); 
    text = text.Replace("|", ""); text = text.Replace("?", ""); text = text.Replace("*", ""); return text.Replace(";", "");
  }
}

[System.Serializable] 
public class Behaviour {
  public int agentID = 0;
  public string voice = "";

  public string speech = "";
  public float rate = 1f, pitch = 1f;

  public string animation = "";
  public float animationDuration = 1f;

  public string emotion = "";
  public float emotionAmount = 1f;
  public float emotionDuration = 1f;

  public string gaze = "";
  public float gazeDuration = 0f;
}