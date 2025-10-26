using System;
using UnityEngine;
using UnityEngine.UI;

public enum InteractionType
{
    X0Register,
    StackPointer,
    GCAutoCollect,
    Async,
    LibraryHooking,
    NoNVIDIA
}

public abstract class BasePlugin
{
    public PluginData pluginData;

    public Text InvokeUI;

    protected Player player;
    public virtual void Effect(){}
    public virtual void RemoveEffect(){}
    public virtual void SetPlayer(Player player)
    {
        this.player = player;
        if (player != null)
        {
            Effect();
        }
        else
        {
            RemoveEffect();
        }
    }
}