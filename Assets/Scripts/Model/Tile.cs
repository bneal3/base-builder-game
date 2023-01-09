using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

// TileType is the base type of the tile. In some tile-based games, that might be
// the terrain type. For us, we only need to differentiate between empty space
// and floor (a.k.a. the station structure/scaffold). Walls/Doors/etc... will be
// Furniture sitting on top of the floor.
public enum TileType { Empty, Floor };

public enum ENTERABILITY { Yes, Never, Soon };

public class Tile {
    private TileType _type = TileType.Empty;
	public TileType Type {
		get { return _type; }
		set {
			TileType oldType = _type;
			_type = value;
			// Call the callback and let things know we've changed.

			if(cbTileChanged != null && oldType != _type)
				cbTileChanged(this);
		}
	}

	// Inventory is something like a drill or a stack of metal sitting on the floor
	public Inventory inventory { get; protected set; }

    public Room room;

	// Furniture is something like a wall, door, or sofa.
	public Furniture furniture {
        get; protected set;
    }

    public Job pendingFurnitureJob;

	// We need to know the context in which we exist. Probably. Maybe.
	public World world { get; protected set; }

	public int X { get; protected set; }
	public int Y { get; protected set; }

    //FIXME: THis is just hardcoded for now. Just a reminder
    const float baseTileMovmenetCost = 1; 

    public float movementCost {
        get {
            if (Type == TileType.Empty)
                return 0; // 0 is unwalkable

            if (furniture == null)
                return 1;

            return baseTileMovmenetCost * furniture.movementCost;
        }
    }

	// The function we callback any time our data changes
	Action<Tile> cbTileChanged;

	/// <summary>
	/// Initializes a new instance of the <see cref="Tile"/> class.
	/// </summary>
	/// <param name="world">A World instance.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	public Tile( World world, int x, int y ) {
		this.world = world;
		this.X = x;
		this.Y = y;
	}

	/// <summary>
	/// Register a function to be called back when our tile type changes.
	/// </summary>
	public void RegisterTileTypeChangedCallback(Action<Tile> callback) {
		cbTileChanged += callback;
	}
	
	/// <summary>
	/// Unregister a callback.
	/// </summary>
	public void UnregisterTileTypeChangedCallback(Action<Tile> callback) {
		cbTileChanged -= callback;
	}

    public bool PlaceFurniture (Furniture objInstance) {
        if(objInstance == null){
            // We are uninstalling whatever was here before.
            furniture = null;
            return true;
        }

        // objInstance isn't null
        if (furniture != null){
            Debug.LogError("Trying to assign a furniture object to a tile that already has one!");
            return false;
        }

        //At this point, everything is fine!

        furniture = objInstance;
        return true;
    }

    public bool PlaceInventory(Inventory inv) {
        if(inv == null) {
            inventory = null;
            return true;
        }

        if(inventory != null) {
            //There's already inventory here. Maybe we can combine a stack?

            if(inventory.objectType != inv.objectType) {
                Debug.LogError("Trying to assign a inventory object to a tile that already has some of a different type!");
                return false;
            }

            if(inventory.stackSize + inv.stackSize > inv.maxStackSize) {
                Debug.LogError("Trying to assign a inventory object to a tile that would exceed max stack sizre!");
                return false;
            }

            int numToMove = inv.stackSize;
            if(inventory.stackSize + numToMove > inventory.maxStackSize) {
                numToMove = inventory.maxStackSize - inventory.stackSize;
            }

            inventory.stackSize += numToMove;
            inv.stackSize -= numToMove;

            return true;
        }

        //At this point, we know that our current inventory is actually
        //null. Now we can't just do a direct assignment, because
        //the inventory manager needs to know that the old stack is now empty
        //and has to be removed the previouslists.

        inventory = inv.Clone();
        inventory.tile = this;
        inv.stackSize = 0;
        
        return true;
    }

    // Tells us if two tiles are adjacent.
    public bool IsNeighbour(Tile tile, bool diagOkay = false) {

        return
            Mathf.Abs(this.X - tile.X) + Mathf.Abs(this.Y - tile.Y) == 1 ||
            (diagOkay && (Mathf.Abs(this.X - tile.X) == 1 && Mathf.Abs(this.Y - tile.Y) == 1));

        /*if(this.X == tile.X && Mathf.Abs(this.Y - tile.Y) == 1)
            return true;

        if(this.Y == tile.Y && Mathf.Abs(this.X - tile.X) == 1)
            return true;

        if (diagOkay) {
            if (this.X == tile.X + 1 && (this.Y == tile.Y + 1 || this.Y == tile.Y - 1))
                return true;

            if (this.X == tile.X - 1 && (this.Y == tile.Y + 1 || this.Y == tile.Y - 1))
                return true;
        }

        return false;*/
    }

    public Tile[] GetNeighbours(bool diagOkay = false) {

        Tile[] ns;

        if (diagOkay == false) {
            ns = new Tile[4]; //Tile order: N E S W
        } else {
            ns = new Tile[8]; //Tile order: N E S W NE SE SW NW
        }

        Tile n;

        n = world.GetTileAt(X, Y + 1);
        ns[0] = n; //Could be null, but that's okay.
        n = world.GetTileAt(X + 1, Y);
        ns[1] = n; //Could be null, but that's okay.
        n = world.GetTileAt(X, Y - 1);
        ns[2] = n; //Could be null, but that's okay.
        n = world.GetTileAt(X - 1, Y);
        ns[3] = n; //Could be null, but that's okay.

        if(diagOkay == true) {
            n = world.GetTileAt(X + 1, Y + 1);
            ns[4] = n; //Could be null, but that's okay.
            n = world.GetTileAt(X + 1, Y - 1);
            ns[5] = n; //Could be null, but that's okay.
            n = world.GetTileAt(X - 1, Y - 1);
            ns[6] = n; //Could be null, but that's okay.
            n = world.GetTileAt(X - 1, Y + 1);
            ns[7] = n; //Could be null, but that's okay.
        }

        return ns;
    }

    public XmlSchema GetSchema() {
        return null;
    }

    public void WriteXml(XmlWriter writer) {
        writer.WriteAttributeString("X", X.ToString());
        writer.WriteAttributeString("Y", Y.ToString());
        writer.WriteAttributeString("Type", ((int)Type).ToString());
    }

    public void ReadXml(XmlReader reader) {
        Type = (TileType)int.Parse(reader.GetAttribute("Type"));
    }

    public ENTERABILITY IsEnterable() {
        //This returns true if you can enter this tile right this moment.
        if(movementCost == 0)
            return ENTERABILITY.Never;

        //Check our furniture to see if it has a special block on enterability
        if(furniture != null && furniture.IsEnterable != null) {
            return furniture.IsEnterable(furniture);
        }

        return ENTERABILITY.Yes;
    }

    public Tile North() {
        return world.GetTileAt(X, Y + 1);
    }

    public Tile South() {
        return world.GetTileAt(X, Y - 1);
    }

    public Tile East() {
        return world.GetTileAt(X + 1, Y);
    }

    public Tile West() {
        return world.GetTileAt(X - 1, Y);
    }

}


