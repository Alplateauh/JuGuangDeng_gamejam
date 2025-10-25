using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_ChangeSideState : PlayerState
{
    public Player_ChangeSideState(Player player, PlayerFSM stateMachine, string animName) : base(player, stateMachine, animName)
    {
    }
    private bool isRotate;
    
    public override void Enter()
    {
        base.Enter();
        player.rb.velocity = Vector2.zero;
        player.lastRot = player.playerRot;
        isRotate = true;
    }

    public override void Update()
    {
        base.Update();
        
        if (isRotate)
        {
            TeleportPlayer(player.playerRot);
            HandleContinuousMove();
        }
        else
            player.stateMachine.ChangeState(player.wallMoveState);
    }
    
    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }
    
    public override void Exit()
    {
        base.Exit();
        player.rb.velocity = Vector2.zero;
        player.isRightChange = false;
        player.isLeftChange = false;
        player.hasEdgePos = false;
    }
    
    private void HandleContinuousMove()
    {
        switch (player.playerRot)
        {
            case 1:
                if (player.lastRot == 4 && player.isMove && player.isFacingRight) 
                {
                    player.continuousMoveDir = 1;
                    player.isContinuousMove = true;
                }
                break;
            case 2:
                break;
            case 3:
                if (player.lastRot == 4 && player.isMove && !player.isFacingRight) 
                {
                    player.continuousMoveDir = -1;
                    player.isContinuousMove = true;
                }
                break;
            case 4:
                if (player.lastRot == 3 && player.isMove && !player.isFacingRight) 
                {
                    player.continuousMoveDir = -1;
                    player.isContinuousMove = true;
                }
                else if (player.lastRot == 1 && player.isMove && player.isFacingRight)
                {
                    player.continuousMoveDir = 1;
                    player.isContinuousMove = true;
                }
                break;
        }
    }
    
    private void TeleportPlayer(int hitSide)
    {
        switch (hitSide)
        { 
            case 1: 
                if (player.isRightChange)
                {
                    player.transform.position = new Vector3(player.lastEdgePos.x + player.playerLength / 2,
                        player.lastEdgePos.y + player.playerHeight / 2, 0);
                    player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    player.playerRot = 2;
                    isRotate = false;
                }
                else if (player.isLeftChange)
                {
                    player.transform.position = new Vector3(player.lastEdgePos.x + player.playerLength / 2,
                        player.lastEdgePos.y - player.playerHeight / 2, 0);
                    player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    player.transform.localScale =
                        new Vector3(-player.transform.localScale.x, -1, player.transform.localScale.z);
                    player.playerRot = 4;
                    player.ChangeFacingRight();
                    isRotate = false;
                }
                break; 
            case 2: 
                if (player.isRightChange) 
                {
                    player.transform.position = new Vector3(player.lastEdgePos.x + player.playerHeight / 2,
                        player.lastEdgePos.y - player.playerLength / 2, 0f);
                    player.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                    player.playerRot = 3;
                    isRotate = false;
                }
                else if (player.isLeftChange)
                {
                    player.transform.position = new Vector3(player.lastEdgePos.x - player.playerHeight / 2,
                        player.lastEdgePos.y - player.playerLength / 2, 0f);
                    player.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                    player.playerRot = 1;
                    isRotate = false;
                }
                break; 
            case 3: 
                if (player.isRightChange)
                {
                    player.transform.position = new Vector3(player.lastEdgePos.x - player.playerLength / 2,
                        player.lastEdgePos.y - player.playerHeight / 2, 0f);
                    player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    player.transform.localScale =
                        new Vector3(-player.transform.localScale.x, -1, player.transform.localScale.z);
                    player.playerRot = 4;
                    player.ChangeFacingRight();
                    isRotate = false;
                }
                else if (player.isLeftChange) 
                {
                    player.transform.position = new Vector3(player.lastEdgePos.x - player.playerLength / 2,
                        player.lastEdgePos.y + player.playerHeight / 2, 0f);
                    player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    player.playerRot = 2;
                    isRotate = false;
                }
                break; 
            case 4: 
                if (player.isRightChange)
                {
                    player.transform.position = new Vector3(player.lastEdgePos.x - player.playerHeight / 2,
                        player.lastEdgePos.y + player.playerLength / 2, 0f);
                    player.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                    player.transform.localScale =
                        new Vector3(-player.transform.localScale.x, 1, player.transform.localScale.z);
                    player.ChangeFacingRight();
                    player.playerRot = 1;
                    isRotate = false;
                }
                else if (player.isLeftChange) 
                {
                    player.transform.position = new Vector3(player.lastEdgePos.x + player.playerHeight / 2,
                        player.lastEdgePos.y + player.playerLength / 2, 0f);
                    player.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                    player.transform.localScale =
                        new Vector3(-player.transform.localScale.x, 1, player.transform.localScale.z);
                    player.ChangeFacingRight();
                    player.playerRot = 3;
                    isRotate = false;
                }
                break;
        }      
    }
}