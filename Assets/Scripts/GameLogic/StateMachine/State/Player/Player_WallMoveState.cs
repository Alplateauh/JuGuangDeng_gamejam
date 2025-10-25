using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_WallMoveState : PlayerState
{
    public Player_WallMoveState(Player player, PlayerFSM stateMachine, string animName) : base(player, stateMachine, animName)
    {
    }
    
    public override void Enter()
    {
        base.Enter();
        player.isJumping = false;

        if (player.playerRot == 2) 
            player.SetPlayerGravityScale(movementData.gravityScale);
        else
            player.SetPlayerGravityScale(0f);
        
        if (player.playerRot == 4)
            player.rb.velocity = new Vector2(player.rb.velocity.x, 0f);
    }

    public override void Update()
    {
        base.Update();
            
        if (player.isFirstHit)
            HandlePlayerRotation();

        if (player.faceDir == 0)  
            stateMachine.ChangeState(player.idleState);

        if (player.canJump && player.isJumping && player.playerRot == 2)  
            stateMachine.ChangeState(player.jumpState);

        if ((player.isLeftRotate || player.isRightRotate) && player.hasHitPos && !player.isFirstHit) 
            stateMachine.ChangeState(player.changeRotationState);

        if ((player.isLeftChange || player.isRightChange) && !player.hasHitPos && !player.isFirstHit) 
            stateMachine.ChangeState(player.changeSideState);
        
        if (!player.isHitBlock && !player.isGround)
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
    
    private void HandlePlayerRotation()
    {
        switch (player.playerRot)
        {
            case 1:
                if (!player.hasHitPos) return; 
                player.rb.velocity = Vector2.zero;
                player.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                player.transform.position = new Vector3(player.blockHitPos.x - player.playerHeight / 2,
                    player.blockHitPos.y, 0);
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
                    player.blockHitPos.y, 0);
                player.isFirstHit = false;
                break;
            case 4:
                player.transform.localScale =
                    new Vector3(player.transform.localScale.x, -1, player.transform.localScale.z);
                player.isFirstHit = false;
                break;
        }
    }
    
    private void Gravity()
    {
        if (player.hitSide == 4)
        {
            player.rb.AddForce(Vector2.up * movementData.gravityScale);
        }
    }
}
