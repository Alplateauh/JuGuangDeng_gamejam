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
    }

    public override void Update()
    {
        base.Update();

        HoldWallStandVelocity();

        if (player.isFirstHit)
            HandlePlayerRotation();
        
        if (player.faceDir != 0 && player.isWallMove)
            stateMachine.ChangeState(player.wallMoveState);

        if (player.canJump && player.isJumping && player.playerRot == 2)   
            stateMachine.ChangeState(player.jumpState);

        if (player.canJump && player.isWallJumping && player.isHitBlock)
            stateMachine.ChangeState(player.wallJumpState);

        if (!player.canJump && player.isLeaving && player.isHitBlock) 
            stateMachine.ChangeState(player.wallLeaveState);

        if (!player.isGround && !player.isWallMove)
            stateMachine.ChangeState(player.fallState);
        
        if ((player.isLeftRotate || player.isRightRotate) && player.hasHitPos)
            stateMachine.ChangeState(player.changeRotationState);

        if ((player.isLeftChange || player.isRightChange) && !player.hasHitPos) 
            stateMachine.ChangeState(player.changeSideState);
    }
    
    public override void Exit()
    {
        base.Exit();
    }

    private void HoldWallStandVelocity()
    {
        if (player.isHitBlock)
        {
            switch (player.playerRot)
            {
                case 1:
                case 3:
                case 4:
                    player.rb.velocity = new Vector2(0f, 0f);
                    player.SetPlayerGravityScale(0f);
                    break;
                case 2: 
                    player.rb.velocity = new Vector2(0f, player.rb.velocity.y);
                    player.SetPlayerGravityScale(movementData.gravityScale);
                    break;
            }
        }
        else
        {
            player.rb.velocity = new Vector2(0f, player.rb.velocity.y);
        }
    }
    
    private void HandlePlayerRotation()
    {
        switch (player.playerRot)
        {
            case 1:
                if (!player.hasHitPos) return; 
                player.rb.velocity = Vector2.zero;
                player.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                player.transform.position = new Vector3(player.blockHitPos.x - player.playerHeight / 2,
                    player.blockHitPos.y + player.playerLength / 2, 0);
                player.isFirstHit = false;
                break;
            case 2:
                player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                player.isFirstHit = false;
                break;
            case 3:
                if (!player.hasHitPos) return; 
                player.rb.velocity = Vector2.zero;
                player.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                player.transform.position = new Vector3(player.blockHitPos.x + player.playerHeight / 2,
                    player.blockHitPos.y + player.playerLength / 2, 0);
                player.isFirstHit = false;
                break;
            case 4:
                player.transform.localScale =
                    new Vector3(player.transform.localScale.x, -1, player.transform.localScale.z);
                player.isFirstHit = false;
                break;
        }
    }
}