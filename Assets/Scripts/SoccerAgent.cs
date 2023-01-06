using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Actuators;

public class SoccerAgent : Agent
{

    // Note that that the detectable tags are different for the blue and purple teams. The order is
    // * ball
    // * own goal
    // * opposing goal
    // * wall
    // * own teammate
    // * opposing player


    //Boolean if on blue team or not (in which case on purple team)
    [HideInInspector] public bool blue;

    float kickPower;

    // The coefficient for the reward for colliding with a ball. Set using curriculum.
    float ballTouch;

    const float k_Power = 2000f;
    float existential;
    float lateralSpeed;
    float forwardSpeed;


    [HideInInspector]
    public Rigidbody agentRb;
    SoccerSettings soccerSettings;
    BehaviorParameters behaviorParameters;
    public Vector3 initialPos;
    public float rotSign;

    EnvironmentParameters resetParams;

    public override void Initialize()
    {
        Environment env = GetComponentInParent<Environment>();
        if (env != null)
        {
            existential = 1f / env.maxEnvironmentSteps;
        }
        else
        {
            existential = 1f / MaxStep;
        }

        behaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        if (behaviorParameters.TeamId == 0)
        {
            blue = true;
            initialPos = new Vector3(transform.position.x - 5f, 1f, transform.position.z);
            rotSign = 1f;
        }
        else
        {
            blue = false;
            initialPos = new Vector3(transform.position.x + 5f, .5f, transform.position.z);
            rotSign = -1f;
        }
        lateralSpeed = 0.65f;
        forwardSpeed = 1.1f;
        soccerSettings = FindObjectOfType<SoccerSettings>();
        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 500;

        resetParams = Academy.Instance.EnvironmentParameters;
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        kickPower = 0f;

        var moveForward = act[0];
        var moveLateral = act[1];
        var rotate = act[2];

        //go forward
        if (moveForward == 1) {//Can only kick while moving forward (no backward kicks for now)
            dirToGo = transform.forward * forwardSpeed;
            kickPower = 1f;
        } else if (moveForward == 2) { //go backward
            dirToGo = transform.forward * -forwardSpeed;
        }
        
        //Moving sideways will override moving forwards/backwards
        //go right
        if (moveLateral == 1) {
            dirToGo = transform.right * lateralSpeed;
        } else if (moveLateral == 2) { //go left
            dirToGo = transform.right * -lateralSpeed;
        }

        if (rotate == 1) { //rotate left
            rotateDir = transform.up * -1f;
        } else if (rotate == 2) {//rotate right
            rotateDir = transform.up * 1f;
        }

        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        agentRb.AddForce(dirToGo * soccerSettings.agentMoveSpeed,ForceMode.VelocityChange);
    }

    public override void OnEpisodeBegin()
    {
        //ballTouch = resetParams.GetWithDefault("ball_touch", 0);
    }

    public override void OnActionReceived(ActionBuffers actions) {
        AddReward(-existential);
        MoveAgent(actions.DiscreteActions);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        //forward
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        //rotate
        if (Input.GetKey(KeyCode.Q))
        {
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[2] = 2;
        }
        //right
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[1] = 2;
        }
    }

    /// <summary>
    /// Used to provide a "kick" to the ball.
    /// </summary>
    void OnCollisionEnter(Collision c)
    {
        var force = k_Power * kickPower;
        if (c.gameObject.CompareTag("ball"))
        {
            AddReward(.2f * ballTouch);
            var dir = c.contacts[0].point - transform.position;
            dir = dir.normalized;
            c.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);
        }
    }
}
