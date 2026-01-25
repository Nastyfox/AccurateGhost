using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuEventSystemHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private List<Selectable> selectables = new List<Selectable>();

    [SerializeField] protected Selectable firstSelected;
    protected Selectable lastSelected;

    [SerializeField] protected InputActionReference navigateReference;

    [SerializeField] protected List<GameObject> animationExclusions = new List<GameObject>();

    protected virtual void Awake()
    {
        foreach(Selectable selectable in selectables)
        {
            AddSelectionListeners(selectable);
        }
    }

    protected virtual void OnEnable()
    {
        navigateReference.action.performed += OnNavigate;

        if(lastSelected != null)
        {
            firstSelected = lastSelected;
        }

        if (firstSelected != null)
        {
            SelectAfterDelay().Forget();
        }
    }

    protected virtual void OnDisable()
    {
        navigateReference.action.performed -= OnNavigate;
    }

    public virtual void SetFirstSelected(Selectable selectable)
    {
        firstSelected = selectable;
    }

    public virtual void AddSelectable(Selectable selectable)
    {
        selectables.Add(selectable);
        AddSelectionListeners(selectable);
    }
    public virtual void AddAnimationExclusion(Selectable selectable)
    {
        animationExclusions.Add(selectable.gameObject);
    }

    protected virtual async UniTask SelectAfterDelay()
    {
        await UniTask.Yield();
        EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
    }

    protected virtual void AddSelectionListeners(Selectable selectable)
    {
        EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = selectable.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry selectEntry = new EventTrigger.Entry()
        {
            eventID = EventTriggerType.Select
        };
        selectEntry.callback.AddListener(OnSelect);
        trigger.triggers.Add(selectEntry);

        EventTrigger.Entry deselectEntry = new EventTrigger.Entry()
        {
            eventID = EventTriggerType.Deselect
        };
        deselectEntry.callback.AddListener(OnDeselect);
        trigger.triggers.Add(deselectEntry);

        EventTrigger.Entry pointerEnter = new EventTrigger.Entry()
        {
            eventID = EventTriggerType.PointerEnter
        };
        pointerEnter.callback.AddListener(OnPointerEnter);
        trigger.triggers.Add(pointerEnter);

        EventTrigger.Entry pointerExit = new EventTrigger.Entry()
        {
            eventID = EventTriggerType.PointerExit
        };
        pointerExit.callback.AddListener(OnPointerExit);
        trigger.triggers.Add(pointerExit);
    }

    public void OnSelect(BaseEventData baseEventData)
    {
        GameObject selectedGO = baseEventData.selectedObject;

        lastSelected = selectedGO.GetComponent<Selectable>();

        if(animationExclusions.Contains(selectedGO))
        {
            return;
        }

        MenuManager.menuManagerInstance.ButtonSelected(selectedGO).Forget();
    }

    public void OnDeselect(BaseEventData baseEventData)
    {
        GameObject selectedGO = baseEventData.selectedObject;

        if (animationExclusions.Contains(selectedGO))
        {
            return;
        }

        MenuManager.menuManagerInstance.ButtonDeselected(selectedGO).Forget();
    }

    public void OnPointerEnter(BaseEventData baseEventData)
    {
        PointerEventData pointerEventData = baseEventData as PointerEventData;

        if (pointerEventData != null)
        {
            Selectable selectable = pointerEventData.pointerEnter.GetComponentInParent<Selectable>();
            if (selectable == null)
            {
                selectable = pointerEventData.pointerEnter.GetComponentInChildren<Selectable>();
            }

            pointerEventData.selectedObject = selectable.gameObject;
        }
    }

    public void OnPointerExit(BaseEventData baseEventData)
    {
        PointerEventData pointerEventData = baseEventData as PointerEventData;

        if (pointerEventData != null)
        {
            pointerEventData.selectedObject = null;
        }
    }

    protected virtual void OnNavigate(InputAction.CallbackContext context)
    {
        if(EventSystem.current.currentSelectedGameObject == null && lastSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(lastSelected.gameObject);
        }
    }
}
