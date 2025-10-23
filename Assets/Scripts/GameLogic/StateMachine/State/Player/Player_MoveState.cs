using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;

public class Player_MoveState : PlayerState
{
    public Player_MoveState(Player player, PlayerFSM stateMachine, string animName) : base(player, stateMachine, animName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.isJumping = false;
        player.hasChanged = false;
    }

    public override void Update()
    {
        base.Update();
        
        if (player.faceDir == 0) 
            stateMachine.ChangeState(player.idleState);
        
        if (player.canJump && player.isJumping) 
            stateMachine.ChangeState(player.jumpState);

        if (player.isWallMove) 
            stateMachine.ChangeState(player.wallMoveState);

        if (!player.isGround) 
            stateMachine.ChangeState(player.fallState);
    }
    
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        
        player.Run(1);
    }
    
    public override void Exit()
    {
        base.Exit();
    }
}