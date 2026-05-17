using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPanel : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Sprite reloadingSprite;
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private TMP_Text magazine;

    private Sprite weaponIcon;
    
    public void Init(WeaponData data)
    {
        weaponIcon = data.Icon;
        icon.sprite = weaponIcon;
        displayName.text = data.DisplayName;
        SetMagazine(data.MagazineSize, data.MagazineSize);
    }

    public void SetMagazine(int currentAmmo, int totalAmmo)
    {
        magazine.text = $"{currentAmmo}/{totalAmmo}";
    }

    public void SetReloading(bool isReloading)
    {
        icon.sprite = isReloading ? reloadingSprite : weaponIcon;
    }
}
