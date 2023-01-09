using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class EnvironmentAdvanced : MonoBehaviour
{
    //Player Class
    [System.Serializable]
    public class PlayerInfo
    {
        public int id;
        public bool blue;
        public SoccerAgentAdvanced Agent;
        [HideInInspector]
        public Vector3 startingPos;
        [HideInInspector]
        public Quaternion startingRot;
        [HideInInspector]
        public Rigidbody Rb;
    }

    /// Max Academy steps before this platform resets
    [Tooltip("Max Environment Steps")] public int maxEnvironmentSteps = 25000;


    public GameObject ball;
    
    [HideInInspector]
    public Rigidbody ballRb;
    Vector3 ballStartingPos;

    //All players
    public List<PlayerInfo> AgentsList = new List<PlayerInfo>();

    private SoccerSettings soccerSettings;

    private SimpleMultiAgentGroup blueAgentGroup;
    private SimpleMultiAgentGroup purpleAgentGroup;

    private int resetTimer;

    void Start() {
        soccerSettings = FindObjectOfType<SoccerSettings>();
        // Initialize TeamManager
        blueAgentGroup = new SimpleMultiAgentGroup();
        purpleAgentGroup = new SimpleMultiAgentGroup();
        ballRb = ball.GetComponent<Rigidbody>();
        ballStartingPos = new Vector3(ball.transform.position.x, ball.transform.position.y, ball.transform.position.z);
        foreach (PlayerInfo item in AgentsList)
        {
            item.startingPos = item.Agent.transform.position;
            item.startingRot = item.Agent.transform.rotation;
            item.Rb = item.Agent.GetComponent<Rigidbody>();
            if (item.Agent.blue)
            {
                blueAgentGroup.RegisterAgent(item.Agent);
            }
            else
            {
                purpleAgentGroup.RegisterAgent(item.Agent);
            }
        }
        resetEnv();
    }

    void FixedUpdate() {
        resetTimer += 1;
        if (resetTimer >= maxEnvironmentSteps && maxEnvironmentSteps > 0)
        {
            blueAgentGroup.GroupEpisodeInterrupted();
            purpleAgentGroup.GroupEpisodeInterrupted();
            resetEnv();
        }
    }

    public void goal(bool blueGoal) {
        if (blueGoal)
        {
            blueAgentGroup.AddGroupReward(1 - (float)resetTimer / maxEnvironmentSteps);
            purpleAgentGroup.AddGroupReward(-1);
        }
        else
        {
            purpleAgentGroup.AddGroupReward(1 - (float)resetTimer / maxEnvironmentSteps);
            blueAgentGroup.AddGroupReward(-1);
        }
        purpleAgentGroup.EndGroupEpisode();
        blueAgentGroup.EndGroupEpisode();
        resetEnv();
    }

    public void resetBall()
    {
        //random respawn
        var randomPosX = Random.Range(-2.5f, 2.5f);
        var randomPosZ = Random.Range(-2.5f, 2.5f);

        ball.transform.position = ballStartingPos + new Vector3(randomPosX, 0f, randomPosZ);
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

    }

    public void resetEnv() {
        resetTimer = 0;

        //Reset Agents
        foreach (var item in AgentsList)
        {
            var randomPosX = Random.Range(-5f, 5f);
            var newStartPos = item.Agent.initialPos + new Vector3(randomPosX, 0f, 0f);
            var rot = item.Agent.rotSign * Random.Range(80.0f, 100.0f);
            var newRot = Quaternion.Euler(0, rot, 0);
            item.Agent.transform.SetPositionAndRotation(newStartPos, newRot);

            item.Rb.velocity = Vector3.zero;
            item.Rb.angularVelocity = Vector3.zero;
        }

        //Reset Ball
        resetBall();
    }
}