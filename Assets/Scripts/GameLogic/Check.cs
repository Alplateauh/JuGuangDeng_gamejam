using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Check : MonoBehaviour
{
    private Player player;
    private PlayerMovementData data;
    
    // 在检查区设置这些
    [Header("地面检测")]
    [SerializeField] private Transform _groundCheckPoint;
    // 地面检查的大小通常比角色宽度（用于地面）和高度（用于墙壁检查）稍小
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]
    
    [Header("层和标签")]
    [SerializeField] private LayerMask _groundLayer;

    private void Start()
    {
        player = GetComponent<Player>();
        data = player.movementData;
    }

    private void Update()
    {
        if (!player.isJumping && data != null && player != null) 
        {
            // 地面检查
            if (IsOnGround() && !player.isJumping) // 检查设置的框是否与地面重叠
            {
                player.SetTimer(TimerType.LastOnGround, data.coyoteTime); // 如果重叠，设置最后的地面时间为 coyoteTime
            }
        }
    }

    public bool IsOnGround()
    {
        if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer))
        {
            return true;   
        }
        else
        {
            return false;
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
        // Gizmos.color = Color.blue;
        // Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
        // Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
    }
}
