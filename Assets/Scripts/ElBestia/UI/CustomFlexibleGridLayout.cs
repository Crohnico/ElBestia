using UnityEngine;
using UnityEngine.UI;

namespace ElBestia.UI
{
    public sealed class CustomFlexibleGridLayout : LayoutGroup
    {
        public Vector2 spacing = new Vector2(0.02f, 0.02f);

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
        }

        public override void CalculateLayoutInputVertical()
        {
        }

        public override void SetLayoutHorizontal()
        {
            LayoutChildren();
        }

        public override void SetLayoutVertical()
        {
            LayoutChildren();
        }

        private void LayoutChildren()
        {
            float availableWidth = Mathf.Max(0f, rectTransform.rect.width - padding.left - padding.right);
            float currentX = padding.left;
            float currentY = padding.top;
            float rowHeight = 0f;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform child = rectChildren[i];
                float childWidth = child.rect.width;
                float childHeight = child.rect.height;

                if (currentX > padding.left && currentX + childWidth > padding.left + availableWidth)
                {
                    currentX = padding.left;
                    currentY += rowHeight + spacing.y;
                    rowHeight = 0f;
                }

                SetChildAlongAxis(child, 0, currentX);
                SetChildAlongAxis(child, 1, currentY);

                currentX += childWidth + spacing.x;
                rowHeight = Mathf.Max(rowHeight, childHeight);
            }
        }
    }
}
