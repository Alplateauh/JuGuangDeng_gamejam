using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_JumpState : PlayerState
{
    public Player_JumpState(Player player, PlayerFSM stateMachine, string animName) : base(player, stateMachine, animName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        player.rb.velocity = new Vector2(player.rb.velocity.x, 0);
        player.SetPlayerGravityScale(movementData.gravityScale);

        float force = movementData.jumpForce;
        if (TimeMgr.GetInstance().IsStop())
            player.rb.AddForce(force * Vector2.up / Time.timeScale, ForceMode2D.Impulse);
        else player.rb.AddForce(force * Vector2.up, ForceMode2D.Impulse);
    }


    public override void Update()
    {
        base.Update();
        
        if (player.isJumpCut)
        {
            player.SetPlayerGravityScale(movementData.gravityScale * movementData.jumpCutGravityMult);
            player.rb.velocity = new Vector2(player.rb.velocity.x, Mathf.Max(player.rb.velocity.y, -movementData.maxFallSpeed));
        }
        else if (Mathf.Abs(player.rb.velocity.y) < movementData.jumpHangTimeThreshold)
        {
            player.SetPlayerGravityScale(movementData.gravityScale * movementData.jumpHangGravityMult);
        }      
        else
        {
            player.SetPlayerGravityScale(movementData.gravityScale);
        }

        if (player.rb.velocity.y < 0)
        {
            player.stateMachine.ChangeState(player.fallState);
        }
    }
    
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        
        player.Run(1);
    }
    
    public override void Exit()
    {
        base.Exit();
        player.isJumping = false;
        player.ResetTimer(TimerType.LastPressJump);
    }
}
