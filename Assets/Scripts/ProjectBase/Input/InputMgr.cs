using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 1.Input类
/// 2.事件中心模块
/// 3.公共Mono模块的使用
/// </summary>
public class InputMgr : BaseManager<InputMgr>
{
    public PlayerControls control;
    private Dictionary<KeyType, Action<UnityEngine.InputSystem.InputAction.CallbackContext>> actionMap = new Dictionary<KeyType, Action<UnityEngine.InputSystem.InputAction.CallbackContext>>();
    private bool isStart = false;
    /// <summary>
    /// 构造函数中 初始化InputSystem
    /// </summary>
    public InputMgr()
    {
        control = new PlayerControls();
        control.Enable();
    }

    /// <summary>
    /// 是否开启或关闭 我的输入检测
    /// </summary>
    public void StartOrEndCheck(bool isOpen)
    {
        isStart = isOpen;
        if (isOpen) control.Player.Enable();
        else control.Player.Disable();
    }

    public void AddKeyCode(KeyType key, UnityAction action)
    {
        if (actionMap.ContainsKey(key))
        {
            Debug.LogWarning($"Key {key} 已经绑定了事件");
            return;
        }
        //Debug.Log(key + "绑定了事件");
        actionMap[key] = _ => action.Invoke();

        switch (key)
        {
            case KeyType.MOVE_PERFORMED:
                control.Player.Move.performed += actionMap[key];
                break;
            case KeyType.MOVE_CANCEL:
                control.Player.Move.canceled += actionMap[key];
                break;
            case KeyType.JUMP_START:
                control.Player.Jump.started += actionMap[key];
                break;
            case KeyType.JUMP_CANCEL:
                control.Player.Jump.canceled += actionMap[key];
                break;
        }
    }
    public void RemoveKeyCode(KeyType key, UnityAction action)
    {
        if (!actionMap.ContainsKey(key))
        {
            Debug.LogWarning($"Key {key} 没有绑定事件");
            return;
        }
        switch (key)
        {
            case KeyType.MOVE_PERFORMED:
                control.Player.Move.performed -= actionMap[key];
                break;
            case KeyType.MOVE_CANCEL:
                control.Player.Move.canceled -= actionMap[key];
                break;
            case KeyType.JUMP_START:
                control.Player.Jump.started -= actionMap[key];
                break;
            case KeyType.JUMP_CANCEL:
                control.Player.Jump.canceled -= actionMap[key];
                break;
        }
        actionMap.Remove(key);
        //Debug.Log(key + "移除了事件");
    }
}
public enum KeyType
{
    MOVE_PERFORMED,
    MOVE_CANCEL,
    JUMP_START,
    JUMP_CANCEL,
}