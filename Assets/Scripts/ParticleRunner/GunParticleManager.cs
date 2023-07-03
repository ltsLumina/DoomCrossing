using System;
using UnityEngine;

public class GunParticleManager : MonoBehaviour
{
    [Header("Gun Particles")]
    [SerializeField] ParticleSystem[] gunParticles;

    public void GunParticles()
    {
        foreach (var system in gunParticles)
        {
            system.Play();
        }
    }
}