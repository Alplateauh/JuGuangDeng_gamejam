using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
    [Header("�ؿ���")]
    [SerializeField] private Transform _frontBlockCheckPoint;
    [SerializeField] private Vector2 _frontBlockCheckSize;
    [SerializeField] private Transform _headBlockCheckPoint;
    [FormerlySerializedAs("_headtBlockCheckSize")] [SerializeField] private Vector2 _headBlockCheckSize;
    
    [Header("层和标签")]
    [SerializeField] private LayerMask _groundLayer;
    [Header("�ؿ�ͼ��")]
    [SerializeField] private LayerMask _blockLayer;

    // �洢��ǰ��ת�Ƕ�
    private float currentRotation;

    private void Start()
    {
        player = GetComponent<Player>();
        data = player.movementData;
        
        // ��ʼ����ת�Ƕ�
        currentRotation = transform.eulerAngles.z;
    }

    private void Update()
    {
        // ���µ�ǰ��ת�Ƕ�
        currentRotation = transform.eulerAngles.z;
        
        if (!player.isJumping && data != null && player != null) 
        {
            // 地面检查
            if (IsOnGround() && !player.isJumping) // 检查设置的框是否与地面重叠
            {
                player.SetTimer(TimerType.LastOnGround, data.coyoteTime); // 如果重叠，设置最后的地面时间为 coyoteTime
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
                // ��ҽӴ�Block���
                if (player.isFacingRight)
                {
                    player.hitSide = 1;
                }
                // ��ҽӴ�Block�Ҳ�
                else
                {
                    player.hitSide = 3;
                }   
            }
            // ���ͷ����Block�²�
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
        // ʹ����ת�ǶȽ��м��
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
        // ʹ����ת�ǶȽ��м��
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
        // ʹ����ת�ǶȽ��м��
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
        // ʹ����ת�ǶȽ��м��
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
        // ���浱ǰ����
        Matrix4x4 originalMatrix = Gizmos.matrix;
        
        // ���Ƶ��������򣨴���ת��
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(
            _groundCheckPoint.position, 
            Quaternion.Euler(0, 0, Application.isPlaying ? currentRotation : transform.eulerAngles.z), 
            Vector3.one
        );
        Gizmos.DrawWireCube(Vector3.zero, _groundCheckSize);
        
        // ����ǰ��Block������򣨴���ת��
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(
            _frontBlockCheckPoint.position, 
            Quaternion.Euler(0, 0, Application.isPlaying ? currentRotation : transform.eulerAngles.z), 
            Vector3.one
        );
        Gizmos.DrawWireCube(Vector3.zero, _frontBlockCheckSize);
        
        // �����Ϸ�Block������򣨴���ת��
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.TRS(
            _headBlockCheckPoint.position, 
            Quaternion.Euler(0, 0, Application.isPlaying ? currentRotation : transform.eulerAngles.z), 
            Vector3.one
        );
        Gizmos.DrawWireCube(Vector3.zero, _headBlockCheckSize);
        
        // �ָ�ԭʼ����
        Gizmos.matrix = originalMatrix;
    }
}