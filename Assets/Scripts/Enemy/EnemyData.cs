using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string EnemyId;
    public string DisplayName;
    public Enemy Prefab;
    public float MaxHealth = 3;
    public float HealthIncreasePerWave;
    public float Damage = 1;
    public float DamageIncreasePerWave;
    public float MoveSpeed = 3.5f;
    public int StartWave = 1;
    public int BloodDropAmount = 1;
}
