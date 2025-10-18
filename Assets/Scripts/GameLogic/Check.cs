using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Check : MonoBehaviour
{
    private Player player;
    private PlayerMovementData data;
    
    [Header("地面检测")]
    [SerializeField] private Transform _groundCheckPoint;
    [SerializeField] private Vector2 _groundCheckSize;
    
    [Space(5)]
    [Header("地块检测")]
    [SerializeField] private Transform _frontBlockCheckPoint;
    [SerializeField] private Vector2 _frontBlockCheckSize;
    [SerializeField] private Transform _headBlockCheckPoint;
    [FormerlySerializedAs("_headtBlockCheckSize")] [SerializeField] private Vector2 _headBlockCheckSize;
    
    [Space(5)]
    [Header("地面图层")]
    [SerializeField] private LayerMask _groundLayer;
    [Header("地块图层")]
    [SerializeField] private LayerMask _blockLayer;

    // 存储当前旋转角度
    private float currentRotation;

    private void Start()
    {
        player = GetComponent<Player>();
        data = player.movementData;
        
        // 初始化旋转角度
        currentRotation = transform.eulerAngles.z;
    }

    private void Update()
    {
        // 更新当前旋转角度
        currentRotation = transform.eulerAngles.z;
        
        if (!player.isJumping && data != null && player != null) 
        {
            // 地面检查
            if (IsOnGround() && !player.isJumping)
            {
                player.SetTimer(TimerType.LastOnGround, data.coyoteTime);
            }
        }

        BlockCheck();
    }

    private void BlockCheck()
    {
        if (player.hitBlock) 
        {
            if (FrontBlockCheck())
            {
                // 玩家接触Block左侧
                if (player.isFacingRight)
                {
                    player.hitSide = 1;
                }
                // 玩家接触Block右侧
                else
                {
                    player.hitSide = 3;
                }   
            }
            // 玩家头顶到Block下侧
            else if(HeadBlockCheck())
            {
                player.hitSide = 4;
            }
            else if(BottomBlockCheck() && player.hitSide == 0)
            {
                player.hitSide = 2;
            }
        }
        else
        {
            player.hitSide = 0;
        }
    }
    
    public bool IsOnGround()
    {
        // 使用旋转角度进行检测
        Collider2D collider = Physics2D.OverlapBox(
            _groundCheckPoint.position, 
            _groundCheckSize, 
            currentRotation, 
            _groundLayer
        );
        
        return collider != null;
    }

    private bool FrontBlockCheck()
    {
        // 使用旋转角度进行检测
        Collider2D collider = Physics2D.OverlapBox(
            _frontBlockCheckPoint.position, 
            _frontBlockCheckSize, 
            currentRotation,
            _blockLayer
        );
        
        return collider != null;
    }

    private bool HeadBlockCheck()
    {
        // 使用旋转角度进行检测
        Collider2D collider = Physics2D.OverlapBox(
            _headBlockCheckPoint.position, 
            _headBlockCheckSize, 
            currentRotation,
            _blockLayer
        );
        
        return collider != null;
    }
    
    public bool BottomBlockCheck()
    {
        // 使用旋转角度进行检测
        Collider2D collider = Physics2D.OverlapBox(
            _groundCheckPoint.position, 
            _headBlockCheckSize, 
            currentRotation, 
            _blockLayer
        );
        
        return collider != null;
    }
    
    private void OnDrawGizmosSelected()
    {
        // 保存当前矩阵
        Matrix4x4 originalMatrix = Gizmos.matrix;
        
        // 绘制地面检测区域（带旋转）
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(
            _groundCheckPoint.position, 
            Quaternion.Euler(0, 0, Application.isPlaying ? currentRotation : transform.eulerAngles.z), 
            Vector3.one
        );
        Gizmos.DrawWireCube(Vector3.zero, _groundCheckSize);
        
        // 绘制前方Block检测区域（带旋转）
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(
            _frontBlockCheckPoint.position, 
            Quaternion.Euler(0, 0, Application.isPlaying ? currentRotation : transform.eulerAngles.z), 
            Vector3.one
        );
        Gizmos.DrawWireCube(Vector3.zero, _frontBlockCheckSize);
        
        // 绘制上方Block检测区域（带旋转）
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(
            _headBlockCheckPoint.position, 
            Quaternion.Euler(0, 0, Application.isPlaying ? currentRotation : transform.eulerAngles.z), 
            Vector3.one
        );
        Gizmos.DrawWireCube(Vector3.zero, _headBlockCheckSize);
        
        // 恢复原始矩阵
        Gizmos.matrix = originalMatrix;
    }
}