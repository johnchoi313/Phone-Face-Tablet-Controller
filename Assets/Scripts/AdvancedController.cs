using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using Crosstales.RTVoice;
using Crosstales.RTVoice.Model;
using TechTweaking.Bluetooth;
using SimpleFileBrowser;

public class AdvancedController : MonoBehaviour {

  [Header("Connection Variables")]
  public BluetoothConnection bluetoothSender;
  public UDPSender udpSender;
  public Misty misty;
  public Toggle autoSendBluetooth;
  public Toggle autoSendUDPIP;
  public Toggle autoSendMisty;
  public GameObject emailPanel;
  
  [Header("Palette Variables")]
  public GameObject paletteSelector;
  public GameObject paletteInstantiator;
  public InputField paletteTitle;
  public InputField GoogleSheetsURL;
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
  public InputField buttonTitle;
  public Dropdown buttonColor;
  public Dropdown buttonEmotion;
  public Dropdown shortcutDropdown;
  public Slider speechPitch;
  public Slider speechRate;
  
  [Header("Color Settings")]
  public List<ColorMap> colors;

  [Header("Other Variables")]
  public GameObject androidSaveLoadBackground;

  ///***********************************************///
  ///***************UPDATE FUNCTIONS****************///
  ///***********************************************///

  public void sayTTS() {
    if(speechSynthesis.text != null && speechSynthesis.text.Length > 0) {
      Speaker.Speak(speechSynthesis.text, null, null, true, speechRate.value, speechPitch.value, 1); //rate = 0-3, pitch = 0-2, volume = 0-1
    }                
  }
  public void sayMistyTTS() {
    if(speechSynthesis.text != null && speechSynthesis.text.Length > 0) {
      misty.SayTTS(getSpeechSynthesis(), getSpeechRate(), getSpeechPitch(), true);
    }                
  }


