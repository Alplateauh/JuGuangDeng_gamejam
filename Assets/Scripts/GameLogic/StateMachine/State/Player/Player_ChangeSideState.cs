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
        lastHitSide = player.playerRot;
        player.rb.velocity = Vector2.zero;
        //player.hasChanged = true;
        
        player.rb.velocity = Vector2.zero;
        TeleportPlayer(player.playerRot);
    }

    public override void Update()
    {
        base.Update();
        
        stateMachine.ChangeState(player.idleState);
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
        player.hasEdgePos = false;
        player.lastEdgePos = Vector2.zero;
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
                    }
                    else if (player.isLeftChange)
                    {
                        player.transform.position = new Vector3(player.lastEdgePos.x + player.playerLength / 2,
                            player.lastEdgePos.y - player.playerHeight / 2, 0);
                        player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                        player.transform.localScale =
                            new Vector3(player.transform.localScale.x, -1, player.transform.localScale.z);
                        player.rb.AddForce(Vector2.up * 100f, ForceMode2D.Impulse);
                        player.playerRot = 4;
                    }
                    break;
                case 2:
                    if (player.isRightChange)
                    {
                        player.transform.position = new Vector3(player.lastEdgePos.x + player.playerHeight / 2,
                            player.lastEdgePos.y - player.playerLength / 2, 0f);
                        player.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                        player.rb.AddForce(Vector2.left * 100f, ForceMode2D.Impulse);
                        player.playerRot = 3;
                    }
                    else if (player.isLeftChange)
                    {
                        player.transform.position = new Vector3(player.lastEdgePos.x - player.playerHeight / 2,
                            player.lastEdgePos.y - player.playerLength / 2, 0f);
                        player.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                        player.rb.AddForce(Vector2.right * 100f, ForceMode2D.Impulse);
                        player.playerRot = 1;
                    }
                    break;
                case 3:
                    if (player.isRightChange)
                    {
                        player.transform.position = new Vector3(player.lastEdgePos.x - player.playerLength / 2,
                            player.lastEdgePos.y - player.playerHeight / 2, 0f);
                        player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                        player.transform.localScale =
                            new Vector3(player.transform.localScale.x, -1, player.transform.localScale.z);
                        player.rb.AddForce(Vector2.up * 100f, ForceMode2D.Impulse);
                        player.playerRot = 4;
                    }
                    else if (player.isLeftChange) 
                    {
                        player.transform.position = new Vector3(player.lastEdgePos.x - player.playerLength / 2,
                            player.lastEdgePos.y + player.playerHeight / 2, 0f);
                        player.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
                        player.playerRot = 2;
                    }

                    break;
                case 4:
                    if (player.isRightChange)
                    {
                        player.transform.position = new Vector3(player.lastEdgePos.x - player.playerHeight / 2,
                            player.lastEdgePos.y + player.playerLength / 2, 0f);
                        player.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
                        player.transform.localScale =
                            new Vector3(player.transform.localScale.x, 1, player.transform.localScale.z);
                        player.rb.AddForce(Vector2.right * 100f, ForceMode2D.Impulse);
                        player.playerRot = 1;
                    }
                    else if (player.isLeftChange) 
                    {
                        player.transform.position = new Vector3(player.lastEdgePos.x + player.playerHeight / 2,
                            player.lastEdgePos.y + player.playerLength / 2, 0f);
                        player.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
                        player.transform.localScale =
                            new Vector3(player.transform.localScale.x, 1, player.transform.localScale.z);
                        player.rb.AddForce(Vector2.left * 100f, ForceMode2D.Impulse);
                        player.playerRot = 3;
                    }
                    break;
        }      
    }
}