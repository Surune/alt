using TMPro;
using UnityEngine;

public class TimePanel : MonoBehaviour
{
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text timeText;

    public void UpdateDisplay(int round, float remainingTime)
    {
        roundText.text = $"라운드 {round}";
        timeText.text = Mathf.CeilToInt(remainingTime).ToString();
    }
}
