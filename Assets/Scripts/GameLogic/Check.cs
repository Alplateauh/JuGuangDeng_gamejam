using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Check : MonoBehaviour
{
    private Player player;
    private PlayerMovementData data;
    
    // �ڼ����������Щ
    [Header("������")]
    [SerializeField] private Transform _groundCheckPoint;
    // ������Ĵ�Сͨ���Ƚ�ɫ��ȣ����ڵ��棩�͸߶ȣ�����ǽ�ڼ�飩��С
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]
    
    [Header("��ͱ�ǩ")]
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
            // ������
            if (IsOnGround() && !player.isJumping) // ������õĿ��Ƿ�������ص�
            {
                player.SetTimer(TimerType.LastOnGround, data.coyoteTime); // ����ص����������ĵ���ʱ��Ϊ coyoteTime
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
