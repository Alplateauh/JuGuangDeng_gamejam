using System.Data.Common;
using UnityEngine;

/// <summary>
/// 负责处理玩家相关的输入
/// </summary>
public class Player_InputHandle : MonoBehaviour
{
    private Player player;
    private int currentWeapon;

    private void Awake()
    {
        AddInput();
        player = GetComponent<Player>();
    }
    private void AddInput()
    {
        InputMgr.GetInstance().AddKeyCode(KeyType.MOVE_PERFORMED, OnMovePerformed);
        InputMgr.GetInstance().AddKeyCode(KeyType.MOVE_CANCEL, OnMoveCanceled);
        InputMgr.GetInstance().AddKeyCode(KeyType.JUMP_START, OnJumpInput);
        InputMgr.GetInstance().AddKeyCode(KeyType.JUMP_CANCEL, OnJumpCanceled);
    }

    private void RemoveInput()
    {
        InputMgr.GetInstance().RemoveKeyCode(KeyType.MOVE_PERFORMED, OnMovePerformed);
        InputMgr.GetInstance().RemoveKeyCode(KeyType.MOVE_CANCEL, OnMoveCanceled);
        InputMgr.GetInstance().RemoveKeyCode(KeyType.JUMP_START, OnJumpInput);
        InputMgr.GetInstance().RemoveKeyCode(KeyType.JUMP_CANCEL, OnJumpCanceled);
    }

    private void OnMovePerformed()
    {
        // 读取当前输入向量，只调用一次
        Vector2 input = InputMgr.GetInstance().control.Player.Move.ReadValue<Vector2>();
        
        // 水平方向判断（A/D键优先）
        if (Input.GetKeyDown(KeyCode.D))
            player.faceDir = 1;
        else if (Input.GetKeyDown(KeyCode.A))
            player.faceDir = -1;
        else
            player.faceDir = input.x > 0 ? 1 : input.x < 0 ? -1 : 0;
        
        // 状态切换
        // if (player.stateMachine.currentState == player.idleState && player.faceDir != 0)
        // {
        //     player.stateMachine.ChangeState(player.moveState);
        // }
    }
    
    private void OnJumpInput()
    {
        if (player.jumpInputWindow)
        {
            player.SetTimer(TimerType.LastPressJump, player.movementData.jumpInputBufferTime);
        }
        player.SetTimer(TimerType.LastPressLeave, player.movementData.leaveInputBufferTime);
        player.SetTimer(TimerType.LastPressWallJump, player.movementData.leaveInputBufferTime);
    }
    
    private void OnJumpCanceled()
    {
        if (player.isJumping && player.rb.velocity.y > 0) 
            player.isJumpCut = true;
    }
    
    private void OnMoveCanceled()
    {
        // 水平取消逻辑
        if (Input.GetKey(KeyCode.A) && Input.GetAxis("Horizontal") > 0)
            player.faceDir = -1;
        else if (Input.GetKey(KeyCode.D) && Input.GetAxis("Horizontal") < 0)
            player.faceDir = 1;
        else
            player.faceDir = 0;
    }

    private void OnDestroy()
    {
        RemoveInput();
    }
}