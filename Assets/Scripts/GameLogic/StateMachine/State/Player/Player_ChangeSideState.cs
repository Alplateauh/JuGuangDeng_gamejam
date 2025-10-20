using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_ChangeSideState : PlayerState
{
    public Player_ChangeSideState(Player player, PlayerFSM stateMachine, string animName) : base(player, stateMachine, animName)
    {
    }
    private int lastHitSide;

    public override void Enter()
    {
        base.Enter();
        lastHitSide = player.hitSide;
        player.P_lastHitSide = player.hitSide;
        player.rb.velocity = Vector2.zero;
        player.hasChanged = true;
        
        player.rb.velocity = Vector2.zero;
        TeleportPlayer(player.hitSide);
    }

    public override void Update()
    {
        base.Update();
        
        stateMachine.ChangeState(player.wallMoveState);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void Exit()
    {
        base.Exit();
        
        player.isRightChange = false;
        player.isLeftChange = false;
    }

    private void  TeleportPlayer(int hitSide)
    {
        if (player.block == null || player.blockCornerPos == null || player.blockCornerPos.Length < 4 || player.changePos == null)
            return;

        if (player.isRightChange)
        {
            switch (hitSide)
            {
                case 1:
                    player.transform.position = new Vector3(player.blockCornerPos[0].x + 0.5f,
                        player.blockCornerPos[0].y + player.playerHeight / 2, 0);
                    player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    player.hitSide = 2;
                    break;
                case 2:
                    player.transform.position = new Vector3(player.blockCornerPos[1].x + player.playerHeight / 2,
                        player.blockCornerPos[1].y - 0.5f, 0f);
                    player.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                    player.hitSide = 3;
                    break;
                case 3:
                    player.transform.position = new Vector3(player.blockCornerPos[2].x - 0.5f,
                        player.blockCornerPos[2].y - player.playerHeight / 2, 0f);
                    player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    player.transform.localScale =
                        new Vector3(-player.transform.localScale.x, -1, player.transform.localScale.z);
                    player.hitSide = 4;
                    break;
                case 4:
                    player.transform.position = new Vector3(player.blockCornerPos[3].x - player.playerHeight / 2,
                        player.blockCornerPos[3].y + 0.5f, 0f);
                    player.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                    player.transform.localScale =
                        new Vector3(-player.transform.localScale.x, 1, player.transform.localScale.z);
                    player.hitSide = 1;
                    break;
            }      
        }

        if (player.isLeftChange)
        {
            switch (hitSide)
            {
                case 1:
                    player.transform.position = new Vector3(player.blockCornerPos[3].x + 0.2f,
                        player.blockCornerPos[3].y - player.playerHeight / 2, 0);
                    player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    player.transform.localScale =
                        new Vector3(player.transform.localScale.x, -1, player.transform.localScale.z);
                    player.hitSide = 4;
                    break;
                case 2:
                    player.transform.position = new Vector3(player.blockCornerPos[0].x - player.playerHeight / 2,
                        player.blockCornerPos[0].y - 0.2f, 0f);
                    player.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                    player.hitSide = 1;
                    break;
                case 3:
                    player.transform.position = new Vector3(player.blockCornerPos[1].x - 0.2f,
                        player.blockCornerPos[1].y + player.playerHeight / 2, 0f);
                    player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    player.hitSide = 2;
                    break;
                case 4:
                    player.transform.position = new Vector3(player.blockCornerPos[2].x + player.playerHeight / 2,
                        player.blockCornerPos[2].y + 0.5f, 0f);
                    player.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                    player.transform.localScale =
                        new Vector3(player.transform.localScale.x, 1, player.transform.localScale.z);
                    player.hitSide = 3;
                    break;   
            }   
        }
    }
}