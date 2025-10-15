using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFSM
{
    public PlayerState currentState { get; private set; }

    /// <summary>
    /// ��������״̬��
    /// </summary>
    /// <param name="state">��ʼ״̬</param>
    public void Init(PlayerState state)
    {
        currentState = state;
        currentState.Enter();
    }
    
    /// <summary>
    /// �л���ǰ״̬
    /// </summary>
    /// <param name="newState">���л�״̬</param>
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
