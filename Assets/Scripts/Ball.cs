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

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.CompareTag("BlueGoal") == true) {
            //Debug.Log("Blue GOAL!");
            env.goal(true);
            env.resetEnv(true);
        } else if (other.gameObject.CompareTag("RedGoal") == true) {
            //Debug.Log("Red GOAL!");
            env.goal(false);
            env.resetEnv(true);
        }
    }

    void Update() {
        curVel = rb.velocity;
    }

    void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("BluePlayer") == true) {
            //Debug.Log("Blue Kick!");
            env.kick(true);
        } else if (other.gameObject.CompareTag("RedPlayer") == true) {
            //Debug.Log("Red Kick!");
            env.kick(false);
        } else {
            float speed = curVel.magnitude;
            Vector3 dir = Vector3.Reflect(curVel.normalized, other.contacts[0].normal);
            rb.velocity = dir * Mathf.Max(speed, 0f);
        }

    }
}
