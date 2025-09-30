using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using UnityEngine.InputSystem;

namespace cowsins.Inventory
{
    /// <summary>
    /// Allows to visually indicate the current hovered Item ( If available ) in the Inventory.
    /// </summary>
    public class TooltipManager : MonoBehaviour
    {
        [SerializeField, Title("TOOLTIP MANAGER"), Tooltip("Image that displays the Icon of the current Dragged Item.")] private Image dragImage;

        [SerializeField, Tooltip("Object that contains the Tooltip UI")] private GameObject tooltip;

        [SerializeField, Tooltip("Text that displays the Input Keybind to rotate text ( only visible if using Tetris Inventory )")] private GameObject rotateText;

        [SerializeField, Tooltip("Text that displays the current hovered Item Name")] private TextMeshProUGUI tooltipText;

        [SerializeField, Tooltip("Text that displays the current hovered Item Description")] private TextMeshProUGUI tooltipDescription;

        [SerializeField, Tooltip("Offset Position added to the tooltip to avoid displaying it exactly on the mouse, for better readability")] private Vector2 tooltipOffset = new Vector2(50,0);

        [SerializeField, Tooltip("Horizontal space added to the text at the end of the line")] private float rightPadding = 30;

        private RectTransform rectTransform;

        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();

            SetActive(false);
            Disable();
        }

        private void Update()
        {
            // If mouse is not available or it´s not moving, dont update its position
            if (Mouse.current?.delta.magnitude < .1f) return;

            // Update Tooltip position to the current mouse position
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            rectTransform.position = mousePosition;
        }

        /// <summary>
        /// Visually enables or disables Tooltip and activates or deactivates it 
        /// </summary>
        public void SetActive(bool value) => tooltip.gameObject.SetActive(value);

        /// <summary>
        /// Update Drag Image State ( Whether it is visible or not ) and the visuals to display
        /// </summary>
        public void SetDragImageVisibility(bool visible, Sprite sprite, bool showRotateText)
        {
            dragImage.gameObject.SetActive(visible);
            dragImage.sprite = visible && sprite ? sprite : null;
            rotateText.SetActive(showRotateText);
            if (visible) SetTooltipVisibility(false, null, null);
        }
        /// <summary>
        /// Update Tooltip State ( Whether it is visible or not ) and the Item Parameters to represent ( Name & Description )
        /// </summary>
        public void SetTooltipVisibility(bool visible, string name, string description)
        {
            // If dragging, Tooltip cannot be shown
            if (dragImage.gameObject.activeInHierarchy)
            {
                tooltip.gameObject.SetActive(false);
                return;
            }

            // Update Texts
            tooltipText.text = name;
            if (string.IsNullOrEmpty(description))
                tooltipDescription.gameObject.SetActive(false);
            else
            {
                tooltipDescription.gameObject.SetActive(true);
                tooltipDescription.text = description;
            }

            // Adjust Tooltip width & height to adapt based on the name & description passed to the tooltip, to prevent overflow issues.
            float preferredWidth = string.IsNullOrEmpty(description) || tooltipDescription.preferredWidth < tooltipText.preferredWidth ?
                tooltipText.preferredWidth + 20 : tooltipDescription.GetComponent<RectTransform>().sizeDelta.x + 10;

            float preferredHeight = string.IsNullOrEmpty(description) || tooltipDescription.preferredWidth < tooltipText.preferredWidth ?
                30 : tooltipDescription.GetComponent<RectTransform>().sizeDelta.y + tooltipText.GetComponent<RectTransform>().sizeDelta.y + 20;

            RectTransform tooltipRect = tooltip.GetComponent<RectTransform>();
            tooltipRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredWidth + rightPadding);
            tooltipRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredHeight);

            tooltip.gameObject.SetActive(visible);
        }

        /// <summary>
        /// Disables the Tooltip so it cannot be shown nor used.
        /// </summary>
        public void Disable()
        {
            SetTooltipVisibility(false, "", "");
            SetDragImageVisibility(false, null, false);
            SetActive(false);
        }

        /// <summary>
        /// Rotates Drag Image. Used by Tetris Inventory only. Items in Grid Inventory cannot be rotated.
        /// </summary>
        /// <param name="rotation"></param>
        public void Rotate(int rotation) => dragImage.transform.rotation = Quaternion.Euler(0, 0, rotation);

        /// <summary>
        /// Updates Tooltip Position in the UI
        /// </summary>
        /// <param name="pos"></param>
        public void SetPosition(Vector2 pos) => rectTransform.position = (Vector3)pos + new Vector3(tooltipOffset.x, tooltipOffset.y, 0);
    }
}