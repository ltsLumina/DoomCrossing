using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunThrowAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.AddForce(-transform.right * 6, ForceMode.Impulse);
        rb.AddForce(transform.up * 2, ForceMode.Impulse);
        rb.AddTorque(new Vector3(Random.Range(-20,20), Random.Range(-20, 20), Random.Range(-20, 20)), ForceMode.Impulse);
    }

}