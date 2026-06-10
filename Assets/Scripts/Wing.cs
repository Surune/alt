using System.Collections.Generic;
using UnityEngine;

public class Wing : MonoBehaviour
{
    [SerializeField] float orbitRadius = 0.5f;
    [SerializeField] float orbitSpeed = 50f;

    static readonly List<Wing> ActiveWings = new();
    static float orbitAngle;
    static int orbitUpdateFrame = -1;

    Player player;
    float nextShotTime;

    public void Initialize(Player owner)
    {
        player = owner;
        ActiveWings.Add(this);
    }

    private void Update()
    {
        if (!GameStateManager.Instance.IsGameplayActive || Time.time < nextShotTime)
        {
            return;
        }

        var abilityManager = AbilityManager.Instance;
        nextShotTime = Time.time + abilityManager.WingCooldown;
        var origin = transform.position;
        player.FireWingProjectile(
            origin,
            player.GetNearestEnemyDirection(origin),
            abilityManager.WingDamage,
            abilityManager.WingSpeed,
            abilityManager.WingHoming,
            abilityManager.WingFreezing
        );
    }

    private void LateUpdate()
    {
        if (orbitUpdateFrame != Time.frameCount)
        {
            orbitAngle += orbitSpeed * Time.deltaTime;
            orbitUpdateFrame = Time.frameCount;
        }

        var index = ActiveWings.IndexOf(this);
        var angle = orbitAngle + (360f * index / ActiveWings.Count);
        var orbitDirection = Quaternion.AngleAxis(angle, Vector3.up) * Vector3.forward;
        transform.position = player.transform.position + (orbitDirection * orbitRadius);
        transform.rotation = Quaternion.LookRotation(orbitDirection, Vector3.up);
    }

    private void OnDestroy()
    {
        ActiveWings.Remove(this);
        if (ActiveWings.Count == 0)
        {
            orbitAngle = 0f;
            orbitUpdateFrame = -1;
        }
    }
}
