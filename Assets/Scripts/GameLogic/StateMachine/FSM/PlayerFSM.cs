using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFSM
{
    public PlayerState currentState { get; private set; }

    /// <summary>
    /// 生成有限状态机
    /// </summary>
    /// <param name="state">初始状态</param>
    public void Init(PlayerState state)
    {
        currentState = state;
        currentState.Enter();
    }
    
    /// <summary>
    /// 切换当前状态
    /// </summary>
    /// <param name="newState">欲切换状态</param>
    public void ChangeState(PlayerState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public PlayerState CheckState()
    {
        return currentState;
    }
}
