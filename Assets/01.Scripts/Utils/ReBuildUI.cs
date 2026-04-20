using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ReBuildUI : MonoBehaviour
{
    private RectTransform root;

    public void Awake()
    {
        root = GetComponent<RectTransform>();
    }
    private void OnEnable()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        StartCoroutine(RebuildAtEndOfFrame());
    }

    public IEnumerator RebuildAtEndOfFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(root);
    }
}
