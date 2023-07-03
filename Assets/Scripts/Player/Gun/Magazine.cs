#region
using Lumina.Essentials;
using TMPro;
using UnityEngine;
#endregion

public class Magazine : MonoBehaviour
{
    [Header("Magazine Options")]
    [SerializeField] float maxMagazineSize = 18;
    [SerializeField] float currentMagCount;
    [SerializeField] bool canReload;

    // Cached References
    Gun gun;
    TextMeshPro ammoText;

    // Cached Hashes
    readonly static int DoReload = Animator.StringToHash("doReload");

    public float MaxMagazineSize
    {
        get => maxMagazineSize;
        set => maxMagazineSize = value;
    }

    public float CurrentMagCount
    {
        get => currentMagCount;
        set
        {
            currentMagCount = value;

            // Check if magazine is empty, and automatically reload if it is.
            if (currentMagCount <= 0 && CanReload) ReloadMagazine();
        }
    }

    public bool CanReload
    {
        get => canReload;
        set => canReload = value;
    }

    void Start()
    {
        gun      = GetComponent<Gun>();
        ammoText = GetComponentInChildren<TextMeshPro>();

        CanReload = true;

        UpdateAmmoText();

        Debug.Assert(currentMagCount < 0 == false,
                     "CurrentMagCount is less than 0! \n An error has occured somewhere!");
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.R) || !CanReload) return;
        ReloadMagazine();
    }

    public void ReloadMagazine()
    {
        Debug.Log("Reload");
        if (Reloading() || !CanReload) return;

        gun.GunAnim.SetTrigger(DoReload);

        // awful way of doing this but it works :))
        // -william hälsar.
        UpdateAmmoText();
        StartCoroutine(Sequencing.SequenceActions(UpdateAmmoText, 0.75f, () =>
        {
            CurrentMagCount = MaxMagazineSize;
            UpdateAmmoText();
        }));
    }

    public void UpdateAmmoText()
    {
        ammoText.text = currentMagCount.ToString();
    }

    public bool Reloading()
    {
        // return true if the gun animation is still playing.
        return gun.GunAnim.GetCurrentAnimatorStateInfo(0).IsName("Reload");
    }
}