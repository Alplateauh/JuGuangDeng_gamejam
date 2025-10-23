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
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    
    [Space(5)]
    [Header("Block检测")]
    [SerializeField] private Transform _frontBlockCheckPoint;
    [SerializeField] private Vector2 _frontBlockCheckSize;
    [SerializeField] private Transform _headBlockCheckPoint;
    [SerializeField] private Vector2 _headBlockCheckSize;
    
    [Header("BlockHit射线设置")]
    [SerializeField] private Transform _frontBlockHitCheckPoint; 
    [SerializeField] private float _frontBlockHitCheckPointLen;
    [SerializeField, Range(1, 15)] private int _frontHitRays = 5;
    [SerializeField] private float _frontHitVerticalSpan = 0.4f;
    
    [Header("地面探针检测")]
    [SerializeField] private Transform _probeCenterTransform;
    [SerializeField] private Transform _probeBehindTransform;
    [SerializeField] private Transform _probeFrontTransform;
    [SerializeField] private float _probeLen;
    private Vector2 lastProbeHitPoint;
    
    [Header("层和标签")]
    [SerializeField] private LayerMask _groundLayer;
    
    private float currentRotation;
    private bool front;
    private bool head;
    private bool bottom;

    private void Start()
    {
        player = GetComponent<Player>();
        data = player.movementData;
        currentRotation = transform.eulerAngles.z;
    }

    private void Update()
    {
        currentRotation = transform.eulerAngles.z;
        
        if (!player.isJumping && data != null && player != null) 
        {
            if (IsOnGround() && !player.isJumping)
            {
                player.SetTimer(TimerType.LastOnGround, data.coyoteTime);
            }
        }

        BlockCheck();
        BlockHitCheck();
        BlockEdgeCheck();
    }

    private void FixedUpdate()
    {
        
    }

    private void BlockHitCheck()
    {
        if (player == null || _frontBlockHitCheckPoint == null) return;
        
        Vector2 forward = _frontBlockHitCheckPoint.right;
        if (!player.isFacingRight) forward = -forward;
        
        Vector2 up = new Vector2(-forward.y, forward.x);

        // 保证至少 1 条射线
        int rayCount = Mathf.Max(1, _frontHitRays);
        
        float span = Mathf.Max(0f, _frontHitVerticalSpan);
        float closestDist = float.MaxValue;
        RaycastHit2D bestHit = default;
        bool found = false;

        for (int i = 0; i < rayCount; i++)
        {
            float t = (rayCount == 1) ? 0f : (float)i / (rayCount - 1);
            float offset = Mathf.Lerp(-span * 0.5f, span * 0.5f, t); 
            Vector2 origin = (Vector2)_frontBlockHitCheckPoint.position + up * offset;
            
            RaycastHit2D hit = Physics2D.Raycast(origin, forward, _frontBlockHitCheckPointLen, _groundLayer);
            
            if (hit.collider != null)
            {
                if (hit.distance < closestDist)
                {
                    closestDist = hit.distance;
                    bestHit = hit;
                    found = true;
                }
            }
        }

        if (found)
        {
            player.hasHitPos = true;
            player.blockHitPos = bestHit.point;
        }
        else
        {
            player.hasHitPos = false;
        }
    }
    
    private void BlockCheck()
    {
        if (player == null) return;

        front = FrontBlockCheck();
        head = HeadBlockCheck();
        bottom = BottomBlockCheck();
        
        player.isHitBlock = front || head || bottom;
        
        // 第一次碰撞到Block并吸附
        if ((front || head || bottom) && !player.isWallMove) 
        {
            HandleFirstBlockHit();
            return;
        }
        
        // 在Wallmove中碰到Block
        if ((front || head || bottom) && !player.isFirstHit && player.isGround && player.isWallMove &&
            !player.isRightRotate && !player.isLeftRotate) 
        {
            HandleWallMovement();
            return;
        }
        
        // 离开Block
        if (!front && !head && !bottom && player.isWallMove)
        {
            player.playerRot = 0;
            player.isWallMove = false;
            player.isLeftRotate = false;
            player.isRightRotate = false;
        }
    }

    private void BlockEdgeCheck()
    {
        if (player == null || _probeCenterTransform == null || player.playerRot == 0) return;
        
        Vector2 downDir = (-_probeCenterTransform.up).normalized * player.transform.localScale.y;

        RaycastHit2D hitCenter = Physics2D.Raycast(_probeCenterTransform.position, downDir, _probeLen, _groundLayer);
        bool hitC = hitCenter.collider != null;
        bool hitB = Physics2D.Raycast(_probeBehindTransform.position, downDir, _probeLen, _groundLayer);
        bool hitF = Physics2D.Raycast(_probeFrontTransform.position, downDir, _probeLen, _groundLayer);

        if (hitC)
        {
            lastProbeHitPoint = hitCenter.point;
        }

        if (hitB && !hitC && !hitF && !player.hasEdgePos)
        {
            player.lastEdgePos = lastProbeHitPoint;

            bool normalDirection = player.playerRot != 4;
            if (normalDirection)
            {
                if (player.isFacingRight) player.isRightChange = true;
                else player.isLeftChange = true;
            }
            else
            {
                if (player.isFacingRight) player.isLeftChange = true;
                else player.isRightChange = true;
            }

            player.hasEdgePos = true;
        }
    }
    
    private void HandleFirstBlockHit()
    {
        if (FrontBlockCheck())
        {
            player.playerRot = player.isFacingRight ? 1 : 3;
            player.isWallMove = true;
            player.isFirstHit = true;
        }
        else if (HeadBlockCheck())
        {
            player.playerRot = 4;
            player.isWallMove = true;
            player.isFirstHit = true;
        }
        else if (BottomBlockCheck())
        {
            player.playerRot = 2;
            player.isWallMove = true;
            player.isFirstHit = true;
        }
    }

    private void HandleWallMovement()
    {
        switch (player.playerRot)
        {
            case 1:
                if (FrontBlockCheck())
                {
                    if (player.isFacingRight)
                    {
                        player.playerRot = 4;   
                        player.isLeftRotate = true;
                    }
                    else
                    {
                        player.playerRot = 2;    
                        player.isRightRotate = true;
                    }
                }
                break;
            case 2:
                if (FrontBlockCheck())
                {
                    if (player.isFacingRight)
                    {
                        player.playerRot = 1;   
                        player.isLeftRotate = true;
                    }
                    else
                    {
                        player.playerRot = 3;    
                        player.isRightRotate = true;
                    }
                }
                break;
            case 3:
                if (FrontBlockCheck())
                {
                    if (player.isFacingRight)
                    {
                        player.playerRot = 2;   
                        player.isLeftRotate = true;
                    }
                    else
                    {
                         player.playerRot = 4;    
                        player.isRightRotate = true;
                    }
                }
                break;
            case 4:
                if (FrontBlockCheck())
                {
                    if (player.isFacingRight)
                    {
                        player.playerRot = 1;   
                        player.isRightRotate = true;
                    }
                    else
                    {
                        player.playerRot = 3;    
                        player.isLeftRotate = true;
                    }
                }
                break;
        }
    }

    public bool IsOnGround()
    {
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
        Collider2D collider = Physics2D.OverlapBox(
            _frontBlockCheckPoint.position, 
            _frontBlockCheckSize, 
            currentRotation,
            _groundLayer
        );
        return collider != null;
    }

    private bool HeadBlockCheck()
    {
        Collider2D collider = Physics2D.OverlapBox(
            _headBlockCheckPoint.position, 
            _headBlockCheckSize, 
            currentRotation,
            _groundLayer
        );
        return collider != null;
    }
    
    public bool BottomBlockCheck()
    {
        Collider2D collider = Physics2D.OverlapBox(
            _groundCheckPoint.position, 
            _headBlockCheckSize, 
            currentRotation, 
            _groundLayer
        );
        return collider != null;
    }
    
