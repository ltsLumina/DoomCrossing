using System;
using UnityEngine;

public class PlayerParticles : MonoBehaviour
{
    [Header("Movement Particles")]
    [SerializeField] ParticleSystem runningParticles;
    [SerializeField] ParticleSystem jumpParticles;
    [SerializeField] ParticleSystem wallRunParticles;
    [SerializeField] ParticleSystem pickupParticles;

    public void ParticlePlayer(Type particleToRun)
    {
        switch (particleToRun)
        {
            case Type.Jump:
                jumpParticles.Play();
                break;

            case Type.Running:
                runningParticles.Play();
                break;

            case Type.WallRunning:
                wallRunParticles.Play();
                break;

            case Type.Pickup:
                pickupParticles.Play();
                break;
        }
    }

    public void StopParticle(Type particleToStop)
    {
        switch (particleToStop)
        {
            case Type.Jump:
                jumpParticles.Stop();
                break;

            case Type.Running:
                runningParticles.Stop();
                break;

            case Type.WallRunning:
                wallRunParticles.Stop();
                break;

            case Type.Pickup:
                pickupParticles.Stop();
                break;
        }
    }

    public enum Type
    {
        Jump,
        Running,
        WallRunning,
        Pickup
    }
}