using System.Collections.Generic;
using UnityEngine;

public class HealthPanel : MonoBehaviour
{
    [SerializeField] private RectTransform heartContainer;
    [SerializeField] private Heart heartPrefab;

    private readonly List<Heart> hearts = new();

    private void OnEnable()
    {
        PlayerMover.HealthChanged += HandleHealthChanged;
    }

    private void Start()
    {
        var player = FindFirstObjectByType<PlayerMover>();
        HandleHealthChanged(player.CurrentHealth, player.MaxHealth);
    }

    private void OnDisable()
    {
        PlayerMover.HealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        SyncHeartCount(maxHealth);

        for (var i = 0; i < hearts.Count; i++)
        {
            if (i < currentHealth)
            {
                hearts[i].Activate();
                continue;
            }

            hearts[i].Deactivate();
        }
    }

    private void SyncHeartCount(int maxHealth)
    {
        for (var i = hearts.Count; i < maxHealth; i++)
        {
            var heart = Instantiate(heartPrefab, heartContainer); 
            hearts.Add(heart);
        }
    }
}
