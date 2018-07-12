using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class InitalDisableXR : MonoBehaviour {

	// Use this for initialization
	void Start () {
        XRSettings.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
