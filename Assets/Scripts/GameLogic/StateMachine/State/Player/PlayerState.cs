using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState
{
    protected PlayerFSM stateMachine;
    protected Player player;
    protected PlayerMovementData movementData;
    private string animName;

    /// <summary>
    /// 用于玩家状态的初始化
    /// </summary>
    /// <param name="player">玩家实例</param>
    /// <param name="stateMachine">玩家的状态机</param>
    /// <param name="animName">对应的动画名称</param>
    protected PlayerState(Player player, PlayerFSM stateMachine, string animName)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        this.animName = animName;
        movementData = player.movementData;
    }

    /// <summary>
    /// 状态进入时执行
    /// </summary>
    public virtual void Enter()
    {
        player.animator.SetBool(animName, true); // 设置动画状态为 true
    }
    
    /// <summary>
    /// 状态每帧执行
    /// </summary>
    public virtual void Update()
    {
        
    }

    /// <summary>
    /// 状态每物理帧执行
    /// </summary>
    public virtual void FixedUpdate()
    {
        
    }

    /// <summary>
    /// 状态退出时执行
    /// </summary>
    public virtual void Exit()
    {
        player.animator.SetBool(animName, false); // 设置动画状态为 false
    }
}