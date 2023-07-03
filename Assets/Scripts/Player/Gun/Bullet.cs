using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    void OnCollisionEnter(Collision other)
    {
        Destroy(gameObject);
    }
}