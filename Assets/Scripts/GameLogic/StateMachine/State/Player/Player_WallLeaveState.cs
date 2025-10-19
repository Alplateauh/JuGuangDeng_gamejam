using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Player_WallLeaveState : PlayerState
{
    public Player_WallLeaveState(Player player, PlayerFSM stateMachine, string animName) : base(player, stateMachine, animName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        player.SetTimer(TimerType.LeaveBlockCoolDown, movementData.leaveBlockCooldown);
    }

    public override void Update()
    {
        base.Update();

        if (player.hitSide == 1)
        {
            player.transform.position = new Vector3(player.transform.position.x - player.playerLength / 2,
                player.transform.position.y, player.transform.position.z);
            player.transform.rotation = Quaternion.Euler(0, 0, 0);
            player.stateMachine.ChangeState(player.fallState);
        }
        else if (player.hitSide == 3)
        {
            player.transform.position = new Vector3(player.transform.position.x + player.playerLength / 2,
                player.transform.position.y, player.transform.position.z);
            player.transform.rotation = Quaternion.Euler(0, 0, 0);
            player.stateMachine.ChangeState(player.fallState);
        }
        else if (player.hitSide == 4)
        {
            player.stateMachine.ChangeState(player.fallState);
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void Exit()
    {
        base.Exit();
        player.isLeaving = false;
        player.ResetPlayerBlock();
        player.SetTimer(TimerType.LeaveBlockCoolDown, player.movementData.leaveBlockCooldown);
    }
}
