using System;
using UnityEngine;

public enum GameState
{
    Playing,
    CardPopup
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public event Action<GameState> StateChanged;

    public GameState CurrentState { get; private set; } = GameState.Playing;
    public bool IsGameplayActive => CurrentState == GameState.Playing;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            ApplyTimeScale();
            return;
        }

        Destroy(gameObject);
    }

    public void EnterPlayingState()
    {
        CurrentState = GameState.Playing;
        ApplyTimeScale();
        StateChanged?.Invoke(CurrentState);
    }

    public void EnterCardPopupState()
    {
        CurrentState = GameState.CardPopup;
        ApplyTimeScale();
        StateChanged?.Invoke(CurrentState);
    }

    private void ApplyTimeScale()
    {
        Time.timeScale = CurrentState == GameState.Playing ? 1f : 0f;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Time.timeScale = 1f;
            Instance = null;
        }
    }
}
