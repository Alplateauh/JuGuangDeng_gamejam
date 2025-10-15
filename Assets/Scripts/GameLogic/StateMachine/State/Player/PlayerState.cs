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
    /// �������״̬�ĳ�ʼ��
    /// </summary>
    /// <param name="player">���ʵ��</param>
    /// <param name="stateMachine">��ҵ�״̬��</param>
    /// <param name="animName">��Ӧ�Ķ�������</param>
    protected PlayerState(Player player, PlayerFSM stateMachine, string animName)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        this.animName = animName;
        movementData = player.movementData;
    }

    /// <summary>
    /// ״̬����ʱִ��
    /// </summary>
    public virtual void Enter()
    {
        player.animator.SetBool(animName, true); // ���ö���״̬Ϊ true
    }
    
    /// <summary>
    /// ״̬ÿִ֡��
    /// </summary>
    public virtual void Update()
    {
        
    }

    /// <summary>
    /// ״̬ÿ����ִ֡��
    /// </summary>
    public virtual void FixedUpdate()
    {
        
    }

    /// <summary>
    /// ״̬�˳�ʱִ��
    /// </summary>
    public virtual void Exit()
    {
        player.animator.SetBool(animName, false); // ���ö���״̬Ϊ false
    }
}