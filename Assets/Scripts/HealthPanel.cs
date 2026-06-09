using TMPro;
using UnityEngine;

public class HealthPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;

    private void OnEnable()
    {
        Player.HealthChanged += HandleHealthChanged;
    }

    private void Start()
    {
        var player = FindFirstObjectByType<Player>();
        HandleHealthChanged(player.CurrentHealth);
    }

    private void OnDisable()
    {
        Player.HealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int currentHealth)
    {
        healthText.text = currentHealth.ToString();
    }
}
