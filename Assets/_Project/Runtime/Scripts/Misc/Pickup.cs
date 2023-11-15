using UnityEngine;

public class Pickup : MonoBehaviour
{
    Gun gun;
    Magazine magazine;
    [SerializeField] ParticleSystem pickupParticles;
    [SerializeField] AudioSource pickupSFX;

    void Start()
    {
        gun = FindObjectOfType<Gun>();
        magazine = FindObjectOfType<Magazine>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        gun.ShootDelay *= 0.85f;
        pickupParticles.Play();
        pickupSFX.Play();

        float round = Mathf.Round(magazine.MaxMagazineSize * 1.25f);
        magazine.MaxMagazineSize = round;

        Debug.Log($"ShootDelay = {gun.ShootDelay}MagazineSize = {magazine.MaxMagazineSize}");
        Destroy(gameObject, 0.2f);
    }
}