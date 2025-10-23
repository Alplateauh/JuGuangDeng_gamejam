using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GCAutoCollectPlugin : BasePlugin
{
    public override void Effect()
    {
        Debug.Log("GCAutoCollectPlugin Effect");
    }

    public override void RemoveEffect()
    {
        Debug.Log("GCAutoCollectPlugin RemoveEffect");
    }
}
