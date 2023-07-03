using System;
using UnityEngine;

public class Killplane : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player has entered a killplane!");
        }
    }
}