using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsyncPlugin : BasePlugin
{
    public override void Effect()
    {
        Debug.Log("AsyncPlugin Effect");
    }

    public override void RemoveEffect()
    {
        Debug.Log("AsyncPlugin RemoveEffect");
    }
}
