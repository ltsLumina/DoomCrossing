using Lumina.Essentials;
using Lumina.Essentials.Sequencer;
using UnityEngine;

public class Disintegrate : MonoBehaviour
{
    [Tooltip("The time the object is on the ground before dissipating."),
    SerializeField] float groundTime;

    // Cached References
    Collider col;

    void OnEnable()
    {
        col = GetComponent<Collider>();
        Dissipate();
    }

    void Dissipate()
    {
        Sequence dissipation = new Sequence(this);
        dissipation.Execute(() => col.enabled = false).WaitForSeconds(1f).Execute(() => Destroy(gameObject));
    }
}