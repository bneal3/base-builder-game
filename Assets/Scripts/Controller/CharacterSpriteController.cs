using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpriteController : MonoBehaviour {

    Dictionary<Character, GameObject> characterGameObjectMap;

    Dictionary<string, Sprite> characterSprites;

    World world {
        get { return WorldController.Instance.world;  }
    }

	// Use this for initialization
	void Start () {
		
        LoadSprites();

		// Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        characterGameObjectMap = new Dictionary<Character, GameObject>();

        world.RegisterCharacterCreated(OnCharacterCreated);

        //Check for pre-existing characters, which won't do the callback.
        foreach (Character c in world.characters) {
            OnCharacterCreated(c);
        }
	}
	
     void LoadSprites(){
        characterSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Images/Characters/");

        foreach(Sprite s in sprites){
            Debug.Log(s);
            characterSprites[s.name] = s;
        }
    }
    
    public void OnCharacterCreated(Character c){
        Debug.Log("OnCharacterCreated");
        //Create a visual GameObject linked to this data.

        //FIXME: Does not consider multi-tile objects nor rotated objects

        // This creates a new GameObject and adds it to our scene.
		GameObject char_go = new GameObject();

		// Add our tile/GO pair to the dictionary.
		characterGameObjectMap.Add( c, char_go );

        char_go.name = "Character";
		char_go.transform.position = new Vector3( c.X, c.Y, 0);
		char_go.transform.SetParent(this.transform, true);

        // FIXME: We assume that the object must be a wall, so use the hardcoded reference to the wall sprite.
        SpriteRenderer sr = char_go.AddComponent<SpriteRenderer>();
        sr.sprite = characterSprites["p1_front"];
        sr.sortingLayerName = "Characters";

		// Register our callback so that our GameObject gets updated whenever
		// the object's type changes.
		c.RegisterOnChangedCallback( OnCharacterChanged );
    }

    void OnCharacterChanged(Character c){
        //Make sure the furniture's graphics are correct.

        if(characterGameObjectMap.ContainsKey(c) == false) {
            Debug.LogError("OnCharacterChanged -- trying to change visuals for character not in our map.");
            return;
        }

        GameObject char_go = characterGameObjectMap[c];
        //char_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(c);

        char_go.transform.position = new Vector3(c.X, c.Y, 0);
    }

}
