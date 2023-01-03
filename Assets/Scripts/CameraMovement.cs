using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float moveSpd;
 
    // Update is called once per frame
    void Update () {
 
        Vector3 dir = Vector3.zero;
        if (Input.GetAxisRaw("Horizontal") > 0.5f || Input.GetAxisRaw("Horizontal") < -0.5f)
        {
            //transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * playerSpeed);
            dir += new Vector3(1, 0, 0) * Input.GetAxisRaw("Horizontal");        
        }
        if (Input.GetAxisRaw("Vertical") > 0.5f || Input.GetAxisRaw("Vertical") < -0.5f)
        {
            //transform.Translate(Vector3.forward * Input.GetAxis("Vertical") * playerSpeed);
            dir += new Vector3(0, 0, 1) * Input.GetAxisRaw("Vertical");
        }
        this.transform.position += dir*moveSpd;
    }
}
