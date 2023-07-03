using UnityEngine;

public class GunAnimationEvents : MonoBehaviour
{
    [Header("Serialized References")]
    [SerializeField] GameObject throwingGunPrefab;

    GameObject thrownGuns;

    Magazine mag;

    void Start()
    {
        mag = FindObjectOfType<Magazine>();
        thrownGuns = new GameObject("Thrown Guns");
    }

    public void ThrowGun()
    {
        GameObject thrownGun = Instantiate(throwingGunPrefab, transform.position, transform.rotation);
        thrownGun.transform.parent = thrownGuns.transform;
    }
}