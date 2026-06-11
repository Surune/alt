using UnityEngine;
using UnityEngine.UI;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;
    
    public void Activate()
    {
        image.color = activeColor;
    }

    public void Inactivate()
    {
        image.color = inactiveColor;
    }
}
