using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_FallState : PlayerState
{
    private float timer;
    private float deltaTime;
    
    public Player_FallState(Player player, PlayerFSM stateMachine, string animName) : base(player, stateMachine, animName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        player.SetPlayerGravityScale(movementData.gravityScale);
        player.transform.rotation = Quaternion.Euler(0, 0, 0);
        player.transform.localScale = new Vector3(player.transform.localScale.x, 1, player.transform.localScale.z);
    }
    
    public override void Update()
    {
        base.Update();
        
        if (Mathf.Abs(player.rb.velocity.y) < movementData.jumpHangTimeThreshold * 1f / Time.timeScale)
        {
            player.SetPlayerGravityScale(movementData.gravityScale * movementData.jumpHangGravityMult);
        }
        else
        {
            player.SetPlayerGravityScale(movementData.gravityScale * movementData.fallGravityMult);
        }

        player.rb.velocity = new Vector2(player.rb.velocity.x,
            Mathf.Max(player.rb.velocity.y, -movementData.maxFallSpeed * 1f / Time.timeScale));
        
        if (player.isJumping)
            stateMachine.ChangeState(player.jumpState);

        if (player.isWallMove) 
            player.stateMachine.ChangeState(player.wallMoveState);
    }
    
    public override void FixedUpdate()
    {
        base.FixedUpdate();
        
        player.Run(1);
    }

    
    public override void Exit()
    {
        base.Exit();
        player.isJumpCut = false;
        //if(player.isGround) player.jumpParticle.Play();
        
        //player.hasChanged = false;
    }
}