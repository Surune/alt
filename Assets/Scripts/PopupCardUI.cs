using UnityEngine;

public class PopupCardUI : MonoBehaviour
{
    [SerializeField] private Transform cardRoot;
    [SerializeField] private Card cardPrefab;
    
    private void Awake()
    {
        for (var i = 0; i < 3; i++)
        {
            var card = Instantiate(cardPrefab, cardRoot);
            card.SetClickAction(Close);
        }
    }

    private void Close()
    {
        UIManager.Instance.ClosePopupCard();
    }
}
