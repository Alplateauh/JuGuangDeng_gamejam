using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_ChangeRotation : PlayerState
{
    public Player_ChangeRotation(Player player, PlayerFSM stateMachine, string animName) : base(player, stateMachine, animName)
    {
    }

    private bool isRotate;
    
    public override void Enter()
    {
        base.Enter();
        isRotate = true;
    }

    public override void Update()
    {
        base.Update();
        
        if (isRotate && player.hasHitPos) 
            HandlePlayerRotation();
        else
            player.stateMachine.ChangeState(player.wallMoveState);

        if (!player.isGround) 
            player.stateMachine.ChangeState(player.fallState);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void Exit()
    {
        base.Exit();
        player.isLeftRotate = false;
        player.isRightRotate = false;
        player.hasHitPos = false;
        player.blockHitPos = new Vector2(0f, 0f);
    }
    
    private void HandlePlayerRotation()
    {
        if (player.isLeftRotate)
        {
            switch (player.playerRot)
            {
                case 1:
                    player.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                    player.transform.position = new Vector3(player.blockHitPos.x - player.playerHeight / 2,
                        player.blockHitPos.y + player.playerLength / 2, 0);
                    isRotate = false;
                    break;
                case 2:
                    player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    player.transform.position = new Vector3(player.blockHitPos.x + player.playerLength / 2,
                        player.blockHitPos.y + player.playerHeight / 2, 0);
                    isRotate = false;
                    break;
                case 3:
                    player.transform.localScale = new Vector3(player.transform.localScale.x,
                        1f, player.transform.localScale.z);
                    player.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                    player.transform.position = new Vector3(player.blockHitPos.x + player.playerHeight / 2,
                        player.blockHitPos.y - player.playerLength / 2, 0);
                    isRotate = false;
                    break;
                case 4:
                    player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    player.transform.localScale = new Vector3(player.transform.localScale.x,
                        -1f, player.transform.localScale.z);
                    player.transform.position = new Vector3(player.blockHitPos.x - player.playerLength / 2,
                        player.blockHitPos.y - player.playerHeight / 2, 0);
                    isRotate = false;
                    break;
            }     
        }
        else if (player.isRightRotate)
        {
            switch (player.playerRot)
            {
                case 1:
                    player.transform.localScale =
                        new Vector3(player.transform.localScale.x, 1f, player.transform.localScale.z);
                    player.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                    player.transform.position = new Vector3(player.blockHitPos.x - player.playerHeight / 2,
                        player.blockHitPos.y - player.playerLength / 2, 0);
                    isRotate = false;
                    break;
                case 2:
                    player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    player.transform.position = new Vector3(player.blockHitPos.x - player.playerLength / 2,
                        player.blockHitPos.y + player.playerHeight / 2, 0);
                    isRotate = false;
                    break;
                case 3:
                    player.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                    player.transform.position = new Vector3(player.blockHitPos.x + player.playerHeight / 2,
                        player.blockHitPos.y + player.playerLength / 2, 0);
                    isRotate = false;
                    break;
                case 4:
                    player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                    player.transform.localScale = new Vector3(player.transform.localScale.x,
                        -1f, player.transform.localScale.z);
                    player.transform.position = new Vector3(player.blockHitPos.x + player.playerLength / 2,
                        player.blockHitPos.y - player.playerHeight / 2, 0);
                    isRotate = false;
                    break;
            }   
        }
    }
}
