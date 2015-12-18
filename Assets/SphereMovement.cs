using UnityEngine;
using System.Collections;

public class SphereMovement : MonoBehaviour {

    public GameObject camera;

	// Use this for initialization
	void Start () {
        Rigidbody body = gameObject.GetComponent<Rigidbody>();
        body.AddTorque(new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)), ForceMode.Acceleration);
    }
	
	// Update is called once per frame
	void Update () {
        Rigidbody body = gameObject.GetComponent<Rigidbody>();
        body.maxAngularVelocity = 180;
        Vector3 forceDir = Vector3.zero;
        float power = 2;

        if (Input.GetKey(KeyCode.W))
            forceDir += camera.transform.forward;

        if (Input.GetKey(KeyCode.S))
            forceDir -= camera.transform.forward;

        if (Input.GetKey(KeyCode.A))
            forceDir -= camera.transform.right;

        if (Input.GetKey(KeyCode.D))
            forceDir += camera.transform.right;

        if (Physics.Raycast(transform.position, -Vector3.up, 10))
            power = 16;

        body.AddTorque(Vector3.Cross(Vector3.up, forceDir).normalized * power, ForceMode.Acceleration);

        if (Input.GetKey(KeyCode.Space))
        {
            //body.angularVelocity = Vector3.zero;
            body.AddForce(Vector3.down * 3000, ForceMode.Acceleration);
        }

    }
}
