using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject popupCardPrefab;
    [SerializeField] private WeaponPanel currentWeaponPanel;

    private GameObject activePopupCard;

    public bool IsPopupCardOpen => activePopupCard != null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        Destroy(gameObject);
    }

    private void OnEnable()
    {
        PlayerMover.CurrentWeaponChanged += HandleCurrentWeaponChanged;
        PlayerMover.CurrentAmmoChanged += HandleCurrentAmmoChanged;
        PlayerMover.ReloadStateChanged += HandleReloadStateChanged;
    }

    public void ShowPopupCard()
    {
        activePopupCard = Instantiate(popupCardPrefab);
        activePopupCard.transform.localScale = Vector3.one;

        var popupCardUi = activePopupCard.GetComponent<PopupCardUI>();
        if (!popupCardUi.Initialize())
        {
            Destroy(activePopupCard);
            activePopupCard = null;
            return;
        }

        GameStateManager.Instance.EnterCardPopupState();
    }

    public void ClosePopupCard()
    {
        Destroy(activePopupCard);
        activePopupCard = null;
    }

    private void OnDestroy()
    {
        PlayerMover.CurrentWeaponChanged -= HandleCurrentWeaponChanged;
        PlayerMover.CurrentAmmoChanged -= HandleCurrentAmmoChanged;
        PlayerMover.ReloadStateChanged -= HandleReloadStateChanged;

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void HandleCurrentWeaponChanged(WeaponData weapon)
    {
        currentWeaponPanel.Init(weapon);
    }

    private void HandleCurrentAmmoChanged(int currentAmmo, int totalAmmo)
    {
        currentWeaponPanel.SetMagazine(currentAmmo, totalAmmo);
    }

    private void HandleReloadStateChanged(bool isReloading)
    {
        currentWeaponPanel.SetReloading(isReloading);
    }
}
