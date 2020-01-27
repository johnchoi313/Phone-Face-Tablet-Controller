using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;

using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class MP3MusicPlayer : MonoBehaviour {

    public string SLASH;
    public string musicFolderPath;

    public GameObject musicFolderBackground;
    //public Text musicFolderPathText;

    public AudioImporter importer;
    public AudioSource audioSource;

    public ParticleSystem singleNotes;
    public ParticleSystem doubleNotes;

    //Initializing Palettes.
    void Start() {    
        InitSimpleFileBrowser();
        SLASH = (Application.platform == RuntimePlatform.Android)?"/":"\\";
        musicFolderPath = PlayerPrefs.GetString("MusicFolderPath", Application.persistentDataPath); //SLASH+ "/Palettes";
    }

    ///***********************************************///
    ///***************MP3 MUSIC PLAYER****************///
    ///***********************************************///
    public void PlayMP3FromFile(string file) { PlayMP3FromPath(musicFolderPath + SLASH + file); }
    public void PlayMP3FromPath(string path) {
        StopMP3();
        if(!File.Exists(path)) { Debug.LogWarning("Music file does not exist in this path: " + path); return; } 
        if(!(path.Length > 4 && path.Substring(path.Length - 4).ToLower() == ".mp3")) {
            Debug.LogWarning("Music file must be MP3! File not loaded."); return;  
        }
        if (importer.isDone) { Destroy(audioSource.clip); }
        StartCoroutine(Import(path));
    }
    IEnumerator Import(string path) {
        importer.Import(path);
        while (!importer.isDone) { yield return null; }
        Debug.Log("Successfully playing MP3 from path: " + path); 
        audioSource.clip = importer.audioClip;
        audioSource.Play();
        singleNotes.Play();
        doubleNotes.Play();

    }
    public void StopMP3() {
        singleNotes.Stop();
        doubleNotes.Stop();
        if(audioSource != null && audioSource.clip != null) { 
            audioSource.Stop();
        }
    }


    ///***********************************************///
    ///***************FOLDER SELECTION****************///
    ///***********************************************///
    public void SimpleFileBrowserLoad() {
        musicFolderBackground.SetActive(true);
        StartCoroutine( SimpleFileBrowserLoadCoroutine() );
    }
    IEnumerator SimpleFileBrowserLoadCoroutine() {
        yield return SimpleFileBrowser.FileBrowser.WaitForLoadDialog( true, musicFolderPath, "Select Music MP3 Folder", "Select Folder" );
        if(SimpleFileBrowser.FileBrowser.Success) { 
            musicFolderPath = SimpleFileBrowser.FileBrowser.Result; 
            PlayerPrefs.SetString("MusicFolderPath", musicFolderPath);
        }
        musicFolderBackground.SetActive(false);
    }

    //test debug play music file.
    public void SimpleFileBrowserPlay() {
        musicFolderBackground.SetActive(true);
        StartCoroutine( SimpleFileBrowserPlayCoroutine() );
    }
    IEnumerator SimpleFileBrowserPlayCoroutine() {
        yield return SimpleFileBrowser.FileBrowser.WaitForLoadDialog( false, musicFolderPath, "Select Music MP3 File", "Play MP3" );
        if(SimpleFileBrowser.FileBrowser.Success) { PlayMP3FromPath(SimpleFileBrowser.FileBrowser.Result); }
        musicFolderBackground.SetActive(false);
    }
    
    //Initializing Save/Load Browser
    void InitSimpleFileBrowser() {  //Android only.
        SimpleFileBrowser.FileBrowser.SetFilters( true, new SimpleFileBrowser.FileBrowser.Filter("Music",".mp3"));
        SimpleFileBrowser.FileBrowser.SetDefaultFilter( ".mp3" );
        SimpleFileBrowser.FileBrowser.SingleClickMode = true;
        SimpleFileBrowser.FileBrowser.RequestPermission();
    }

}
