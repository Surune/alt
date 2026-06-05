public enum AbilityManagerFlag
{
    Assassination,
    Penetrate,
    LuckySeven,
    BeingStronger,
    Third,
    Nuker,
    Culling,
    Noxious,
    Psychosense,
    Psychosink,
    Freezing,
    Locked,
    Awaken,
    Burst,
    Echo,
    Immolation,
    Explode,
    Magnet,
    Burning,
    KineticBarrage,
    Udo,
    Fifth,
    Fracture,
    Firm
}

public enum AbilityValueOperation
{
    Add,
    Multiply,
    Set,
    Subtract
}

public enum AbilityEffectKind
{
    SetAbilityCooldown,
    ChangePlayerSkillCooldown,
    SetAbilityManagerFlag,
    PlayerManagerFloat,
    PlayerManagerBool,
    PlayerManagerRefresh,
    PlayerManagerWing,
    SpawnerFloat,
    SpawnerBool,
    HPBarrier,
    HPBool,
    ChangeHP,
    ChangeHPValues,
    RefreshHealthBar,
    Timer,
    ExpBool,
    ExpMax,
    CoinCoefficient,
    EnemyItemProb,
    EnemyDamageCoefficient,
    FireballFatalDamage,
    WingBulletDamage,
    WingBulletSpeed,
    WingBulletUdo,
    WingFreezing,
    ChangeAllWingCooldown,
    CriticalProbFromKills,
    CriticalCoefficientFromKills,
    DamageCoefficientFromKills,
    DamageFromKills,
    CelestialShot,
    Psychosink,
    LibraSwap,
    Berkano,
    ChooseRandomAbilities
}

public enum PlayerManagerFloatStat
{
    Damage,
    DamageCoefficient,
    CriticalProb,
    CriticalCoefficient,
    FatalProb,
    FixDamage,
    ShotSpeedCoefficient
}

public enum PlayerManagerBoolStat
{
    Statikk,
    Reinforce,
    Jera,
    Dagaz,
    Aquaris,
    Repair
}

public enum SpawnerFloatStat
{
    DamageCoefficient,
    MeteorCoefficient,
    SpeedCoefficient,
    AddHP
}

public enum SpawnerBoolStat
{
    Disabled,
    MakeMeteor,
    SpawnSmall,
    SpawnRandom
}

public enum HPBoolStat
{
    Berserk,
    Blunt,
    Carving,
    Meatshield,
    OurL,
    OurR,
    Porcupine,
    Lethal,
    Virgo,
    Capricon
}

public enum ExpBoolStat
{
    Hextech
}

public static class AbilityValueOperationExtensions
{
    public static float Apply(this AbilityValueOperation operation, float current, float value)
    {
        switch (operation)
        {
            case AbilityValueOperation.Add:
                return current + value;
            case AbilityValueOperation.Multiply:
                return current * value;
            case AbilityValueOperation.Set:
                return value;
            case AbilityValueOperation.Subtract:
                return current - value;
            default:
                return current;
        }
    }
}

public enum AbilityType
{
    Passive,
    Active,
    Area
}
