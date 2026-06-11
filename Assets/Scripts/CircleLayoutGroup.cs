using UnityEngine;
using UnityEngine.UI;

public class CircleLayoutGroup : LayoutGroup
{
    [SerializeField] private float radius = 70f;
    [SerializeField] private float startAngle = 90f;
    [SerializeField] private bool clockwise = true;
    [SerializeField] private Vector2 childSize;
    [SerializeField] private float rotationOffset;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
    }

    public override void CalculateLayoutInputVertical()
    {
    }

    public override void SetLayoutHorizontal()
    {
        ArrangeChildren();
    }

    public override void SetLayoutVertical()
    {
        ArrangeChildren();
    }

    private void ArrangeChildren()
    {
        var angleStep = 360f / rectChildren.Count;
        var direction = clockwise ? -1f : 1f;

        for (var i = 0; i < rectChildren.Count; i++)
        {
            var angleDegrees = startAngle + (angleStep * i * direction);
            var angle = angleDegrees * Mathf.Deg2Rad;
            var position = ((Vector2.right * Mathf.Cos(angle)) + (Vector2.up * Mathf.Sin(angle))) * radius;
            var child = rectChildren[i];

            SetChildAlongAxis(child, 0, (rectTransform.rect.width - childSize.x) * 0.5f + position.x, childSize.x);
            SetChildAlongAxis(child, 1, (rectTransform.rect.height - childSize.y) * 0.5f - position.y, childSize.y);
            child.localRotation = Quaternion.Euler(0f, 0f, angleDegrees + 90f + rotationOffset);
        }
    }
}
