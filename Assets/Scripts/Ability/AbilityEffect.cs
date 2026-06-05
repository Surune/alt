using System;
using UnityEngine;

[Serializable]
public class AbilityEffect
{
    [SerializeField] AbilityEffectKind kind;
    [SerializeField] AbilityManagerFlag abilityManagerFlag;
    [SerializeField] AbilityValueOperation operation;
    [SerializeField] PlayerManagerFloatStat playerManagerFloatStat;
    [SerializeField] PlayerManagerBoolStat playerManagerBoolStat;
    [SerializeField] SpawnerFloatStat spawnerFloatStat;
    [SerializeField] SpawnerBoolStat spawnerBoolStat;
    [SerializeField] HPBoolStat hpBoolStat;
    [SerializeField] ExpBoolStat expBoolStat;
    [SerializeField] float value;
    [SerializeField] float secondValue;
    [SerializeField] int amount;
    [SerializeField] int secondAmount;
    [SerializeField] bool enabledValue = true;

    public void Apply(AbilityApplyContext context)
    {
        switch (kind)
        {
            case AbilityEffectKind.SetAbilityCooldown:
                context.Ability.SkillCooltimeMax = value;
                break;
            case AbilityEffectKind.SetAbilityManagerFlag:
                context.AbilityManager.SetFlag(abilityManagerFlag);
                break;
            case AbilityEffectKind.ChooseRandomAbilities:
                context.AbilityManager.ChooseRandomAbilities(amount);
                break;
        }
    }
}
