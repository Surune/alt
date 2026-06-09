using System.Collections.Generic;
using UnityEngine;

public class BloodPickup : MonoBehaviour
{
    public static event System.Action<Player> OnCollected;
    private static readonly List<BloodPickup> ActivePickups = new();

    [SerializeField] private int healAmount = 1;
    [SerializeField] private float mergeDistance = 0.28f;
    [SerializeField] private float scaleGrowth = 0.12f;
    [SerializeField] private float maxScale = 2.4f;
    [SerializeField] private float floatAmplitude = 0.1f;
    [SerializeField] private float floatFrequency = 2f;
    [SerializeField] private float collectSpeed = 10f;

    private float baseY;
    private float floatOffset;
    private Player collectingPlayer;
    private bool isCollecting;

    public static void SpawnOrGrow(BloodPickup prefab, Vector3 position, Quaternion rotation, int amount)
    {
        foreach (var activePickup in ActivePickups)
        {
            var offset = activePickup.transform.position - position;
            offset.y = 0f;
            if (offset.sqrMagnitude > activePickup.mergeDistance * activePickup.mergeDistance)
            {
                continue;
            }

            activePickup.Grow(amount);
            return;
        }

        var pickup = PoolManager.Instance.GetBloodPickup(prefab, position, rotation);
        pickup.transform.localScale = Vector3.one;
        pickup.healAmount = amount;
        pickup.baseY = position.y;
        pickup.floatOffset = Random.Range(0f, Mathf.PI * 2f);
        pickup.collectingPlayer = null;
        pickup.isCollecting = false;
        pickup.gameObject.SetActive(true);
    }

    public static void CollectAll(Player player)
    {
        for (var i = 0; i < ActivePickups.Count; i++)
        {
            ActivePickups[i].collectingPlayer = player;
            ActivePickups[i].isCollecting = true;
        }
    }

    private void OnEnable()
    {
        ActivePickups.Add(this);
    }

    private void OnDisable()
    {
        ActivePickups.Remove(this);
    }

    private void Update()
    {
        if (isCollecting)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                collectingPlayer.transform.position,
                collectSpeed * Time.unscaledDeltaTime);

            if ((transform.position - collectingPlayer.transform.position).sqrMagnitude <= 0.01f)
            {
                Collect(collectingPlayer);
            }

            return;
        }

        var position = transform.position;
        position.y = baseY + ((Mathf.Sin(Time.time * floatFrequency + floatOffset) * 0.5f + 0.5f) * floatAmplitude);
        transform.position = position;
    }

    private void Grow(int amount)
    {
        var nextScale = transform.localScale.x + scaleGrowth;
        if (nextScale > maxScale)
        {
            nextScale = maxScale;
        }

        transform.localScale = Vector3.one * nextScale;
        healAmount += amount;
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (player == null)
        {
            return;
        }

        Collect(player);
    }

    private void Collect(Player player)
    {
        player.Heal(healAmount);
        OnCollected?.Invoke(player);
        PoolManager.Instance.ReleaseBloodPickup(this);
    }
}
