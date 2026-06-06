using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance { get; private set; }

    [HideInInspector] public int[] Synergy;
    public Color[] SynergyColors;

    public bool assassination = false;
    public bool penetrate = false;
    public bool luckySeven = false;
    public bool beingstronger = false;
    public bool third = false;
    public bool nuker = false;
    public bool culling = false;
    public bool noxious = false;
    public bool psychosense = false;
    public bool psychosink = false;
    public bool freezing = false;
    public bool locked = false;
    public bool awaken = false;
    public bool burst = false;
    public bool echo = false;
    public bool immolation = false;
    public bool explode = false;
    public bool magnet = false;
    public bool burning = false;
    public bool kineticBarrage = false;
    public bool udo = false;
    public bool fifth = false;
    public bool fracture = false;
    public bool firm = false;

    AbilityData[] abilityCatalog;
    readonly List<AbilityData> selectedAbilities = new();
    Dictionary<AbilityManagerFlag, Action> flagSetters;
    Player player;

    public IReadOnlyList<AbilityData> SelectedAbilities => selectedAbilities;
    public int AbilityCount => abilityCatalog.Length;

    private void Awake()
    {
        Instance = this;
        player = FindFirstObjectByType<Player>();
        abilityCatalog = Resources.LoadAll<AbilityData>("Abilities");
        Synergy = new int[11];
        CacheFlagSetters();
    }

    private void CacheFlagSetters()
    {
        flagSetters = new Dictionary<AbilityManagerFlag, Action>
        {
            { AbilityManagerFlag.Assassination, () => assassination = true },
            { AbilityManagerFlag.Penetrate, () => penetrate = true },
            { AbilityManagerFlag.LuckySeven, () => luckySeven = true },
            { AbilityManagerFlag.BeingStronger, () => beingstronger = true },
            { AbilityManagerFlag.Third, () => third = true },
            { AbilityManagerFlag.Nuker, () => nuker = true },
            { AbilityManagerFlag.Culling, () => culling = true },
            { AbilityManagerFlag.Noxious, () => noxious = true },
            { AbilityManagerFlag.Psychosense, () => psychosense = true },
            { AbilityManagerFlag.Psychosink, () => psychosink = true },
            { AbilityManagerFlag.Freezing, () => freezing = true },
            { AbilityManagerFlag.Locked, () => locked = true },
            { AbilityManagerFlag.Awaken, () => awaken = true },
            { AbilityManagerFlag.Burst, () => burst = true },
            { AbilityManagerFlag.Echo, () => echo = true },
            { AbilityManagerFlag.Immolation, () => immolation = true },
            { AbilityManagerFlag.Explode, () => explode = true },
            { AbilityManagerFlag.Magnet, () => magnet = true },
            { AbilityManagerFlag.Burning, () => burning = true },
            { AbilityManagerFlag.KineticBarrage, () => kineticBarrage = true },
            { AbilityManagerFlag.Udo, () => udo = true },
            { AbilityManagerFlag.Fifth, () => fifth = true },
            { AbilityManagerFlag.Fracture, () => fracture = true },
            { AbilityManagerFlag.Firm, () => firm = true }
        };
    }

    public AbilityData[] GetUnselectedAbilities()
    {
        var abilities = new List<AbilityData>();
        for (var i = 0; i < abilityCatalog.Length; i++)
        {
            var ability = abilityCatalog[i];
            if (!selectedAbilities.Contains(ability))
            {
                abilities.Add(ability);
            }
        }

        return abilities.ToArray();
    }

    public void AddAbility(AbilityData abilityData)
    {
        selectedAbilities.Add(abilityData);

        AddSynergy(abilityData.PrimarySynergy);
        AddSynergy(abilityData.SecondarySynergy);

        var ability = CreateAbility(abilityData);
        var context = new AbilityApplyContext(this, ability, abilityData, player);
        abilityData.Apply(context);
    }

    public void SetFlag(AbilityManagerFlag flag)
    {
        flagSetters[flag]();
    }

    public void ChooseRandomAbilities(int count)
    {
        for (var i = 0; i < count; i++)
        {
            AddAbility(abilityCatalog[UnityEngine.Random.Range(0, AbilityCount)]);
        }
    }

    private void AddSynergy(int synergyType)
    {
        Synergy[synergyType]++;
    }

    private Ability CreateAbility(AbilityData abilityData)
    {
        var abilityObject = new GameObject(abilityData.name);
        abilityObject.transform.SetParent(transform);
        var ability = abilityObject.AddComponent<Ability>();
        ability.AbilityID = abilityData.AbilityID;
        ability.AbilityType = abilityData.AbilityType;
        return ability;
    }
}
