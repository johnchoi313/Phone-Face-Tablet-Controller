using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SliderController : MonoBehaviour {
    public AdvancedController advancedController;
    public SliderControl paletteSlider, buttonSlider;
    
	void Start () {
	    paletteSlider.init();
        buttonSlider.init();	
	}
	void Update () {
	    paletteSlider.resize(advancedController.getPaletteCount());
        buttonSlider.resize(advancedController.getCurrentPaletteButtonCount());
    }

    public void updatePaletteSlider(float value) { paletteSlider.reposition(value); }
    public void updateButtonSlider(float value) { buttonSlider.reposition(value); }
}

[System.Serializable]
public class SliderControl {
    public Scrollbar scrollbar;
    public RectTransform content;
    private Vector2 initPos;

    public int rows, columns;
    private int count = 0;

    public float scrollAmount = 60;

    public void resize(int c) {
        count = c;
        Debug.Log(count);
        bool changed = false;

        float oldSize = scrollbar.size;
        float oldValue = scrollbar.value;

        if (count/columns <= rows) { scrollbar.size = 1; changed = true; }
        else { if(count/columns > 0) { scrollbar.size = (float)(rows) / (float)(count/columns); } changed = true; }    
    
        if(changed) { 
            scrollbar.value = oldValue * (scrollbar.size / oldSize); 
            reposition(scrollbar.value);
        }
    }
    public void reposition(float value) {
        if(value < 0) {value = 0;} if(value > 1) {value = 1;}
        content.anchoredPosition = new Vector2(initPos.x, initPos.y+(1f-value)*(count/columns-rows)*scrollAmount);
        scrollbar.value = value;
    }
    public void init() {
        initPos = content.anchoredPosition; 
    }
}
