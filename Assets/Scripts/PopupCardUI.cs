using System.Collections.Generic;
using UnityEngine;

public class PopupCardUI : MonoBehaviour
{
    [SerializeField] private Transform cardRoot;
    [SerializeField] private Card cardPrefab;

    public bool Initialize()
    {
        var abilityManager = AbilityManager.Instance;
        var availableAbilities = new List<AbilityData>(abilityManager.GetUnselectedAbilities());
        if (availableAbilities.Count == 0)
        {
            return false;
        }

        for (var i = availableAbilities.Count - 1; i > 0; i--)
        {
            var swapIndex = Random.Range(0, i + 1);
            var temp = availableAbilities[i];
            availableAbilities[i] = availableAbilities[swapIndex];
            availableAbilities[swapIndex] = temp;
        }

        var localization = LoadLocalization();
        var cardCount = Mathf.Min(3, availableAbilities.Count);
        for (var i = 0; i < cardCount; i++)
        {
            var ability = availableAbilities[i];
            var nameLkey = $"ability_{ability.AbilityID}_name";
            var descriptionLkey = $"ability_{ability.AbilityID}_description";
            var card = Instantiate(cardPrefab, cardRoot);
            card.Init(ability, localization[nameLkey], localization[descriptionLkey]);
            card.SetClickAction(() => SelectAbility(abilityManager, ability));
        }

        return true;
    }

    private static Dictionary<string, string> LoadLocalization()
    {
        var locale = Application.systemLanguage == SystemLanguage.Korean ? "kr" : "en";
        var localization = new Dictionary<string, string>();
        var rows = CSVReader.Read("localization");

        foreach (var row in rows)
        {
            localization.Add((string)row["lkey"], row[locale].ToString());
        }

        return localization;
    }

    private void SelectAbility(AbilityManager abilityManager, AbilityData ability)
    {
        abilityManager.AddAbility(ability);
        Close();
    }

    private void Close()
    {
        UIManager.Instance.ClosePopupCard();
    }
}
