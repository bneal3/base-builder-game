using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager {

    //This is a list of all "live" inventories.
    //Later on this will likely be organized by rooms instead of 
    //a single master list. (Or in addition to.)
    public Dictionary<string, List<Inventory> > inventories;

    public InventoryManager() {
        inventories = new Dictionary<string, List<Inventory>>();
    }

    public bool PlaceInventory(Tile tile, Inventory inv) {
        //Debug.Log("PlaceInventory");

        bool tileWasEmpty = tile.inventory == null;

        if (tile.PlaceInventory(inv) == false){
            //The tile did not accept the inventory for whatever reason, therefore stop.
            return false;
        }

        //At this point, "inv" might be an empty stack if it was merged to another stack.
        if(inv.stackSize == 0) {
            if(inventories.ContainsKey(tile.inventory.objectType)){
                inventories[inv.objectType].Remove(inv);
            }
        }

        //We may also created a new stack on the tile, if the tile was previously empty.
        if (tileWasEmpty) {
            if(inventories.ContainsKey(tile.inventory.objectType) == false) {
                inventories[tile.inventory.objectType] = new List<Inventory>();
            }

            inventories[tile.inventory.objectType].Add(tile.inventory);
        }

        return true;
    }
}
