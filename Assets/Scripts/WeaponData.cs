using UnityEngine;

public enum WeaponCategory
{
    Sidearm,
    Automatic,
    Shotgun,
    Rifle,
    Marksman
}

[CreateAssetMenu(fileName = "WeaponData", menuName = "Game/Weapon Data")]
public class WeaponData : ScriptableObject
{ 
    public string WeaponId;
    public string DisplayName;
    public string Description;
    public WeaponCategory Category;
    public Sprite Icon;
    public PlayerProjectile ProjectilePrefab;
    public int Damage;
    public int ProjectilesPerShot;
    public int BurstCount;
    public float SpreadAngle;
    public float FireInterval;
    public float BurstInterval;
    public float ProjectileSpeed;
    public float ProjectileRange;
    public int MagazineSize;
    public float ReloadDuration;
}
