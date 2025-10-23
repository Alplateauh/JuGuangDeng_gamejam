using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_WallJumpState : PlayerState
{
    public Player_WallJumpState(Player player, PlayerFSM stateMachine, string animName) : base(player, stateMachine, animName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        player.isWallJumping = true;
        player.SetTimer(TimerType.LeaveBlockCoolDown, movementData.leaveBlockCooldown);
        
        player.rb.velocity = new Vector2(player.rb.velocity.x, 0f);
        player.SetPlayerGravityScale(movementData.gravityScale);
        
        float force = movementData.wallJumpForce;
        switch (player.playerRot)
        {
            case 1:
                player.transform.position = new Vector3(player.transform.position.x - player.playerLength / 2,
                    player.transform.position.y, player.transform.position.z);
                player.rb.AddForce(force * Vector2.left, ForceMode2D.Impulse);
                player.faceDir = -1;
                break;
            case 3:
                player.transform.position = new Vector3(player.transform.position.x + player.playerLength / 2,
                    player.transform.position.y, player.transform.position.z);
                player.rb.AddForce(force * Vector2.right, ForceMode2D.Impulse);
                player.faceDir = 1;
                break;
        }
    }

    public override void Update()
    {
        base.Update();
        stateMachine.ChangeState(player.fallState);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void Exit()
    {
        base.Exit();
        player.isWallJumping = false;
        player.faceDir = 0;
        player.ResetPlayerBlock();
    }
}
