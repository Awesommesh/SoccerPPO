using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class SoccerAgent : Agent
{
    private Rigidbody rbd;
    public Transform pos;
    private Vector3 orig;
    public Environment env;
    [SerializeField] bool blue;
    [SerializeField] bool envResetter;

    public override void Initialize()
    {
        orig = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
    }

    public override void OnEpisodeBegin()
    {
        //Debug.Log("New episode...");
        if (envResetter) {
            env.resetEnv(false);
        }
        rbd = this.gameObject.GetComponent<Rigidbody>();
        blue = this.gameObject.CompareTag("BluePlayer");
        pos = this.transform;
    }

    public override void OnActionReceived(ActionBuffers actions) {
        //Read Actions and perform them
        int moveX = actions.DiscreteActions[0];
        int moveZ = actions.DiscreteActions[1];
        if (moveX == 0) {
            moveX = -1;
        }
        if (moveZ == 0) {
            moveZ = -1;
        }
        this.rbd.velocity = new Vector3(moveX*env.moveSpd, 0, moveZ*env.moveSpd);
    }

    public void addReward(float r) {
        AddReward(r);
    }
}
