using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class FurnitureSpriteController : MonoBehaviour {

    Dictionary<Furniture, GameObject> furnitureGameObjectMap;

    Dictionary<string, Sprite> furnitureSprites;

    World world {
        get { return WorldController.Instance.world;  }
    }

	// Use this for initialization
	void Start () {

        LoadSprites();

		// Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();

        world.RegisterFurnitureCreated(OnFurnitureCreated);

        //Go through any EXISTING furniture and call the OnCreated event manually?
        foreach(Furniture furn in world.furnitures) {
            OnFurnitureCreated(furn);
        }
	}

    void LoadSprites(){
        furnitureSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Furniture/");

        foreach(Sprite s in sprites){
            //Debug.Log(s);
            furnitureSprites[s.name] = s;
        }
    }
    
    public void OnFurnitureCreated(Furniture furn){
        //Debug.Log("OnFurnitureCreated");
        //Create a visual GameObject linked to this data.

        //FIXME: Does not consider multi-tile objects nor rotated objects

        // This creates a new GameObject and adds it to our scene.
		GameObject furn_go = new GameObject();

		// Add our tile/GO pair to the dictionary.
		furnitureGameObjectMap.Add( furn, furn_go );

		furn_go.name = furn.objectType + "_" + furn.tile.X + "_" + furn.tile.Y;
		furn_go.transform.position = new Vector3( furn.tile.X, furn.tile.Y, 0);
		furn_go.transform.SetParent(this.transform, true);

        //FIXME: This hardcoding is not ideal!
        if(furn.objectType == "Door") {
            //By default, the door graphic is meant for walls to the east & west
            //Check to see if we actually have a wall north/south, and if so then rotate this GO by 90 degrees.

            Tile northTile = world.GetTileAt(furn.tile.X, furn.tile.Y + 1);
            Tile southTile = world.GetTileAt(furn.tile.X, furn.tile.Y + 1);

            if(northTile != null && southTile != null && northTile.furniture != null && southTile.furniture != null &&
                northTile.furniture.objectType == "Wall" && southTile.furniture.objectType == "Wall") {
                furn_go.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }

        // FIXME: We assume that the object must be a wall, so use the hardcoded reference to the wall sprite.
        SpriteRenderer sr = furn_go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSpriteForFurniture(furn);
        sr.sortingLayerName = "Furniture";

		// Register our callback so that our GameObject gets updated whenever
		// the object's type changes.
		furn.RegisterOnChangedCallback( OnFurnitureChanged );
    }

    void OnFurnitureChanged(Furniture furn){
        //Make sure the furniture's graphics are correct.

        if(furnitureGameObjectMap.ContainsKey(furn) == false) {
            Debug.LogError("OnCharacterChanged -- trying to change visuals for furniture not in our map.");
            return;
        }

        GameObject furn_go = furnitureGameObjectMap[furn];
        furn_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);

    }

    public Sprite GetSpriteForFurniture(Furniture furn){
        string spriteName = furn.objectType;

        if(furn.linksToNeighbour == false){
            //If this is a DOOR, lets check for openness and update the sprite.
            //FIXME: All this hardcoding needs ot be generalized later.
            if(furn.objectType == "Door") {
                if(furn.GetParameter("openness") < 0.1f) {
                    //Door is closed
                    spriteName = "Door";
                } else if(furn.GetParameter("openness") < 0.5f) {
                    //Door is a bit open
                    spriteName = "Door_openness_1";
                } else if(furn.GetParameter("openness") < 0.9f) {
                    //Door is a lot open
                    spriteName = "Door_openness_2";
                } else {
                    //Door is fully open
                    spriteName = "Door_openness_3";
                }
            }

            return furnitureSprites[spriteName];
        }

        //Otherwise, the sprite name is more complicated
        spriteName = furn.objectType + "_";

        //Check for neighbours North, East, South, West

        int x = furn.tile.X;
        int y = furn.tile.Y;

        Tile t;
        
        t = world.GetTileAt(x, y + 1);
        if(t != null && t.furniture != null && t.furniture.objectType == furn.objectType){
            spriteName += "N";
        }
        t = world.GetTileAt(x + 1, y);
        if(t != null && t.furniture != null && t.furniture.objectType == furn.objectType){
            spriteName += "E";
        }
        t = world.GetTileAt(x, y - 1);
        if(t != null && t.furniture != null && t.furniture.objectType == furn.objectType){
            spriteName += "S";
        }
        t = world.GetTileAt(x - 1, y);
        if(t != null && t.furniture != null && t.furniture.objectType == furn.objectType){
            spriteName += "W";
        }

        if(furnitureSprites.ContainsKey(spriteName) == false){
            Debug.LogError("GetSpriteForFurniture -- No Sprites with name: " + spriteName);
        }

        return furnitureSprites[spriteName];
    }

    public Sprite GetSpriteForFurniture(string objectType) {
        if (furnitureSprites.ContainsKey(objectType)) {
            return furnitureSprites[objectType];
        }

        if (furnitureSprites.ContainsKey(objectType+"_")) {
            return furnitureSprites[objectType+"_"];
        }

        Debug.LogError("GetSpriteForFurniture -- No Sprites with name: " + objectType);
        return null;

    }

}
