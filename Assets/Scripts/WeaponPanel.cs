using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPanel : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private TMP_Text attackCost;

    public void Init(WeaponData data)
    {
        icon.sprite = data.Icon;
        displayName.text = data.DisplayName;
        attackCost.text = "HP -1 / ATK";
    }
}
