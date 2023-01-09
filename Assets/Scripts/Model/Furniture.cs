using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

//Furniture are things like walls, doors and furniture

public class Furniture : IXmlSerializable {

    protected Dictionary<string, float> furnParameters;
    protected Action<Furniture, float> updateActions;

    public Func<Furniture, ENTERABILITY> IsEnterable;

    public void Update(float deltaTime) {

        if(updateActions != null){
            updateActions(this, deltaTime);
        }
    }

    //This represent s the BASE tile of the object -- but in practice, large objects may occupy multiple tiles.
    public Tile tile {
        get; protected set;
    }
    
    //This "objectType" will be queried by the visual system to know what sprite to render for this object
    public string objectType {
        get; protected set;
    }
    
    //This is a multiplier. A value of "2" here, means you move twice as slowly.
    public float movementCost { get; protected set; }

    public bool roomEnclosure { get; protected set; }

    int width = 1;
    int height = 1;

    public bool linksToNeighbour {
        get; protected set;
    }

    public Action<Furniture> cbOnChanged;

    Func<Tile, bool> funcPositionValidation;

    //Empty constructor is used for serialization
    public Furniture(){
        furnParameters = new Dictionary<string, float>();

    }

    //Copy Constructor
    protected Furniture(Furniture other) {
        this.objectType = other.objectType;
        this.movementCost = other.movementCost;
        this.roomEnclosure = other.roomEnclosure;
        this.width = other.width;
        this.height = other.height;
        this.linksToNeighbour = other.linksToNeighbour;

        furnParameters = new Dictionary<string, float>(other.furnParameters);

        if(other.updateActions != null)
            this.updateActions = (Action<Furniture, float>)other.updateActions.Clone();

        this.IsEnterable = other.IsEnterable;
    }

    virtual public Furniture Clone() {
        return new Furniture(this);
    }

    //Create furniture from parameters -- this will probably ONLY ever be used for prototypes
    public Furniture(string objectType, float movementCost = 1f, int width = 1, int height = 1, bool linkstoNeighbour = false, bool roomEnclosure = false){
        this.objectType = objectType;
        this.movementCost = movementCost;
        this.roomEnclosure = roomEnclosure;
        this.width = width;
        this.height = height;
        this.linksToNeighbour = linkstoNeighbour;

        this.funcPositionValidation = this.DEFAULT__IsValidPosition;

        furnParameters = new Dictionary<string, float>();
    }

    static public Furniture PlaceInstance(Furniture proto, Tile tile){
        if(proto.funcPositionValidation(tile) == false){
            Debug.LogError("PlaceInstance -- Position Validity Function returned FALSE");
            return null;
        }

        //We know our placement destination is valid

        Furniture obj = proto.Clone();  //new Furniture(proto);
        obj.tile = tile;

        //FIXME: This assumes we are 1x1!
        if(tile.PlaceFurniture(obj) == false){
            //For some reason, we weren't able to place our object in this tile.

            // Do NOT return our newly instantiated object.
            // (It will be garbage collected.)
            return null;
        }

        if (obj.linksToNeighbour) {
            //This type of furniture links itself to its neighbors.
            //so we should inform our neighbours that they have a new buddy.
            //Just trigger their OnChangedCallback.

            Tile t;
            int x = tile.X;
            int y = tile.Y;

            t = tile.world.GetTileAt(x, y + 1);
            if(t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType){
                //We have a Northern neighbour witht he same object type as us, so tell it that it has changed by firing its callback.
                t.furniture.cbOnChanged(t.furniture);
            }
            t = tile.world.GetTileAt(x + 1, y);
            if(t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType){
                t.furniture.cbOnChanged(t.furniture);
            }
            t = tile.world.GetTileAt(x, y - 1);
            if(t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType){
                t.furniture.cbOnChanged(t.furniture);
            }
            t = tile.world.GetTileAt(x - 1, y);
            if(t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType){
                t.furniture.cbOnChanged(t.furniture);
            }
        }

        return obj;
    }

    public void RegisterOnChangedCallback(Action<Furniture> callbackFunc){
        cbOnChanged += callbackFunc;
    }

    public void UnregisterOnChangedCallback(Action<Furniture> callbackFunc){
        cbOnChanged -= callbackFunc;
    }

    public bool IsValidPosition(Tile t) {
        return funcPositionValidation(t);
    }

    //FIXME: Should not be public
    protected bool DEFAULT__IsValidPosition(Tile t){
        //Make sure tile is FLOOR
        if(t.Type != TileType.Floor){
            return false;
        }

        //Make sure tile doesn't already have furniture
        if(t.furniture != null){
            return false;
        }

        return true; 
    }

    public XmlSchema GetSchema() {
        return null;
    }

    public void WriteXml(XmlWriter writer) {
        writer.WriteAttributeString("X", tile.X.ToString());
        writer.WriteAttributeString("Y", tile.Y.ToString());
        writer.WriteAttributeString("objectType", objectType);
        //writer.WriteAttributeString("movementCost", movementCost.ToString());

        foreach (string k in furnParameters.Keys) {
            writer.WriteStartElement("Param");
            writer.WriteAttributeString("name", k);
            writer.WriteAttributeString("value", furnParameters[k].ToString());
            writer.WriteEndElement();
        }
        

    }

    public void ReadXml(XmlReader reader) {
        //X, Y, and objectType have already been set, and we should already be assigned to a tile. 
        //So just read extra data.
        //movementCost = int.Parse(reader.GetAttribute("movementCost"));

        if(reader.ReadToDescendant("Param")){
            do {
                string k = reader.GetAttribute("name");
                float v = float.Parse(reader.GetAttribute("value"));
                furnParameters[k] = v;
            } while (reader.ReadToNextSibling("Param"));
        } 
    }

    public float GetParameter(string key, float default_value = 0) {
        if(furnParameters.ContainsKey(key) == false) {
            return default_value;
        }
        return furnParameters[key];
    }

    public void SetParameter(string key, float value) {
        furnParameters[key] = value;
    }

    public void ChangeParameter(string key, float value) {
        if(furnParameters.ContainsKey(key) == false) {
            furnParameters[key] = value;
            return;
        }

        furnParameters[key] += value;
    }

    public void RegisterUpdateAction(Action<Furniture, float> a) {
        updateActions += a;
    }

    public void UnregisterUpdateAction(Action<Furniture, float> a) {
        updateActions -= a;
    }

}
