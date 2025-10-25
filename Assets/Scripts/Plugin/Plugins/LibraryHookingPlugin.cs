using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LibraryHookingPlugin : BasePlugin
{
    public override void Effect()
    {
        Debug.Log("LibraryHookingPlugin Effect");
    }

    public override void RemoveEffect()
    {
        Debug.Log("LibraryHookingPlugin RemoveEffect");
    }
}
