using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoNVIDIAPlugin : BasePlugin
{
    void OnEnable()
    {
        PlayerEnter();
    }

    private void OnDisable()
    {
        PlayerExit();
    }

    public override void Effect()
    {
        Debug.Log("X0RegisterPlugin Effect");
    }
}
