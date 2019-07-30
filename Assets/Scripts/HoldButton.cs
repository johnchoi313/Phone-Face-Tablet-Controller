using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    public bool pressed;
    public void OnPointerDown(PointerEventData eventData){ pressed = true; Debug.Log("Pressed!"); }
    public void OnPointerUp(PointerEventData eventData){ pressed = false; Debug.Log("Released!"); }
}
