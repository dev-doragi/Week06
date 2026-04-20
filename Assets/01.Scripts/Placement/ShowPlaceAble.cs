using UnityEngine;
using TMPro;

public class ShowPlaceAble : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _UIText;


    public void ReFreshPlaceableUI(int current, int max)
    {
        if (_UIText == null) return;
        _UIText.text = $"{Mathf.Clamp(current, 0, max)} / {max}";
         _UIText.color = current >= max ? Color.red : Color.white;
    }
}
