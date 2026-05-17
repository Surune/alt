using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerMoveHandler, IPointerDownHandler, IPointerExitHandler
{
    [Header("Target")]
    [SerializeField] private RectTransform cardRoot;
    [SerializeField] private Image foilOverlay;

    [Header("Rotate")]
    [SerializeField] private float rotateAmount = 20f;
    [SerializeField] private float smooth = 12f;

    [Header("Foil")]
    [SerializeField] private float foilOpacity = 0.8f;

    [Header("Data")] 
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private TMP_Text cardDescription;

    private Action clickAction;
    private Material foilMat;
    private Quaternion targetRotation;

    private void Awake()
    {
        targetRotation = Quaternion.identity;

        foilMat = Instantiate(foilOverlay.material);
        foilOverlay.material = foilMat;
    }

    public void Init(WeaponData data)
    {
        icon.sprite = data.Icon;
        cardName.text = data.DisplayName;
        cardDescription.text = data.Description;
    }

    private void Update()
    {
        cardRoot.localRotation = Quaternion.Lerp(cardRoot.localRotation, targetRotation, Time.unscaledDeltaTime * smooth);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cardRoot,
            eventData.position,
            eventData.pressEventCamera,
            out var localPoint
        );

        var size = cardRoot.rect.size;

        var nx = Mathf.Clamp(localPoint.x / (size.x * 0.5f), -1f, 1f);
        var ny = Mathf.Clamp(localPoint.y / (size.y * 0.5f), -1f, 1f);

        var rotY = -nx * rotateAmount;
        var rotX = ny * rotateAmount;

        targetRotation = Quaternion.Euler(rotX, rotY, 0f);

        var foilPos = Vector4.zero;
        foilPos.x = Mathf.InverseLerp(-1f, 1f, nx);
        foilPos.y = Mathf.InverseLerp(-1f, 1f, ny);

        foilMat.SetVector("_FoilPosition", foilPos);
        foilMat.SetFloat("_Opacity", foilOpacity);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        clickAction();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        targetRotation = Quaternion.identity;
        foilMat.SetFloat("_Opacity", 0f);
    }

    public void SetClickAction(Action action)
    {
        clickAction = action;
    }
}
