using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    private void Awake()
    {
        SelectionListener();
    }

    private void SelectionListener()
    {
        EventTrigger trigger = dropdown.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = dropdown.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry pointerClickEntry = new EventTrigger.Entry()
        {
            eventID = EventTriggerType.PointerClick
        };
        pointerClickEntry.callback.AddListener(OnDropdownOpened);
        trigger.triggers.Add(pointerClickEntry);

        EventTrigger.Entry submitEntry = new EventTrigger.Entry()
        {
            eventID = EventTriggerType.Submit
        };
        submitEntry.callback.AddListener(OnDropdownOpened);
        trigger.triggers.Add(submitEntry);
    }

    private void OnDropdownOpened(BaseEventData eventData)
    {
        ScrollRect scrollRect = GetComponentInChildren<ScrollRect>();

        if (scrollRect != null)
        {
            int selectedIndex = dropdown.value + 1;
            Transform itemsContainer = scrollRect.content;

            if (itemsContainer != null && selectedIndex < itemsContainer.childCount)
            {
                RectTransform selectedItem = itemsContainer.GetChild(selectedIndex) as RectTransform;
                if (selectedItem != null)
                {
                    scrollRect.ScrollToCenter(selectedItem);
                }
            }
        }
    }
}
