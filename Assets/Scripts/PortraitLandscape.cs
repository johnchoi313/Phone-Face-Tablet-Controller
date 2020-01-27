using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PortraitLandscape : MonoBehaviour {

	public enum Orientation { LandscapeLeft, LandscapeRight, PortraitLeft, PortraitRight };
	private Orientation orientation = Orientation.LandscapeLeft;

	public Vector3 landscapePosition, portraitPosition;
    public ParticleSystem singleNotes, doubleNotes;
	public Transform mainCamera;
	public Dropdown dropdown;

	public void switchOrientation() {
		switch(orientation) {
			case Orientation.LandscapeLeft: setOrientation(Orientation.LandscapeRight); break;
			case Orientation.LandscapeRight: setOrientation(Orientation.PortraitLeft); break;
			case Orientation.PortraitLeft: setOrientation(Orientation.PortraitRight); break;
			case Orientation.PortraitRight: setOrientation(Orientation.LandscapeLeft); break;
		}
	}
	public void setOrientation(int mode) { mode = mode % 4;
		switch(mode) {
			case 0: setOrientation(Orientation.LandscapeLeft); break;
			case 1: setOrientation(Orientation.LandscapeRight); break;
			case 2: setOrientation(Orientation.PortraitLeft); break;
			case 3: setOrientation(Orientation.PortraitRight); break;
		}
	}
	public void setOrientation(Orientation orient) {
		//Set orientation position
		orientation = orient;
		mainCamera.position = (orient == Orientation.LandscapeLeft || orient == Orientation.LandscapeRight) ? landscapePosition : portraitPosition;
		//Set orientation angle
		int mode = 0; float faceAngle = 0, noteAngle = 0; 
		switch(orientation) {
			case Orientation.LandscapeLeft: mode = 0; faceAngle = 0; noteAngle = 0; break;
			case Orientation.LandscapeRight: mode = 1; faceAngle = 180; noteAngle = 3.14f; break;
			case Orientation.PortraitLeft: mode = 2; faceAngle = -90; noteAngle = -1.57f; break;
			case Orientation.PortraitRight: mode = 3; faceAngle = 90; noteAngle = 1.57f; break;
		}
    	PlayerPrefs.SetInt("Orientation", mode);
		if(dropdown != null) { dropdown.value = mode; }
    	mainCamera.localEulerAngles = new Vector3(0, 0, faceAngle);
        if(singleNotes != null) { singleNotes.startRotation = noteAngle; }
        if(doubleNotes != null) { doubleNotes.startRotation = noteAngle; }
	}

	//On start, load orientation
	void Start() { setOrientation(PlayerPrefs.GetInt("Orientation", 0)); }
	//On [O] press, switch orientation
	void Update () { if(Input.GetKeyDown(KeyCode.O)) { switchOrientation(); } }

}
