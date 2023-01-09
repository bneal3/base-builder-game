using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobSpriteController : MonoBehaviour {

    //This bare-bones controller is mostly just going to piggyback on FurnitureSpriteController because we don't yet fully know what our job system is going to look like in the end.

    FurnitureSpriteController fsc;
    Dictionary<Job, GameObject> jobGameObjectMap;

	// Use this for initialization
	void Start () {
        fsc = GameObject.FindObjectOfType<FurnitureSpriteController>();
        jobGameObjectMap = new Dictionary<Job, GameObject>();

        //FIXME: No such thing as a JobQueue yet
        WorldController.Instance.world.jobQueue.RegisterJobCreationCallback(OnJobCreated);
	}

    void OnJobCreated(Job job) {
        //FIXME: We can only do furniture-building jobs.
        
        if (jobGameObjectMap.ContainsKey(job)) {
            Debug.LogError("OnJobCreated for a jobGO that already exists -- most likely a job being RE-QUEUED, as opposed to created.");
            return;
        }

        GameObject job_go = new GameObject();

		// Add our tile/GO pair to the dictionary.
		jobGameObjectMap.Add( job, job_go );

		job_go.name = "JOB_" + job.jobObjectType + "_" + job.tile.X + "_" + job.tile.Y;
		job_go.transform.position = new Vector3( job.tile.X, job.tile.Y, 0);
		job_go.transform.SetParent(this.transform, true);

        // FIXME: We assume that the object must be a wall, so use the hardcoded reference to the wall sprite.
        SpriteRenderer sr = job_go.AddComponent<SpriteRenderer>();
        sr.sprite = fsc.GetSpriteForFurniture(job.jobObjectType);
        sr.color = new Color(0.5f, 1f, 0.5f, 0.25f);
        sr.sortingLayerName = "Jobs";

        //FIXME: This hardcoding is not ideal!
        if(job.jobObjectType == "Door") {
            //By default, the door graphic is meant for walls to the east & west
            //Check to see if we actually have a wall north/south, and if so then rotate this GO by 90 degrees.

            Tile northTile = job.tile.world.GetTileAt(job.tile.X, job.tile.Y + 1);
            Tile southTile = job.tile.world.GetTileAt(job.tile.X, job.tile.Y + 1);

            if(northTile != null && southTile != null && northTile.furniture != null && southTile.furniture != null &&
                northTile.furniture.objectType == "Wall" && southTile.furniture.objectType == "Wall") {
                job_go.transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }


        job.RegisterJobCompleteCallback(OnJobEnded);
        job.RegisterJobCancelCallback(OnJobEnded);
    }
    
    void OnJobEnded(Job job) {
        //This exectures wheteher a job was COMPLETED OR CANCELLED

        //FIXME: We can only do furniture-building jobs.

        GameObject job_go = jobGameObjectMap[job];

        job.UnregisterJobCancelCallback(OnJobEnded);
        job.UnregisterJobCompleteCallback(OnJobEnded);

        Destroy(job_go);
    }
}
