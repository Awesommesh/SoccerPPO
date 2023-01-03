using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;
    public SoccerAgent [] blueTeam;
    public SoccerAgent [] redTeam;
    public Transform ball;
    public Rigidbody ballRbdy;
    public Transform blueGoal;
    public Transform redGoal;
    public float moveSpd;
    public float punishScale;
    public float randomRepos;
    [SerializeField] float goalReward;
    private Vector3 origBallPos;
    public Vector3 [] origBlueTeam;
    public Vector3 [] origRedTeam;
    [SerializeField] int rewardFreq;
    [SerializeField] int kickReward;
    [SerializeField] int debugFreq;
    [SerializeField] bool enableKickRew;
    int counter;
    int debugCounter;

    public static GameManager instance { get { return _instance; } }

    void Awake()
    {
        if (_instance != null && _instance != this) {
            Destroy(this.gameObject);
        } else {
            _instance = this;
            counter = 0;
            origBlueTeam = new Vector3[blueTeam.Length];
            origRedTeam = new Vector3[redTeam.Length];
            origBallPos = ball.position;
            for (int i = 0; i < blueTeam.Length; i++) {
                origBlueTeam[i] = blueTeam[i].transform.position;
                origRedTeam[i] = redTeam[i].transform.position;
            }
        }
    }

    void FixedUpdate() {
        /*counter++;
        if (counter >= rewardFreq) {
            Debug.Log("updating rewards for agents...");
            updateRewards();
            counter = 0;
        }*/
    }

    public void resetEnv(bool soft) {
        ball.position = origBallPos + new Vector3(Random.Range(-1, 1)*randomRepos, 0, Random.Range(-1, 1)*randomRepos);
        ballRbdy.velocity = Vector3.zero;
        ballRbdy.angularVelocity = Vector3.zero;
        for (int i = 0; i < blueTeam.Length; i++) {
            blueTeam[i].pos.position = origBlueTeam[i];
            redTeam[i].pos.position = origRedTeam[i];
        }
        if (!soft) {
            counter = 0;
            debugCounter = 0;
        }
    }

    void updateRewards() {
        //Approach ball tactics
        float blueDist = 0;
        float redDist = 0;
        /*for (int i = 0; i < blueTeam.Length; i++) {
            blueDist += Vector3.Distance(ball.position, blueTeam[i].pos.position);
        }
        for (int i = 0; i < redTeam.Length; i++) {
            redDist += Vector3.Distance(ball.position, redTeam[i].pos.position);
        }

        giveReward(true, -blueDist/(punishScale*100));
        giveReward(false, -redDist/(punishScale*100));

        debugCounter++;
        if (debugCounter >= debugFreq) {
            Debug.Log("Blue Reward: " + -blueDist/punishScale);
            Debug.Log("Red Reward: " + -redDist/punishScale);
            debugCounter = 0;
        }*/
        //Goal based rewards
        blueDist = Vector3.Distance(redGoal.position, ball.position);
        redDist = Vector3.Distance(blueGoal.position, ball.position);
        Debug.Log(redGoal.position + " goals " + blueGoal.position);
        Debug.Log(blueDist + " " + redDist);
        Debug.Log("Reward for Blue: " + ((redDist-blueDist)/punishScale));
        Debug.Log("Reward for Red: " + ((blueDist-redDist)/punishScale));
        giveReward(true, ((redDist-blueDist)/punishScale));
        giveReward(false, ((blueDist-redDist)/punishScale));
        
    }

    public void goal(bool blueGoal) {
        giveReward(blueGoal, goalReward);
        giveReward(!blueGoal, -goalReward);
    }

    public void kick(bool blueKick) {
        if (enableKickRew) {
            giveReward(blueKick, kickReward);
            giveReward(!blueKick, -kickReward);
        }
       
    }

    void giveReward(bool blue, float val) {
        if (blue) {
            for (int i = 0; i < blueTeam.Length; i++) {
                blueTeam[i].addReward(val);
            }
        } else {
            for (int i = 0; i < redTeam.Length; i++) {
                redTeam[i].addReward(val);
            }
        }
    }
}