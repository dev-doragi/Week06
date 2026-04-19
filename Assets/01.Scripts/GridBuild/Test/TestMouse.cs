using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TestMouse : MonoBehaviour, IPointerClickHandler
{
    public int key;
    [SerializeField] BuildManager _buildManager;

    public void OnPointerClick(PointerEventData eventData)
    {
        _buildManager.SelectPart(key);
    }

}
