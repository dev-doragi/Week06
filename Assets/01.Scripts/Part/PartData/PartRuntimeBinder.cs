using UnityEngine;

public class PartRuntimeBinder : MonoBehaviour
{
    public void Bind(PartRuntimeContext context)
    {
        IPartRuntimeBindable[] bindables = GetComponentsInChildren<IPartRuntimeBindable>(true);

        for (int i = 0; i < bindables.Length; i++)
        {
            bindables[i].BindRuntime(context);
        }
    }
}