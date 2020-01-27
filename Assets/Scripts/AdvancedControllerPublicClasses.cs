using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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
public class EmotionMap {
  public string defaultEmotion;
  public string mistyEmotion;
}  

[System.Serializable]
public class GoalMap {
  public string goal;
  public List<string> subgoals;
  [HideInInspector]
  public Dropdown subgoalDropdown;
  [HideInInspector] //Proficiency measurements  
  public int none, exposure, understanding, practicing, demonstrating = 0; 
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
public class ButtonPress {
  public string timestamp;
  public string speech;
  public int goal;
  public int subgoal;
  public int proficiency;
  public ButtonPress(string S = "", int g = 0, int s = 0, int p = 0) {
    timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt");
    speech = S;
    goal = g;
    subgoal = s;
    proficiency = p;
  }
}

[System.Serializable]
public class PaletteButton {
  public GameObject button;
  public string title;
  public int color;
  public int emotion;
  public string speech;
  public string animation;
  public string gaze;

  public float rate;
  public float pitch;
  public int shortcut;

  public int goal;
  public int subgoal;
  public int proficiency;

  public PaletteButton(GameObject b, 
                       string t = "Button Title", 
                       int c = 0, int e = 0, 
                       string s = "", 
                       float r = 1, float p = 1, int key = 0, 
                       int g = 0, int sg = 0, int pr = 0, string a = "None", string ga = "None") {
    button = b; button.name = t;
    title = t;
    color = c;
    emotion = e;
    speech = s;
    rate = r;
    pitch = p;
    shortcut = key;
    goal = g;
    subgoal = sg;
    proficiency = pr;
    animation = a;
    gaze = ga; 
  }
}