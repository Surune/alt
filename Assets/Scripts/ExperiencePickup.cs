using UnityEngine;

public class ExperiencePickup : MonoBehaviour
{
    private int amount;

    public void Init(int experienceAmount)
    {
        amount = experienceAmount;
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<PlayerMover>();
        if (player == null)
        {
            return;
        }

        player.AddExperience(amount);
        Destroy(gameObject);
    }
}
