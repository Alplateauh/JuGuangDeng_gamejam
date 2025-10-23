using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_HitWallState : PlayerState
{
    public Player_HitWallState(Player player, PlayerFSM stateMachine, string animName) : base(player, stateMachine, animName)
    {
    }
    
    private float hitWallDuration = 0.1f;
    private float timer;
    
    public override void Enter()
    {
        base.Enter();
        timer = 0f;
    }

    public override void Update()
    {
        base.Update();

        timer += Time.deltaTime;
        if (timer <= hitWallDuration)
        {
            HandlePlayerRotation();
        }
        else
        {
            player.isWallMove = true;
            stateMachine.ChangeState(player.wallMoveState);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void Exit()
    {
        base.Exit();
    }
    
    private void HandlePlayerRotation()
    {
        player.PlayerRotate(player.playerRot);
        
        // if (player.playerRot == 2) 
        //     player.SetPlayerGravityScale(movementData.gravityScale);
        // else
        //     player.SetPlayerGravityScale(0f);
        //
        // if (player.playerRot == 4)
        //     player.rb.velocity = new Vector2(player.rb.velocity.x, 0f);
    }
}
