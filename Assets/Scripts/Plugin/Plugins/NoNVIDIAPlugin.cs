using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoNVIDIAPlugin : BasePlugin
{
    public override void Effect()
    {
        Debug.Log("NoNVDIAPlugin Effect");
    }

    public override void RemoveEffect()
    {
        Debug.Log("NoNVDIAPlugin RemoveEffect");
    }
}
