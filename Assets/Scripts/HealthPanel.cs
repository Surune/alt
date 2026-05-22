using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthPanel : MonoBehaviour
{
    [SerializeField] private Slider gauge;
    [SerializeField] private TMP_Text healthText;

    private void OnEnable()
    {
        Player.HealthChanged += HandleHealthChanged;
    }

    private void Start()
    {
        var player = FindFirstObjectByType<Player>();
        HandleHealthChanged(player.CurrentHealth, player.MaxHealth);
    }

    private void OnDisable()
    {
        Player.HealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int currentHealth, int maxHealth)
    {
        gauge.value = (float)currentHealth / maxHealth;
        healthText.text = $"{currentHealth}/{maxHealth}";
    }
}
