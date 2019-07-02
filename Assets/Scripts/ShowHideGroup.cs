using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ShowHideGroup: MonoBehaviour {
    public Group[] groups;
    private int groupIndex;
    public Dropdown dropdown;

    public void showGroup(int index) {
    	if(0 > index || index >= groups.Length) { index = 0; }
    	for(int i = 0; i < groups.Length; i++) {
    		if (i == index) { groups[i].show(); } 
    		else { groups[i].hide(); }
    	}
    	PlayerPrefs.SetInt("Face", index);
    	groupIndex = index;	
    	if(dropdown != null) { dropdown.value = index; }
    }

	void Start() {
    	showGroup(PlayerPrefs.GetInt("Face",0));
    }
    void Update () {
		if(Input.GetKeyDown(KeyCode.F)) { 
			groupIndex = (groupIndex + 1) % groups.Length;
			showGroup(groupIndex);
		}
	}

}

[System.Serializable]
public class Group {
	public string name = "";
	public GameObject[] elements;
	private bool isOn = false;

	public void show() { showHide(true); }
	public void hide() { showHide(false); }
	public void showHide(bool on) {
		foreach(GameObject element in elements) { element.SetActive(on); }	
		isOn = on;
	}
}