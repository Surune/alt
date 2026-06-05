using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] GameObject abilityPrefab;
    [SerializeField] AbilityData[] abilityCatalog;
    [SerializeField] GameObject content;

    readonly List<GameObject> abilityViews = new();
    readonly List<AbilityData> selectedAbilities = new();
    readonly List<int> selectedAbilityIds = new();
    Dictionary<int, AbilityData> abilityById;
    Dictionary<AbilityManagerFlag, Action> flagSetters;
    Player player;

    public IReadOnlyList<AbilityData> SelectedAbilities => selectedAbilities;
    public IReadOnlyList<int> SelectedAbilityIds => selectedAbilityIds;
    public int AbilityCount => abilityCatalog.Length;

    private void Awake()
    {
        Instance = this;
        player = FindFirstObjectByType<Player>();
        abilityCatalog = Resources.LoadAll<AbilityData>("Abilities");
        Synergy = new int[11];
        CacheAbilityCatalog();
        CacheFlagSetters();
    }

    private void CacheAbilityCatalog()
    {
        abilityById = new Dictionary<int, AbilityData>(abilityCatalog.Length);
        foreach (var ability in abilityCatalog)
        {
            abilityById.Add(ability.AbilityID, ability);
        }
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

    private void AddAbility(int id)
    {
        var abilityData = abilityById[id];
        selectedAbilities.Add(abilityData);
        selectedAbilityIds.Add(id);

        AddSynergy(abilityData.PrimarySynergy);
        AddSynergy(abilityData.SecondarySynergy);

        var ability = CreateAbilityView(abilityData);
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
            AddAbility(UnityEngine.Random.Range(0, AbilityCount));
        }
    }

    private void AddSynergy(int synergyType)
    {
        Synergy[synergyType]++;
    }

    private Ability CreateAbilityView(AbilityData abilityData)
    {
        var abilityViewObject = Instantiate(abilityPrefab, content.transform);
        abilityViews.Add(abilityViewObject);

        var ability = abilityViewObject.GetComponent<Ability>();
        var icon = abilityViewObject.transform.Find("Icon").GetComponent<Image>();
        icon.sprite = abilityData.Icon;

        ability.AbilityID = abilityData.AbilityID;
        ability.AbilityType = abilityData.AbilityType;
        ability.AbilityImage.color = SynergyColors[abilityData.PrimarySynergy];
        return ability;
    }
}
