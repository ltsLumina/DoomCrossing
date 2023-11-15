using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] float slowdownFactor = 0.05f;

    Magazine mag;

    void Start()
    {
        mag = FindObjectOfType<Magazine>();
    }

    public float SlowdownFactor
    {
        get => slowdownFactor;
        set => slowdownFactor = value;
    }

    public void DoSlowMotion()
    {
        Time.timeScale = SlowdownFactor;
        Time.fixedDeltaTime = Time.timeScale * .02f;

        if (mag.Reloading()) ResetTimeScale();
    }

    public static void ResetTimeScale()
    {
        Time.timeScale      = 1f;
        Time.fixedDeltaTime = Time.timeScale * .02f;
    }
}