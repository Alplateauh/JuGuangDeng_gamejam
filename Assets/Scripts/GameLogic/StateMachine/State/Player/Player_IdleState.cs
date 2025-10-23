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
        
        if (player.isWallMove)
        {
            switch (player.hitSide)
            {
                case 1:
                case 3:
                    player.rb.velocity = new Vector2(0f, 0f);
                    break;
                case 2: 
                    player.rb.velocity = new Vector2(0f, player.rb.velocity.y);
                    break;
                case 4:
                    player.rb.velocity = new Vector2(0f, 0f);
                    break;
            }
        }
        else
        {
            player.rb.velocity = new Vector2(0f, player.rb.velocity.y);
        }
    }

    public override void Update()
    {
        base.Update();

        if (player.isWallMove) 
            HandlePlayerRotation();
        
        if (player.faceDir != 0)
            stateMachine.ChangeState(player.moveState);

        if (player.canJump && player.isJumping)   
            stateMachine.ChangeState(player.jumpState);

        if (!player.isGround && !player.isWallMove)
            stateMachine.ChangeState(player.fallState);
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
                //player.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
                break;
        }
    }
}