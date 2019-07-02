using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeverSleep : MonoBehaviour {
    void Start() { Screen.sleepTimeout = SleepTimeout.NeverSleep; }
}
