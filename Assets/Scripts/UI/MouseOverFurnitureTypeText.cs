﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseOverFurnitureTypeText : MonoBehaviour {

    //Every frame, this script checks to see which tile is under the mouse and then updates
    //the GetComponent<Text>.text parameter of the object ist is attached to. 

    Text myText;
    MouseController mouseController;

	// Use this for initialization
	void Start () {
        myText = GetComponent<Text>();

        if(myText == null) {
            Debug.LogError("MouseOverFurnitureTypeText: No 'Text' UI component on this object.");
            this.enabled = false;
            return;
        }

        mouseController = GameObject.FindObjectOfType<MouseController>();
        if(mouseController == null) {
            Debug.LogError("How do we not have an instance of mouse controller?");
            return;
        }
	}
	
	// Update is called once per frame
	void Update () {
        Tile t = mouseController.GetMouseOverTile();

        if (t == null)
            return;

        string s = "NULL";
        if(t.furniture != null) {
            s = t.furniture.objectType;
        }

        myText.text = "Furniture Type: " + s;
	}
}
