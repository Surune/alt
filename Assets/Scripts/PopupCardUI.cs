using System;
using UnityEngine;

public class PopupCardUI : MonoBehaviour
{
    [SerializeField] private HologramCardUI[] cards;

    private void Awake()
    {
        for (var i = 0; i < cards.Length; i++)
        {
            cards[i].SetClickAction(Close);
        }
    }

    public void Close()
    {
        UIManager.Instance.ClosePopupCard();
    }
}
