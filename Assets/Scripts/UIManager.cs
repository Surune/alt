using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject popupCardPrefab;

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
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
