using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupCardUI : MonoBehaviour
{
    [SerializeField] private Transform cardRoot;
    [SerializeField] private Card cardPrefab;
    [SerializeField] private int refreshHealthCost = 1;

    private readonly List<Card> cards = new();
    private readonly HashSet<int> offeredAbilityIds = new();
    private AbilityManager abilityManager;
    private Player player;
    private Dictionary<string, string> localization;
    private Button refreshButton;
    private TMP_Text refreshButtonText;

    public bool Initialize()
    {
        abilityManager = GameManager.Instance.Ability;
        player = FindFirstObjectByType<Player>();
        localization = LoadLocalization();

        if (abilityManager.GetUnselectedAbilities().Length == 0)
        {
            return false;
        }

        CreateShopControls();
        PopulateCards(false);
        return true;
    }

    private void PopulateCards(bool excludeCurrentOffers)
    {
        ClearCards();

        var availableAbilities = new List<AbilityData>();
        var unselectedAbilities = abilityManager.GetUnselectedAbilities();
        for (var i = 0; i < unselectedAbilities.Length; i++)
        {
            var ability = unselectedAbilities[i];
            if (!excludeCurrentOffers || !offeredAbilityIds.Contains(ability.AbilityID))
            {
                availableAbilities.Add(ability);
            }
        }

        if (availableAbilities.Count == 0)
        {
            availableAbilities.AddRange(unselectedAbilities);
        }

        for (var i = availableAbilities.Count - 1; i > 0; i--)
        {
            var swapIndex = Random.Range(0, i + 1);
            var temp = availableAbilities[i];
            availableAbilities[i] = availableAbilities[swapIndex];
            availableAbilities[swapIndex] = temp;
        }

        offeredAbilityIds.Clear();
        var cardCount = Mathf.Min(4, availableAbilities.Count);
        for (var i = 0; i < cardCount; i++)
        {
            var ability = availableAbilities[i];
            var nameLkey = $"ability_{ability.AbilityID}_name";
            var descriptionLkey = $"ability_{ability.AbilityID}_description";
            var card = Instantiate(cardPrefab, cardRoot);
            card.Init(ability,
                localization[nameLkey],
                localization[descriptionLkey],
                ability.HealthCost,
                () => PurchaseAbility(ability, card));
            cards.Add(card);
            offeredAbilityIds.Add(ability.AbilityID);
        }

        UpdateRefreshButton();
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

    private void PurchaseAbility(AbilityData ability, Card card)
    {
        if (!player.CanSpendHealth(ability.HealthCost))
        {
            return;
        }

        player.SpendHealth(ability.HealthCost);
        abilityManager.AddAbility(ability);
        offeredAbilityIds.Remove(ability.AbilityID);
        card.Empty();
        UpdateRefreshButton();
    }

    private void Refresh()
    {
        if (!player.CanSpendHealth(refreshHealthCost))
        {
            return;
        }

        player.SpendHealth(refreshHealthCost);
        PopulateCards(true);
    }

    private void ClearCards()
    {
        for (var i = 0; i < cards.Count; i++)
        {
            cards[i].gameObject.SetActive(false);
            Destroy(cards[i].gameObject);
        }

        cards.Clear();
    }

    private void CreateShopControls()
    {
        var controls = new GameObject("ShopControls", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        controls.transform.SetParent(transform, false);

        var controlsRect = (RectTransform)controls.transform;
        controlsRect.anchorMin = new Vector2(0.3f, 0.76f);
        controlsRect.anchorMax = new Vector2(0.7f, 0.88f);
        controlsRect.offsetMin = Vector2.zero;
        controlsRect.offsetMax = Vector2.zero;

        var layout = controls.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 30f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        refreshButton = CreateButton(controls.transform, Refresh);
        refreshButtonText = refreshButton.GetComponentInChildren<TMP_Text>();

        var closeButton = CreateButton(controls.transform, Close);
        closeButton.GetComponentInChildren<TMP_Text>().text =
            Application.systemLanguage == SystemLanguage.Korean ? "나가기" : "Leave";
    }

    private static Button CreateButton(Transform parent, UnityEngine.Events.UnityAction action)
    {
        var buttonObject = new GameObject("ShopButton", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        var image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        var button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);

        var textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);

        var textRect = (RectTransform)textObject.transform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        var label = textObject.GetComponent<TMP_Text>();
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 32f;
        label.color = Color.white;

        return button;
    }

    private void UpdateRefreshButton()
    {
        var korean = Application.systemLanguage == SystemLanguage.Korean;
        refreshButtonText.text = korean
            ? $"새로고침\n체력 -{refreshHealthCost}"
            : $"Refresh\nHP -{refreshHealthCost}";
        refreshButton.interactable = player.CanSpendHealth(refreshHealthCost)
                                     && abilityManager.GetUnselectedAbilities().Length > 0;
    }

    private void Close()
    {
        UIManager.Instance.ClosePopupCard();
    }
}
