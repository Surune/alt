using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityData", menuName = "Game/Ability Data")]
public class AbilityData : ScriptableObject
{
    [SerializeField] int abilityID;
    [SerializeField] AbilityType abilityType;
    [SerializeField] Sprite icon;
    [SerializeField] int primarySynergy;
    [SerializeField] int secondarySynergy;
    [SerializeField] List<AbilityEffect> effects = new List<AbilityEffect>();

    public int AbilityID => abilityID;
    public AbilityType AbilityType => abilityType;
    public Sprite Icon => icon;
    public int PrimarySynergy => primarySynergy;
    public int SecondarySynergy => secondarySynergy;

    public void Apply(AbilityApplyContext context)
    {
        for (var i = 0; i < effects.Count; i++)
        {
            effects[i].Apply(context);
        }
    }
}
