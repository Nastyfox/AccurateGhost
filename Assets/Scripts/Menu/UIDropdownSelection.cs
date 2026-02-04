using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDropdownSelection : MonoBehaviour
{
    private void Awake()
    {
        SelectionListener();
    }

    private void SelectionListener()
    {
        EventTrigger trigger = this.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = this.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry selectEntry = new EventTrigger.Entry()
        {
            eventID = EventTriggerType.Select
        };
        selectEntry.callback.AddListener(OnSelect);
        trigger.triggers.Add(selectEntry);
    }
    private void OnSelect(BaseEventData eventData)
    {
        ScrollRect scrollRect = GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.ScrollToCenter(transform as RectTransform);
        }
    }
}
