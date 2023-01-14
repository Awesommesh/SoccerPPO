using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class SoccerAgentAdvanced : Agent
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

    //the coefficient for jumping
    float jumpCoeff;

    const float k_Power = 75f;
    float existential;
    float lateralSpeed;
    float forwardSpeed;
    float jumpPower;
    float distToGround;
    Collider col;


    [HideInInspector]
    public Rigidbody agentRb;
    SoccerSettings soccerSettings;
    BehaviorParameters behaviorParameters;
    public Vector3 initialPos;
    public float rotSign;

    public int id;

    List<EnvironmentAdvanced.PlayerInfo> agents;

    public Ball ball;
    public bool inferenceMode;

    int maxEnvironmentSteps;

    EnvironmentParameters resetParams;
    int yKickDir;

    public override void Initialize()
    {
        EnvironmentAdvanced env = GetComponentInParent<EnvironmentAdvanced>();
        if (env != null)
        {
            maxEnvironmentSteps = env.maxEnvironmentSteps;
            existential = 1f / maxEnvironmentSteps;
            agents = env.AgentsList;
            agents.Sort((p, q) => p.id - q.id);
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
        col = GetComponent<Collider>();
        distToGround = col.bounds.extents.y;
        agentRb.maxAngularVelocity = 500;

        resetParams = Academy.Instance.EnvironmentParameters;
    }

    Vector3 transformAxis(Vector3 pos, int xzOrientation) {
        Vector3 temp = Vector3.zero;
        temp.x = pos.x*xzOrientation;
        temp.z = pos.z*xzOrientation;
        temp.y = pos.y;
        return temp;
    }

    public override void CollectObservations(VectorSensor sensor) {
        int xzOrientation = 1;
        if (!blue) {
            xzOrientation = -1;
        }
        //ball
        sensor.AddObservation(transformAxis(ball.transform.localPosition, xzOrientation));
        //self
        sensor.AddObservation(transformAxis(transform.localPosition, xzOrientation));
        int counter = 0;
        //other two team agents
        for (int i = 0; i < agents.Count; i++) {
            if (agents[i].blue == this.blue && agents[i].id != this.id) {
                sensor.AddObservation(transformAxis(agents[i].Agent.transform.localPosition, xzOrientation));
                counter++;
            }
        }
        //enemy agents
        for (int i = 0; i < agents.Count; i++) {
            if (agents[i].blue != this.blue) {
                sensor.AddObservation(transformAxis(agents[i].Agent.transform.localPosition, xzOrientation));
                counter++;
            }
        }
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var jumpDir = Vector3.zero;
        var rotateDir = Vector3.zero;

        kickPower = 0f;

        var moveForward = act[0];
        var moveLateral = act[1];
        var rotate = act[2];
        var jump = act[3];

        yKickDir = act[4];

        bool grounded = Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f);
        //Debug.Log(Physics.Raycast(transform.position, -Vector3.up, distToGround + 0.1f));
        //go forward
        if (moveForward == 1) {//Can only kick while moving forward (no backward kicks for now)
            dirToGo = transform.forward * forwardSpeed;
            
        } else if (moveForward == 2) { //go backward
            dirToGo = transform.forward * -forwardSpeed;
        }

        if (moveForward <= 1) {
            kickPower = 1f;
        }
        
        //Moving sideways will override moving forwards/backwards
        //go right
        if (moveLateral == 1) {
            dirToGo = transform.right * lateralSpeed;
        } else if (moveLateral == 2) { //go left
            dirToGo = transform.right * -lateralSpeed;
        }
        if (jump == 1 && grounded) {//Add jump force
            if (inferenceMode) {
                jumpCoeff = 1;
            }
            jumpDir = transform.up * soccerSettings.jumpPower * jumpCoeff;
        }

        //Debug.Log(grounded + " " + jump);

        if (rotate == 1) { //rotate left
            rotateDir = transform.up * -1f;
        } else if (rotate == 2) {//rotate right
            rotateDir = transform.up * 1f;
        }

        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        if (!grounded) {
            agentRb.AddForce(dirToGo * soccerSettings.agentMoveSpeedAir,ForceMode.VelocityChange);
        } else {
            agentRb.AddForce(dirToGo * soccerSettings.agentMoveSpeed,ForceMode.VelocityChange);
        }
        
        //Debug.Log(jumpDir);
        agentRb.AddForce(jumpDir, ForceMode.Impulse);
    }

    public override void OnEpisodeBegin()
    {
        ballTouch = resetParams.GetWithDefault("ball_touch", 0);
        existential = resetParams.GetWithDefault("existential", 1) * (1f / maxEnvironmentSteps);
        jumpCoeff = resetParams.GetWithDefault("jump_power", 0);
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
        //jump
        if (Input.GetKey(KeyCode.Space)) {
            discreteActionsOut[3] = 1;
        }

        //Kick Y dir
        discreteActionsOut[4] = 0;
        if (Input.GetKey(KeyCode.UpArrow)) {
            discreteActionsOut[4] = 1;
        } else if (Input.GetKey(KeyCode.DownArrow)) {
            discreteActionsOut[4] = 2;
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
            Vector3 dir = c.contacts[0].point - transform.position;
            dir.y = 0;
            dir = dir.normalized;
            Vector3 dir2 = Vector3.up;
            if (yKickDir == 0) {//kick parallel to the ground
                dir2 = Vector3.zero;
            } else if (yKickDir == 2) {//kick down
                dir2 = Vector3.up*-1;
            }
            c.gameObject.GetComponent<Rigidbody>().AddForce((dir+dir2) * force, ForceMode.Impulse);
        }
    }
}
