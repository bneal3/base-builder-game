using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Job {

    //This class holds info for a queued up job, which can include things like placing furniture, moving stored inventory, etc.

    public Tile tile { get; protected set; }
    float jobTime;

    //FIXME: Hard-coding a parameter for furniture.
    public string jobObjectType {
        get; protected set;
    }

    Action<Job> cbJobComplete;
    Action<Job> cbJobCancel; 
    public Job(Tile tile, string jobObjectType, Action<Job> cbJobComplete, float jobTime = 0.1f) {
        this.tile = tile;
        this.jobObjectType = jobObjectType;
        this.cbJobComplete += cbJobComplete;
        this.jobTime = jobTime;
    }

    public void RegisterJobCompleteCallback(Action<Job> cb) {
        this.cbJobComplete += cb;
    }

    public void RegisterJobCancelCallback(Action<Job> cb) {
        this.cbJobCancel += cb;
    }

    public void UnregisterJobCompleteCallback(Action<Job> cb) {
        this.cbJobComplete -= cb;
    }

    public void UnregisterJobCancelCallback(Action<Job> cb) {
        this.cbJobCancel -= cb;
    }

    public void DoWork(float workTime) {
        jobTime -= workTime;

        if(jobTime <= 0) {
            if(cbJobComplete != null)
                cbJobComplete(this);
        }
    }

    public void CancelJob() {
        if(cbJobCancel != null)
            cbJobCancel(this);
    }
    
}
