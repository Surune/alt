using UnityEngine;

public class ExperienceManager : MonoBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [SerializeField] private int popupExperienceThreshold = 10;

    public int CurrentExperience { get; private set; }

    private int nextPopupExperience;
    private int pendingPopupCount;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            nextPopupExperience = popupExperienceThreshold;
            return;
        }

        Destroy(gameObject);
    }

    public void AddExperience(int amount)
    {
        CurrentExperience += amount;

        while (CurrentExperience >= nextPopupExperience)
        {
            pendingPopupCount++;
            nextPopupExperience += popupExperienceThreshold;
        }

        TryOpenPopupCard();
    }

    public void HandlePopupCardClosed()
    {
        if (pendingPopupCount > 0)
        {
            TryOpenPopupCard();
            return;
        }

        GameStateManager.Instance.EnterPlayingState();
    }

    private void TryOpenPopupCard()
    {
        if (pendingPopupCount == 0 || UIManager.Instance.IsPopupCardOpen)
        {
            return;
        }

        pendingPopupCount--;
        UIManager.Instance.ShowPopupCard();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
