public class AbilityApplyContext
{
    public AbilityApplyContext(AbilityManager abilityManager, Ability ability, AbilityData abilityData, Player player)
    {
        AbilityManager = abilityManager;
        Ability = ability;
        AbilityData = abilityData;
        Player = player;
    }

    public AbilityManager AbilityManager { get; }
    public Ability Ability { get; }
    public AbilityData AbilityData { get; }
    public Player Player { get; }
}
