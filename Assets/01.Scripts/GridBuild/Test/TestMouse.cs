using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TestMouse : MonoBehaviour, IPointerClickHandler
{
    public int key;

    public void OnPointerClick(PointerEventData eventData)
    {
        BuildManager.Instance.SelectPart(key);
    }

}
