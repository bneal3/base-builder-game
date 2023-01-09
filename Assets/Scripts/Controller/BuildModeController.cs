using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildModeController : MonoBehaviour {
    
	bool buildModeIsObjects = false;
	TileType buildModeTile = TileType.Floor;
    string buildModeObjectType;

	// Use this for initialization
	void Start () {
		
	}

	public void SetMode_BuildFloor( ) {
		buildModeIsObjects = false;
		buildModeTile = TileType.Floor;
	}
	
	public void SetMode_Bulldoze( ) {
		buildModeIsObjects = false;
		buildModeTile = TileType.Empty;
	}

	public void SetMode_BuildFurniture( string objectType ) {
		// Wall is not a Tile!  Wall is an "Furniture" that exists on TOP of a tile.
		buildModeIsObjects = true;
        buildModeObjectType = objectType;
	}

    public void DoPathfindingTest() {
        WorldController.Instance.world.SetupPathfindingExample();

        Path_TileGraph tileGraph = new Path_TileGraph(WorldController.Instance.world);
    }

    public void DoBuild(Tile t) {
        if(buildModeIsObjects == true) {
            // Create the Furniture and assign it to the tile

            // FIXME: This instantly build the furniture
            //WorldController.Instance.World.PlaceFurniture(buildModeObjectType, t);

            //Can we build the furniture in the selected tile?
            //Run the ValidPlacement function!

            string furnitureType = buildModeObjectType;

            if(WorldController.Instance.world.IsFurniturePlacementValid(furnitureType, t)
                && t.pendingFurnitureJob == null){
                // This tile isn't valid for this furniture
                //Create a job for it to be build
                                
                Job j = new Job(t, furnitureType, (theJob) => {
                    WorldController.Instance.world.PlaceFurniture(furnitureType, theJob.tile);
                    t.pendingFurnitureJob = null;
                });

                //FIXME: No manual flag setting!
                t.pendingFurnitureJob = j;
                j.RegisterJobCancelCallback((theJob) => { theJob.tile.pendingFurnitureJob = null; });

                //Add the job to the queue
                WorldController.Instance.world.jobQueue.Enqueue(j);
            }

		} else {
			// We are in tile-changing mode.
			t.Type = buildModeTile;
		}
    }
}
