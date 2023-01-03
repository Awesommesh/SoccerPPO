using UnityEngine;
using System.Collections;
 
public class Movement : MonoBehaviour {
 
    public float playerSpeed;
    public float jumpHeight = 3f;
 
    private Rigidbody rigidBody;
 
    // Use this for initialization
    void Start () {
        rigidBody = GetComponent<Rigidbody>();
 
    }
 
    // Update is called once per frame
    void Update () {
 
        Vector3 dir = Vector3.zero;
        if (Input.GetAxisRaw("Horizontal") > 0.5f || Input.GetAxisRaw("Horizontal") < -0.5f)
        {
            //transform.Translate(Vector3.right * Input.GetAxis("Horizontal") * playerSpeed);
            dir += new Vector3(1, 0, 0) * Input.GetAxisRaw("Horizontal");        }
        if (Input.GetAxisRaw("Vertical") > 0.5f || Input.GetAxisRaw("Vertical") < -0.5f)
        {
            //transform.Translate(Vector3.forward * Input.GetAxis("Vertical") * playerSpeed);
            dir += new Vector3(0, 0, 1) * Input.GetAxisRaw("Vertical");
        }
        rigidBody.velocity = dir*playerSpeed;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rigidBody.AddForce(new Vector3(0, 1, 0) * jumpHeight, ForceMode.Impulse);
        } 
    }
}