using UnityEngine;
using UnityEngine.UI;

public class Heart : MonoBehaviour
{
    [SerializeField] private Image heart;
    [SerializeField] private Color activeColor = Color.red;
    [SerializeField] private Color inactiveColor = Color.white;

    public void Activate()
    {
        heart.color = activeColor;
    }

    public void Deactivate()
    {
        heart.color = inactiveColor;
    }
}
