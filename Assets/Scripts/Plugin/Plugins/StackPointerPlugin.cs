using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StackPointerPlugin : BasePlugin
{
    public override void Effect()
    {
        if (player == null)
        {
            Debug.LogError("StackPointerPlugin 无法在同一个 GameObject 上找到 Player 组件！");
            return;
        }
        player.OpenOrCloseJumpInputWindow(true);
        Debug.Log("StackPointerPlugin Effect");
    }
    
    public override void RemoveEffect()
    {
        player.OpenOrCloseJumpInputWindow(false);
    }
}