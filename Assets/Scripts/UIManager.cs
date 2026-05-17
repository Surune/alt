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
            ExperienceManager.Instance.HandlePopupCardClosed();
            return;
        }

        GameStateManager.Instance.EnterCardPopupState();
    }

    public void ClosePopupCard()
    {
        Destroy(activePopupCard);
        activePopupCard = null;
        ExperienceManager.Instance.HandlePopupCardClosed();
    }

    private void OnDestroy()
    {
        PlayerMover.CurrentWeaponChanged -= HandleCurrentWeaponChanged;

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void HandleCurrentWeaponChanged(WeaponData weapon)
    {
        currentWeaponPanel.Init(weapon);
    }
}
