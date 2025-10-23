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
        player.SetPlayerGravityScale(0f);
        
        if (player.hitSide == 4)
            player.rb.velocity = new Vector2(player.rb.velocity.x, 0f);
    }

    public override void Update()
    {
        base.Update();

        HandlePlayerRotation();
        
        if (player.faceDir == 0) 
            stateMachine.ChangeState(player.idleState);

        if (!player.hitBlock || !player.isWallMove)
            stateMachine.ChangeState(player.fallState);

        if (player.isJumping) 
            stateMachine.ChangeState(player.jumpState);

        if (player.isRightChange || player.isLeftChange)
            stateMachine.ChangeState(player.changeSideState);
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
        switch (player.hitSide)
        {
            case 1:
            case 3:
                player.PlayerRotate(player.hitSide);
                break;
            case 2:
                break;
            case 4:
                player.transform.localScale =
                    new Vector3(player.transform.localScale.x, -1, player.transform.localScale.z);
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