  //Checking for Keyboard Shortcuts
  void Update() {
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

      if(charKey > 0) {
        if(paletteIndex > -1) {
          for(int b = 0; b < getCurrentPaletteButtonCount(); b++) {
            if (palettes[paletteIndex].buttons[b].shortcut == charKey) {
              selectButton(b); sendBluetoothMessage(); sendUDPMessage();
            }
          }
        }
      }
    }
  }

  ///***********************************************///
  ///***********INITIALIZATION FUNCTIONS************///
  ///***********************************************///

  //Initializing Palettes.
  void Start() {
    //Check if anything is null
    if(!buttonTitle)      { Debug.LogError("buttonTitle is null!"); }
    if(!buttonColor)      { Debug.LogError("buttonColor is null!"); }
    if(!buttonEmotion)    { Debug.LogError("buttonEmotion is null!"); }
    if(!speechSynthesis)  { Debug.LogError("speechSynthesis is null!"); }
    if(!speechRate)       { Debug.LogError("speechRate is null!"); }
    if(!speechPitch)      { Debug.LogError("speechPitch is null!"); }
    if(!shortcutDropdown) { Debug.LogWarning("shortcutDropdown is null!"); }

  	SLASH = (Application.platform == RuntimePlatform.Android)?"/":"\\";
    //defaultPalettePath = Application.streamingAssetsPath + "/Palettes";
    defaultPalettePath = Application.persistentDataPath; //SLASH+ "/Palettes";
    palettes = new List<Palette>();
    InitSimpleFileBrowser();
    initColorDropdown();
    //Optionally, load all palettes.
    if(autoSaveLoadPalettes) { LoadAllCSVPalettes(); }
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
    /* PygmyMonkey.FileBrowser.FileBrowser.OpenMultipleFilesPanel("Load CSV Palette(s)", getPalettePathFolder(), new string[] { "CSV" }, "Open", (bool canceled, string[] filePathArray) => { if (canceled) { return; }
      for (int i = 0; i < filePathArray.Length; i++) {
        if(filePathArray[i].Substring(filePathArray[i].Length - 4).ToLower().Equals(".csv")) {
          LoadCSVPalette(filePathArray[i]);
        } else { Debug.LogWarning("Failed to load \"filePathArray[i]\"! Only .csv files can be loaded!"); }
      }
    }); */
  } 
  public void LoadAllCSVPalettes() {
    //DirectoryInfo directory = new DirectoryInfo(getPalettePathFolder());
    //FileInfo[] info = directory.GetFiles("*.csv"); 
    //foreach(FileInfo file in info) {  LoadCSVPalette(getPalettePathFolder() + "/" + file.Name  + ".csv"); }
    string[] filePaths = System.IO.Directory.GetFiles(getPalettePathFolder(), "*.csv");
    foreach(string filePath in filePaths) { LoadCSVPalette(filePath); }
  }
  public void LoadCSVPalette(string path) {
    //NO WEIRD FILE NAMES!
    string title = getPaletteNameFromFilePath(path);   
    if(title.Contains("<") || title.Contains(">") || title.Contains(":") || title.Contains("\"") || 
       title.Contains("|") || title.Contains("?") || title.Contains("*") || title.Contains(";")) {
      Debug.LogWarning("Tried to open file with restricted characters in name. Ignoring: " + title);
      return;
    }
    //Load files.
  	Debug.Log("Load: " + path);
    StreamReader reader = new StreamReader(path); 
    string encodedString = reader.ReadToEnd().ToLower();
    reader.Close();
    string[][] table = CsvParser2.Parse(encodedString);    
    LoadPalette(table, title);
    PlayerPrefs.SetString("PalettePath", getPalettePathFolder(path));
  }
  public void LoadGoogleSheetsPalette() {
    string url = GoogleSheetsURL.text;
    LoadGoogleSheetsPalette(url);
  }
  public void LoadGoogleSheetsPalette(string url) {
    //Download CSV from Public Google Sheets
    string csvurl = "";
    if(url.Substring(url.LastIndexOf("/")).Length < 25) {
      csvurl = url.Substring(0,url.LastIndexOf("/")) + "/export?format=csv&gid=0";
    } else { 
      csvurl = url + "/export?format=csv&gid=0";
    }
    Debug.Log(csvurl);
    WWW data = new WWW (csvurl);
    while(!data.isDone) {}
    //Load CSV
    string[][] table = CsvParser2.Parse(data.text); 
    string title = GetGoogleSheetTitle(url);
    LoadPalette(table, title);
  }
  private string GetGoogleSheetTitle(string url) {
      //Download Html
      WWW data = new WWW (url);
      while(!data.isDone){}
      // Define a regular expression for finding target.
      Regex rx = new Regex(@"<meta property=""og:title"" content="".*?"">", RegexOptions.IgnoreCase);
      //Find Matches.
      MatchCollection matches = rx.Matches(data.text);
      Debug.Log("Title found: " + matches.Count);
      // Use match data to fill in slide data
      if(matches.Count > 0) {  
        return matches[0].Value.Substring(35, matches[0].Value.Length - 37); //Apply title.
      } else {
        return ""; //No title found.
      }
  }
  private void LoadPalette(string[][] table, string name = "Palette Title") {
    //Create a new palette:
    NewPalette();
    setPaletteTitle(name);
    //Add each line of CSV File
    for(int i = 1; i < table.Length; i++) {
      //Create a new button
      NewButton();
      selectButton(getCurrentPaletteButtonCount()-1);
      //Set Button Title
      string title = table[i][0];
      setButtonTitle(title); 
      //Set Button Color
      int color = 0; if(!int.TryParse(table[i][1], out color)) 
      { Debug.LogWarning("Table ["+i+"][1] must be int!"); }
      setButtonColor(color);
      //Set Button Emotion
      int emotion = 0; if(!int.TryParse(table[i][2], out emotion))
      { Debug.LogWarning("Table ["+i+"][2] must be int!"); }
      setButtonEmotion(emotion); 
      //Set Button Speech
      string speech = table[i][3];
      setSpeechSynthesis(speech); 
      //Set Button Rate
      float rate = 0; if(!float.TryParse(table[i][4], out rate))
      { Debug.LogWarning("Table ["+i+"][4] must be float!"); }
      setSpeechRate(rate);
      //Set Button Pitch
      float pitch = 0; if(!float.TryParse(table[i][5], out pitch))
      { Debug.LogWarning("Table ["+i+"][5] must be float!"); }
      setSpeechPitch(pitch); 
      //Set Shortcut Key
      if(shortcutDropdown) {
        int key = 0; if(!int.TryParse(table[i][6], out key))
        { Debug.LogWarning("Table ["+i+"][6] must be int!"); }
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
    /* PygmyMonkey.FileBrowser.FileBrowser.SaveFilePanel("Save CSV Palette", "", getPalettePathFolder(), null, new string[] { "CSV" }, null, (bool canceled, string filePath) => { if (canceled) { return; }
      if(filePath.Length > 4 && !filePath.Substring(filePath.Length - 4).ToLower().Equals(".csv")) { filePath = filePath + ".csv"; }
      SaveCSVPalette(filePath);
    }); */
  }
  public void SaveAllCSVPalettes() {
    if (getPaletteCount() == 0) { Debug.Log("No palettes to save! Nothing saved."); }
    for(int i = 0; i < getPaletteCount(); i++) {
      selectPalette(i);
      SaveCSVPalette(getPalettePathFolder() + SLASH + palettes[i].title  + ".csv");
    }
  }
  public void SaveCSVPalette() {
    SaveCSVPalette(getPalettePathFolder() + SLASH + palettes[paletteIndex].title  + ".csv");
  }
  public void SaveCSVPalette(string path) {
    //DON'T SAVE FILE WITH WEIRD CHARACTERS NAME
    string title = getPaletteNameFromFilePath(path);   
    if(title.Contains("<") || title.Contains(">") || title.Contains(":") || title.Contains("\"") || 
       title.Contains("|") || title.Contains("?") || title.Contains("*") || title.Contains(";")) {
      Debug.LogWarning("Tried to save file with restricted characters in name. Ignoring: " + title);
      return;
    }
    
    Debug.Log("Save: " + path);
    if(paletteSelector.activeSelf) {
      //Change Palette Name to Path Name
      setPaletteTitle(getPaletteNameFromFilePath(path));
      //Create CSV String
      string CSVString = "TITLE(text),COLOR(0-7),EMOTION(0-7),SPEECH(text),RATE(0.0-3.0),PITCH(0.0-2.0)"; 
      //Add button info to each row
      foreach(PaletteButton button in palettes[paletteIndex].buttons) {
        CSVString += "\r\n";
        CSVString += button.title + ",";
        CSVString += button.color + ",";
        CSVString += button.emotion + ",";
        CSVString += button.speech + ",";
        CSVString += button.rate.ToString("F2") + ",";
        CSVString += button.pitch.ToString("F2") + ",";
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
    if(paletteIndex > -1) {
      return (getPalettePathFolder() + SLASH + palettes[paletteIndex].title  + ".csv"); 
    } else {
      return null;
    }
  }
  public string[] getPalettePaths() {
    string[] paths = new string[getPaletteCount()];
    for(int i = 0; i < getPaletteCount(); i++) { paths[i] = getPalettePathFolder() + SLASH + palettes[i].title  + ".csv"; }
    return paths;
  }
  private string getPalettePathFolder(string path = null) {
    if(path == null || path.LastIndexOf(SLASH) < 0) { return PlayerPrefs.GetString("PalettePath", defaultPalettePath); }
    //if(path == null || path.LastIndexOf(SLASH) < 0) { return defaultPalettePath; }
    return path.Substring(0, path.LastIndexOf(SLASH));
  }
  private string getPaletteNameFromFilePath(string path) {
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
    selectPalette(index);
  }
  public void selectPalette(int index) {
  	//Save previous palette if autosave is enabled.
  	if(autoSaveLoadPalettes && 0 <= paletteIndex && paletteIndex < getPaletteCount() && getPaletteCount() > 0) { SaveCSVPalette(); }
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
    text = text.Replace("<", ""); text = text.Replace(">", ""); 
    text = text.Replace(":", ""); text = text.Replace("\"", ""); 
    text = text.Replace("|", ""); text = text.Replace("?", "");
    text = text.Replace("*", ""); text = text.Replace(";", "");
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
    } 
    return -1;
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
    selectButton(index, true);
  }
  public void selectButton(int index, bool autoSend = false) {
    //Select Button
    if(paletteIndex > -1) {
      if(0 <= index && index < getCurrentPaletteButtonCount() && getCurrentPaletteButtonCount() > 0) {
        PaletteButton button = palettes[paletteIndex].buttons[index];
        buttonIndex = index;
        buttonTitle.text = button.title;
        buttonColor.value = button.color;
        buttonEmotion.value = button.emotion;
        speechSynthesis.text = button.speech;
        speechRate.value = button.rate;
        speechPitch.value = button.pitch;
        if(shortcutDropdown) { shortcutDropdown.value = button.shortcut; }
        revert = new PaletteButton(buttonInstantiator, button.title, button.color, button.emotion, button.speech, button.rate, button.pitch);
        buttonSelector.GetComponent<RectTransform>().anchoredPosition = getButtonPositionByIndex(index) + new Vector2(-3,3);
        buttonSelector.SetActive(true);
        if(autoSend) {
          if(autoSendUDPIP != null && autoSendUDPIP.isOn){sendUDPMessage();}
          if(autoSendBluetooth != null && autoSendBluetooth.isOn){sendBluetoothMessage();}
          if(autoSendMisty != null && autoSendMisty.isOn){sendMistyMessage();}
        }
      } 
      //Deselect Button
      else {
        buttonIndex = -1;
        buttonTitle.text = "";
        buttonColor.value = 0;
        buttonEmotion.value = 0;
        speechSynthesis.text = "";
        speechRate.value = 1;
        speechPitch.value = 1;
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
          setSpeechSynthesis(revert.speech); 
          setSpeechRate(revert.rate);
          setSpeechPitch(revert.pitch);
        } 
      } else { Debug.LogWarning("Bad button index! Nothing reverted."); }
    } else { Debug.LogWarning("No button selected! Nothing reverted."); }
  }
  public string getButtonTitle() { return buttonTitle.text; }
  public void setButtonTitle(string text) { 
    text = text.Replace(";", "");
    if(checkValidButton()) {
      buttonTitle.text = text;
      palettes[paletteIndex].buttons[buttonIndex].title = text; 
      palettes[paletteIndex].buttons[buttonIndex].button.name = text; 
      palettes[paletteIndex].buttons[buttonIndex].button.transform.GetChild(0).GetComponent<Text>().text = text; 
      //Set button title as speech text if empty
      if(palettes[paletteIndex].buttons[buttonIndex].speech == null ||
         palettes[paletteIndex].buttons[buttonIndex].speech.Length == 0) { setSpeechSynthesis(text); }
    } else { Debug.LogWarning("Bad button index! Button title not changed."); }
  }
  public int getButtonColor() { return buttonColor.value; }
  public Color32 getButtonColor32() { return colors[buttonColor.value].color; }
  public void setButtonColor(int index) { 
    if(checkValidButton()) {
      buttonColor.value = index;
      palettes[paletteIndex].buttons[buttonIndex].color = index;
      palettes[paletteIndex].buttons[buttonIndex].button.GetComponent<Image>().color = colors[index].color;
    } else { Debug.LogWarning("Bad button index! Button color not changed."); }
  }
  public int getButtonEmotion() { return buttonEmotion.value; }
  public void setButtonEmotion(int index) { 
    if(checkValidButton()) {
      buttonEmotion.value = index;
      palettes[paletteIndex].buttons[buttonIndex].emotion = index; 
    } else { Debug.LogWarning("Bad button index! Button emotion not changed."); }
  }
  public string getSpeechSynthesis() { return speechSynthesis.text; }
  public void setSpeechSynthesis(string text) { 
    text = text.Replace(",", "");
    if(checkValidButton()) {
      speechSynthesis.text = text;  
      palettes[paletteIndex].buttons[buttonIndex].speech = text;   
    } else { Debug.LogWarning("Bad button index! Speech synthesis not changed."); }
  }
  public float getSpeechRate() { return speechRate.value; }
  public void setSpeechRate(float value) { 
    if(checkValidButton()) {
      speechRate.value = value;
      palettes[paletteIndex].buttons[buttonIndex].rate = value;    
    } else { Debug.LogWarning("Bad button index! Speech rate not changed."); }
  }
  public float getSpeechPitch() { return speechPitch.value; }
  public void setSpeechPitch(float value) { 
    if(checkValidButton()) {
      speechPitch.value = value;
      palettes[paletteIndex].buttons[buttonIndex].pitch = value;  
    } else { Debug.LogWarning("Bad button index! Speech pitch not changed."); }
  }
  public int getShortcutKey() { return shortcutDropdown.value; }
  public void setShortcutKey(int index) { 
    if(checkValidButton()) {
      palettes[paletteIndex].buttons[buttonIndex].shortcut = index; 
    } else { Debug.LogWarning("Bad button index! Shortcut key not changed."); }
  }

  ///**************************************************///
  ///***************CONNECTION FUNCTIONS***************///
  ///**************************************************///
  public void sendBluetoothMessage() { bluetoothSender.sendMessage(getJSON()); }
  public void sendUDPMessage() { udpSender.sendMessage(getJSON()); }
  
  public void sendMistyMessage() { 
    misty.ChangeLED(getButtonColor32().r, getButtonColor32().g, getButtonColor32().b);
    misty.ChangeImage(buttonEmotion.options[buttonEmotion.value].text + ".jpg");
    misty.SayTTS(getSpeechSynthesis(), getSpeechRate(), getSpeechPitch());
  }

  public void sendBluetoothBlink() { bluetoothSender.sendMessage("blink"); }
  public void sendUDPBlink() { udpSender.sendMessage("blink"); }
  
  public void showEmailPanel() { emailPanel.SetActive(true); }
  public void hideEmailPanel() { emailPanel.SetActive(false); }

  private string getJSON() { 
    //FAM JSON
    JSONObject json = new JSONObject();
    json.AddField("ra", (int)(getSpeechRate() * 100));
    json.AddField("v", (int)(getSpeechPitch() * 100));
    json.AddField("e", getButtonEmotion());
    json.AddField("s", getSpeechSynthesis());
    json.AddField("r", getButtonColor32().r);
    json.AddField("g", getButtonColor32().g);
    json.AddField("b", getButtonColor32().b);
    Debug.Log(json.ToString());
    return json.ToString();
    //ASPIR JSON
  }

}

///**************************************************///
///***************SUBCLASSES FUNCTIONS***************///
///**************************************************///
[System.Serializable]
public class ColorMap {
  public string name;
  public Color32 color;
  public ColorMap(string n, Color32 c) { name = n; color = c; }
}  

[System.Serializable]
public class Palette {
  public GameObject palette;
  public string title;
  public string path;
  public List<PaletteButton> buttons;
  public Palette(GameObject p, string t = "Palette Title", string PATH = null) {
    palette = p;
    title = t;
    path = PATH;
    buttons = new List<PaletteButton>();
  }
}

[System.Serializable]
public class PaletteButton {
  public GameObject button;
  public string title;
  public int color;
  public int emotion;
  public string speech;
  public float rate;
  public float pitch;
  public int shortcut;
  public PaletteButton(GameObject b, 
                       string t = "Button Title", 
                       int c = 0, int e = 0, 
                       string s = "", 
                       float r = 1, float p = 1, int key = 0) {
    button = b; button.name = t;
    title = t;
    color = c;
    emotion = e;
    speech = s;
    rate = r;
    pitch = p;
    shortcut = key;
  }
}
