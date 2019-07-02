using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PortraitLandscape : MonoBehaviour {

	public enum Orientation { Landscape, Portrait };
	private Orientation orientation = Orientation.Landscape;

	public Vector3 landscapePosition;
	public Vector3 portraitPosition;
	public Transform mainCamera;
	public Dropdown dropdown;

	public void switchOrientation() {
		setOrientation((orientation == Orientation.Landscape) ? Orientation.Portrait : Orientation.Landscape);
	}
	public void setOrientation(int mode) {
		if(mode < 0 || mode > 1) { mode = 0; }
		setOrientation((mode == 0) ? Orientation.Landscape : Orientation.Portrait);
	}
	public void setOrientation(Orientation orient) {
		orientation = orient;
		mainCamera.position = (orient == Orientation.Landscape) ? landscapePosition : portraitPosition;
		mainCamera.localEulerAngles = (orient == Orientation.Landscape) ? new Vector3(0,0,0) : new Vector3(0,0,-90); 
		PlayerPrefs.SetInt("Orientation", (orient == Orientation.Landscape) ? 0 : 1);
		if(dropdown != null) { dropdown.value = (orient == Orientation.Landscape) ? 0 : 1; }
    }

	void Start() {
		setOrientation((PlayerPrefs.GetInt("Orientation", 0) == 0) ? Orientation.Landscape : Orientation.Portrait); 
	}
	void Update () {
		if(Input.GetKeyDown(KeyCode.O)) { switchOrientation(); }
	}

}
