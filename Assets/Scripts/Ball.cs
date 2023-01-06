using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{

    Vector3 curVel;
    Rigidbody rb;
    public Environment env;

    void Awake() {
        rb = gameObject.GetComponent<Rigidbody>();
    }


    void Update() {
        curVel = rb.velocity;
    }

    void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("blueGoal") == true) {
            Debug.Log("Purple Team Scored GOAL!!!");
            env.goal(false);
        } else if (other.gameObject.CompareTag("purpleGoal") == true) {
            env.goal(true);
            Debug.Log("Blue Team Scored GOAL!!!");
        }

    }
}
