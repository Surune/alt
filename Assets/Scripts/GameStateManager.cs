using System;
using UnityEngine;

public enum GameState
{
    Playing,
    RoundTransition,
    CardPopup
}

public sealed class GameStateManager
{
    public event Action<GameState> StateChanged;

    public bool IsGameplayActive => currentState == GameState.Playing;
    private GameState currentState;

    public GameStateManager()
    {
        currentState = GameState.Playing;
        ApplyTimeScale();
    }

    public void EnterPlayingState()
    {
        currentState = GameState.Playing;
        ApplyTimeScale();
        StateChanged?.Invoke(currentState);
    }

    public void EnterCardPopupState()
    {
        currentState = GameState.CardPopup;
        ApplyTimeScale();
        StateChanged?.Invoke(currentState);
    }

    public void EnterRoundTransitionState()
    {
        currentState = GameState.RoundTransition;
        ApplyTimeScale();
        StateChanged?.Invoke(currentState);
    }

    private void ApplyTimeScale()
    {
        Time.timeScale = currentState == GameState.CardPopup ? 0f : 1f;
    }

    public void Dispose()
    {
        Time.timeScale = 1f;
        StateChanged = null;
    }
}
