using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSortingLayer : MonoBehaviour {

    public string sortingLayerName = "default";

	// Use this for initialization
	void Start () {
        GetComponent<Renderer>().sortingLayerName = sortingLayerName;
	}
	
}
