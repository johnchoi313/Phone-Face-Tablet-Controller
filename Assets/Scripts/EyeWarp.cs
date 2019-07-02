using UnityEngine;
using System.Collections;

public class EyeWarp : MonoBehaviour {

    public Vector3 mag;
    public Vector3 off;
    public float spd;
    public float blink;

    private Vector3 size;
    private Vector3 pos;

    //face tracking
    public OpenCvSharp.Demo.FaceDetectorScene faceDetector;
    public float lookAmount = 1f;
    public float lookSpeed = 1f;

    void Start() {
        size = transform.localScale;
        pos = transform.localPosition;
    }

    void Update() {
        //Update scale
        transform.localScale = new Vector3(size.x + mag.x * Mathf.Sin(off.x + Time.time * spd),
                                          (size.y + mag.y * Mathf.Sin(off.y + Time.time * spd)) * Mathf.Abs(blink),
                                          (size.z + mag.z * Mathf.Sin(off.z + Time.time * spd)) * Mathf.Abs(blink));
        
        //Update position
        if(faceDetector != null) {
            Vector3 newpos = pos + new Vector3(faceDetector.faceCenter.x * lookAmount, -faceDetector.faceCenter.y * lookAmount, 0);
            transform.localPosition = Vector3.Lerp(transform.localPosition, newpos, Time.deltaTime * lookSpeed);
        }
    }
}

