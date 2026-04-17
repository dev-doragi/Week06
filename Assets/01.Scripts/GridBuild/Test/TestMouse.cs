using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TestMouse : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int key;

    public void OnPointerDown(PointerEventData eventData)
    {
        BuildManager.Instance.SelectPart(key);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        BuildManager.Instance.TryPlaceCurrentPart();
    }
}
