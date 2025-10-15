using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player_IdleState : PlayerState
{
    public Player_IdleState(Player player, PlayerFSM stateMachine, string animName) : base(player, stateMachine, animName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        player.isJumping = false;
        player.rb.velocity = new Vector2(0, player.rb.velocity.y);
    }

    public override void Update()
    {
        base.Update();

        if (player.faceDir != 0)
            stateMachine.ChangeState(player.moveState);

        if (player.isJumping) 
            stateMachine.ChangeState(player.jumpState);

        if (!player.isGround)
            stateMachine.ChangeState(player.fallState);
    }
    
    public override void Exit()
    {
        base.Exit();
    }
}