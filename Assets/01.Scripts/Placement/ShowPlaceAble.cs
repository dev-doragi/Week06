using UnityEngine;
using TMPro;

public class ShowPlaceAble : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _UIText;


    public void ReFreshPlaceableUI()
    {
        int _currentBlockCount = 0;
        int _maxBlockCount = 50;


        _UIText.text = $"{_currentBlockCount} / {_maxBlockCount}";
    }
}
