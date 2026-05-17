using System.Collections.Generic;
using UnityEngine;

public class PopupCardUI : MonoBehaviour
{
    [SerializeField] private Transform cardRoot;
    [SerializeField] private Card cardPrefab;

    public bool Initialize()
    {
        var player = FindFirstObjectByType<PlayerMover>();
        var availableWeapons = new List<WeaponData>(player.GetUnownedWeapons());
        if (availableWeapons.Count == 0)
        {
            return false;
        }

        for (var i = availableWeapons.Count - 1; i > 0; i--)
        {
            var swapIndex = Random.Range(0, i + 1);
            var temp = availableWeapons[i];
            availableWeapons[i] = availableWeapons[swapIndex];
            availableWeapons[swapIndex] = temp;
        }

        var cardCount = Mathf.Min(3, availableWeapons.Count);
        for (var i = 0; i < cardCount; i++)
        {
            var weapon = availableWeapons[i];
            var card = Instantiate(cardPrefab, cardRoot);
            card.Init(weapon);
            card.SetClickAction(() => SelectWeapon(player, weapon));
        }

        return true;
    }

    private void SelectWeapon(PlayerMover player, WeaponData weapon)
    {
        player.AcquireWeapon(weapon);
        Close();
    }

    private void Close()
    {
        UIManager.Instance.ClosePopupCard();
    }
}
