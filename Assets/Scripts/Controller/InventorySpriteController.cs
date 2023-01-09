using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySpriteController : MonoBehaviour {

    public GameObject inventoryUIPrefab;

    Dictionary<Inventory, GameObject> inventoryGameObjectMap;

    Dictionary<string, Sprite> inventorySprites;

    World world {
        get { return WorldController.Instance.world;  }
    }

	// Use this for initialization
	void Start () {
		
        LoadSprites();

		// Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        inventoryGameObjectMap = new Dictionary<Inventory, GameObject>();

        world.RegisterInventoryCreated(OnInventoryCreated);

        //Check for pre-existing inventory, which won't do the callback.
        foreach (string objectType in world.inventoryManager.inventories.Keys) {
            foreach(Inventory inv in world.inventoryManager.inventories[objectType]){
                OnInventoryCreated(inv);
            }
        }
	}
	
     void LoadSprites(){
        inventorySprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Inventory/");

        foreach(Sprite s in sprites){
            Debug.Log(s);
            inventorySprites[s.name] = s;
        }
    }
    
    public void OnInventoryCreated(Inventory inv){
        Debug.Log("OnInventoryCreated");
        //Create a visual GameObject linked to this data.

        //FIXME: Does not consider multi-tile objects nor rotated objects

        // This creates a new GameObject and adds it to our scene.
		GameObject inv_go = new GameObject();

		// Add our tile/GO pair to the dictionary.
		inventoryGameObjectMap.Add( inv, inv_go );

        inv_go.name = inv.objectType;
		inv_go.transform.position = new Vector3( inv.tile.X, inv.tile.Y, 0);
		inv_go.transform.SetParent(this.transform, true);

        // FIXME: We assume that the object must be a wall, so use the hardcoded reference to the wall sprite.
        SpriteRenderer sr = inv_go.AddComponent<SpriteRenderer>();
        sr.sprite = inventorySprites[inv.objectType];
        sr.sortingLayerName = "Inventory";

        if(inv.maxStackSize > 1) {
            //This is a stackable object, so let's add a InventoryUI component
            //(Which is tsext taht shows the current stackSize.)

            GameObject ui_go = Instantiate(inventoryUIPrefab);
            ui_go.transform.SetParent(inv_go.transform);
            ui_go.transform.localPosition = new Vector3(); //If we change the spriote anchor, this may need to be changed
            ui_go.GetComponentInChildren<Text>().text = inv.stackSize.ToString();
            
        }

		// Register our callback so that our GameObject gets updated whenever
		// the object's type changes.
		//FIXMEE: On changed callback
        //inv.RegisterOnChangedCallback( OnCharacterChanged );
    }

    void OnInventoryChanged(Inventory inv){
        //FIXME: Still needs to work! And get called!

        //Make sure the furniture's graphics are correct.

        if(inventoryGameObjectMap.ContainsKey(inv) == false) {
            Debug.LogError("OnCharacterChanged -- trying to change visuals for character not in our map.");
            return;
        }

        GameObject char_go = inventoryGameObjectMap[inv];
        //inv_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(inv);

        char_go.transform.position = new Vector3(inv.tile.X, inv.tile.Y, 0);
    }

}