private void OnDrawGizmosSelected()
{
    // 不全局修改 Gizmos.matrix（避免影响后续世界坐标绘制）
    // 只在需要画旋转/缩放盒子时临时设置并立即恢复。

    // 画 ground 检测框（绿色）
    if (_groundCheckPoint != null)
    {
        Gizmos.color = Color.green;
        var rot = Quaternion.Euler(0, 0, Application.isPlaying ? currentRotation : transform.eulerAngles.z);
        Matrix4x4 m = Matrix4x4.TRS(_groundCheckPoint.position, rot, Vector3.one);
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = m;
        Gizmos.DrawWireCube(Vector3.zero, _groundCheckSize);
        Gizmos.matrix = old;
    }

    // 画 front 检测框（红色）
    if (_frontBlockCheckPoint != null)
    {
        Gizmos.color = Color.red;
        var rot = Quaternion.Euler(0, 0, Application.isPlaying ? currentRotation : transform.eulerAngles.z);
        Matrix4x4 m = Matrix4x4.TRS(_frontBlockCheckPoint.position, rot, Vector3.one);
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = m;
        Gizmos.DrawWireCube(Vector3.zero, _frontBlockCheckSize);
        Gizmos.matrix = old;
    }

    // 画 head 检测框（红色）
    if (_headBlockCheckPoint != null)
    {
        Gizmos.color = Color.red;
        var rot = Quaternion.Euler(0, 0, Application.isPlaying ? currentRotation : transform.eulerAngles.z);
        Matrix4x4 m = Matrix4x4.TRS(_headBlockCheckPoint.position, rot, Vector3.one);
        Matrix4x4 old = Gizmos.matrix;
        Gizmos.matrix = m;
        Gizmos.DrawWireCube(Vector3.zero, _headBlockCheckSize);
        Gizmos.matrix = old;
    }

    // 多射线可视化（严格使用 world-space，跟随 Transform）
    if (_frontBlockHitCheckPoint != null)
    {
        // 取Transform的世界方向：使用 transform.right / transform.up 保证跟随 Transform 的旋转
        Vector3 forwardWorld = _frontBlockHitCheckPoint.transform.right;
        // 若存在 player 且在运行时使用 player.isFacingRight 控制朝向
        if (Application.isPlaying && player != null)
        {
            if (!player.isFacingRight) forwardWorld = -forwardWorld;
        }
        // 编辑器下没有 player 时，仍用 Transform.right（方便在编辑时移动检查点）
        Vector3 upWorld = _frontBlockHitCheckPoint.transform.up;

        int rayCount = Mathf.Max(1, _frontHitRays);
        float span = Mathf.Max(0f, _frontHitVerticalSpan);
        float len = Mathf.Max(0.01f, _frontBlockHitCheckPointLen);
        
        for (int i = 0; i < rayCount; i++)
        {
            float t = (rayCount == 1) ? 0f : (float)i / (rayCount - 1); // 0..1
            float offset = Mathf.Lerp(-span * 0.5f, span * 0.5f, t);
            Vector3 origin = _frontBlockHitCheckPoint.position + upWorld * offset;
            Vector3 dir = forwardWorld.normalized;

            bool hit = false;
            if (Application.isPlaying)
            {
                RaycastHit2D h = Physics2D.Raycast(origin, dir, len, _groundLayer);
                hit = h.collider != null;
            }

            Gizmos.color = hit ? Color.green : Color.red;
            Gizmos.DrawLine(origin, origin + dir * len);
        }
    }
    
    if (_probeCenterTransform != null)
    {
        if (player == null) 
            return;
        
        Vector2 downDir = (-_probeCenterTransform.up).normalized * player.transform.localScale.y;
        
        Vector3 centerWorld = _probeCenterTransform.position;
        Vector3 behindWorld = _probeBehindTransform != null ? _probeBehindTransform.position : centerWorld;
        Vector3 frontWorld  = _probeFrontTransform  != null ? _probeFrontTransform.position  : centerWorld;

        float probeLen = Mathf.Max(0.01f, _probeLen);
        
        bool hitC = false, hitB = false, hitF = false;
        if (Application.isPlaying)
        {
            hitC = Physics2D.Raycast(centerWorld, downDir, probeLen, _groundLayer);
            hitB = Physics2D.Raycast(behindWorld, downDir, probeLen, _groundLayer);
            hitF = Physics2D.Raycast(frontWorld,  downDir, probeLen, _groundLayer);
        }
        
        Color colC = Application.isPlaying ? (hitC ? Color.green : Color.red) : Color.yellow;
        Color colB = Application.isPlaying ? (hitB ? Color.green : Color.red) : Color.yellow;
        Color colF = Application.isPlaying ? (hitF ? Color.green : Color.red) : Color.yellow;
        
        Gizmos.color = colB;
        Gizmos.DrawLine(behindWorld, behindWorld + (Vector3)downDir * probeLen);
        Gizmos.color = colC;
        Gizmos.DrawLine(centerWorld, centerWorld + (Vector3)downDir * probeLen);
        Gizmos.color = colF;
        Gizmos.DrawLine(frontWorld, frontWorld + (Vector3)downDir * probeLen);
}
}
}