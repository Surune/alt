using UnityEngine;

public class WeaponCatalog : MonoBehaviour
{
    public static WeaponCatalog Instance { get; private set; }

    [SerializeField] private WeaponData[] weapons;

    public WeaponData[] Weapons => weapons;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
