using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    #region ����

    public Animator animator { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public Collider2D collider { get; private set; }
    public Check check { get; private set; }

    #endregion

    [Header("�������")]
    public PlayerMovementData movementData;
    public float playerHeight;
    public float playerLength;
    public bool isGround = false;

    [Header("�����ƶ����")]
    public bool isMove = false;
    public bool isContinuousMove = false; // �Ƿ��������ƶ�״̬
    public int continuousMoveDir = 0; // �����ƶ��ķ���
        
    #region �ؿ����

    [Header("�ؿ���ز���")]
    public Transform rotatePos;
    public Vector2 blockHitPos;
    public Vector2 lastEdgePos;
    public bool hasHitPos;
    public bool hasEdgePos;
    public int lastRot; // �����Block����һ����
    public bool isHitBlock = false; // �Ƿ�����Block
    public bool isFirstHit = false; // �Ƿ�����Block
    public bool isWallMove = false; // �Ƿ����Block���ƶ���״̬
    public int playerRot; // ���������ײ����Block�ߵ���ת״̬��0-�ޣ�1-ͷ����࣬2-�ϲ࣬3-�Ҳ࣬4-�²�
    public bool isLeftRotate = false;
    public bool isRightRotate = false;
    public bool isLeftChange = false;
    public bool isRightChange = false;
    public bool isLeaving = false;

    #endregion

    #region ת����ز���

    [Header("ת����ز���")]
    public int faceDir;
    [HideInInspector] public bool canFlip;
    public bool isFacingRight { get; private set; } // ��ʾ��ҵ�ǰ�Ƿ������Ҳ�

    #endregion

    #region ��Ծ��ز���

    [Header("��Ծ��ز���")]
    public bool canJump;
    public bool isJumping;
    public bool isJumpCut;
    public bool isWallJumping;
    public ParticleSystem jumpParticle;

    #endregion

    #region ���봰��

    // ������Ϊfalse����InputHandle����������
    public bool jumpInputWindow { get; private set; }

    #endregion

    #region ״̬��

    public PlayerFSM stateMachine { get; private set; }
    // ��ҿ���״̬
    public Player_IdleState idleState { get; private set; }
    // ����ƶ�״̬
    //public Player_MoveState moveState { get; private set; }
    // �����Ծ״̬
    public Player_JumpState jumpState { get; private set; }
    // �������״̬
    public Player_FallState fallState { get; private set; }
    // �����ǽ���ƶ��ϵ�״̬
    public Player_WallMoveState wallMoveState { get; private set; }
    // �����ǽ������Ծ/�뿪��״̬
    public Player_WallJumpState wallJumpState { get; private set; }
    // �����ǽ�����뿪��״̬
    public Player_WallLeaveState wallLeaveState { get; private set; }
    // ���ת��Block�ıߵ�״̬
    public Player_ChangeRotation changeRotationState { get; private set; }
    public Player_ChangeSideState changeSideState { get; private set; }

    #endregion

    #region ��ʱ��

    private Dictionary<TimerType, float> timers = new Dictionary<TimerType, float>()
    {
        { TimerType.LastPressJump, 0 },
        { TimerType.LastPressLeave, 0 },
        { TimerType.LastPressWallJump, 0 },
        { TimerType.JumpInterval, 0 },
        { TimerType.LastOnGround, 0 },
        { TimerType.LeaveBlockCoolDown, 0 },
    };

    private List<TimerType> keys;

    #endregion

    private void Awake()
    {
        // ��ʼ��״̬��״̬
        stateMachine = new PlayerFSM();
        idleState = new Player_IdleState(this, stateMachine, "Idle");
        //moveState = new Player_MoveState(this, stateMachine, "Move");
        jumpState = new Player_JumpState(this, stateMachine, "Jump");
        fallState = new Player_FallState(this, stateMachine, "fall");
        wallMoveState = new Player_WallMoveState(this, stateMachine, "wallMove");
        wallJumpState = new Player_WallJumpState(this, stateMachine, "wallJump");
        wallLeaveState = new Player_WallLeaveState(this, stateMachine, "wallLeave");
        changeRotationState = new Player_ChangeRotation(this, stateMachine, "changeRotation");
        changeSideState = new Player_ChangeSideState(this, stateMachine, "changeSide");
    }

    private void Start()
    {
        // ��������
        animator = GetComponentInChildren<Animator>();
        collider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        check = GetComponent<Check>();

        // ��ʼ��״̬��
        stateMachine.Init(idleState);
        keys = new List<TimerType>(timers.Keys);
        jumpInputWindow = true;
        faceDir = 0;

        // ���ó�ʼ�����볯��
        SetPlayerGravityScale(movementData.gravityScale);
        isFacingRight = true;
        canFlip = true;
        
        canJump = true;
    }

    private void Update()
    {
        // ���µ�ǰ״̬
        stateMachine.currentState.Update();
        // ���µ�����ʱ��
        isGround = check.IsOnGround(); 
        
        UpdateTimers();
        FlipCheck();
        HandleJumpInput();
        HandleWallJumpInput();
        HandlePlayerLeaving();
        
        // ������־�������ǰ״̬
        Debug.Log(stateMachine.currentState);
        // Debug.Log(isFacingRight);
        //Debug.Log(GetTimer(TimerType.LeaveBlockCoolDown));
        //Debug.Log("PlayerRot: " + playerRot);
        Debug.Log("playerDir: " + faceDir + ", cDir: " + continuousMoveDir);
        Debug.Log(isFacingRight);
    }

    private void FixedUpdate()
    {
        // �̶�ʱ����µ�ǰ״̬
        stateMachine.currentState.FixedUpdate();
    }

    #region ���÷���

    /// <summary>
    /// ������ҵ�����
    /// </summary>
    /// <param name="gravityScale">����</param>
    public void SetPlayerGravityScale(float gravityScale)
    {
        if (TimeMgr.GetInstance().IsStop()) rb.gravityScale = gravityScale / Time.timeScale / Time.timeScale;
        else rb.gravityScale = gravityScale;
    }

    /// <summary>
    /// �򿪻��߹ر���Ծ���봰��
    /// </summary>
    /// <param name="isOpen"></param>
    public void OpenOrCloseJumpInputWindow(bool isOpen)
    {
        jumpInputWindow = isOpen;
    }

    /// <summary>
    /// ͳһ�ļ�ʱ�����·���
    /// </summary>
    private void UpdateTimers()
    {
        float deltaTime = TimeMgr.GetInstance().IsStop() ? Time.unscaledDeltaTime : Time.deltaTime;

        foreach (var key in keys)
        {
            timers[key] -= deltaTime;
        }
    }

    public float GetTimer(TimerType type) => timers[type];

    public void SetTimer(TimerType type, float value) => timers[type] = value;

    public void ResetTimer(TimerType type) => timers[type] = 0;

    #endregion

    #region ��ⷽ��

    private void HFlip()
    {
        ChangeFacingRight();
        faceDir = isFacingRight ? 1 : -1;
        transform.localScale = isFacingRight
            ? new Vector3(1f, transform.localScale.y, 1f)
            : new Vector3(-1f, transform.localScale.y, 1f);
    }

    private void FlipCheck()
    {
        if (isContinuousMove)
        {
            if (((continuousMoveDir > 0 && !isFacingRight) || (continuousMoveDir < 0 && isFacingRight)) && canFlip)
            {
                HFlip();
                isContinuousMove = false;
            }
        }
        else
        {
            if (((faceDir > 0 && !isFacingRight) || (faceDir < 0 && isFacingRight)) && canFlip)
            {
                HFlip();
            }
        }
    }

    public void ChangeFacingRight()
    {
        isFacingRight = !isFacingRight;
    }
    
    #endregion

    #region �ƶ�����

    public void Run(float lerpAmount)
    {
        // ʹ��Ĭ�ϵ�����ٶ�
        Run(lerpAmount, movementData.runMaxSpeed);
    }

    private void Run(float lerpAmount, float runMaxSpeed)
    {
        // ��ȡ��ǰ�ƶ�����
        Vector2 moveDirection = GetCurrentMoveDirection();

        float targetSpeed;
        // ����Ŀ���ٶ�
        if (isContinuousMove) 
            targetSpeed = continuousMoveDir * runMaxSpeed;
        else
            targetSpeed = faceDir * runMaxSpeed;
        
        targetSpeed = Mathf.Lerp(GetVelocityInMoveDirection(moveDirection), targetSpeed, lerpAmount);

        // ����Ŀ���ٶȼ��������
        float accelRate = CalculateAccelRate(targetSpeed);

        // ��Ծ����ʱ�����⴦��
        if ((isJumping || stateMachine.currentState == fallState) &&
            Mathf.Abs(rb.velocity.y) < movementData.jumpHangTimeThreshold)
        {
            accelRate *= movementData.jumpHangAccelerationMult;
            targetSpeed *= movementData.jumpHangMaxSpeedMult;
        }

        // ������ϱ��ֶ������������򲻽��м���
        if (ShouldMaintainMomentum(targetSpeed, moveDirection))
        {
            accelRate = 0;
        }

        // ���㵱ǰ�ٶ����ƶ������ϵķ���
        float currentVelInMoveDir = GetVelocityInMoveDirection(moveDirection);
        float movement = (targetSpeed - currentVelInMoveDir) * accelRate;

        // ����ȷ�ķ���ʩ����
        rb.AddForce(movement * moveDirection, ForceMode2D.Force);
    }

    /// <summary>
    /// ��ȡ��ǰ�ƶ����򣨿��ǽ�ɫ��ת��
    /// </summary>
    private Vector2 GetCurrentMoveDirection()
    {
        // ���ݽ�ɫ��ת�Ƕ�ȷ���ƶ�����
        float currentRotation = transform.eulerAngles.z;
    
        // ����ת�Ƕȱ�׼����0-360��
        float normalizedRotation = (currentRotation % 360 + 360) % 360;
    
        // ������ת�Ƕȷ��ض�Ӧ���ƶ�����
        if (normalizedRotation >= 315 || normalizedRotation < 45)
        {
            return Vector2.right; // 0�ȷ��� - �����ƶ�
        }
        else if (normalizedRotation >= 45 && normalizedRotation < 135)
        {
            // 90�ȷ��� - �����ƶ�
            return Vector2.up;
        }
        else if (normalizedRotation >= 135 && normalizedRotation < 225)
        {
            // ʵ��������ҿ�����ת�ĽǶ��ǽ���������ģ�����Ϊ�˶�������
            // 180�ȷ��� - �����ƶ�
            return Vector2.left;
        }
        else // 225-315��
        {
            // 270�ȷ��� - �����ƶ�
            return Vector2.down;
        }
    }
    
    /// <summary>
    /// ��ȡ��ǰ�ٶ���ָ�������ϵķ���
    /// </summary>
    private float GetVelocityInMoveDirection(Vector2 moveDirection)
    {
        return Vector2.Dot(rb.velocity, moveDirection);
    }

    /// <summary>
    /// ����Ŀ���ٶȺ����״̬������ٶ�
    /// </summary>
    private float CalculateAccelRate(float targetSpeed)
    {
        if (GetTimer(TimerType.LastOnGround) > 0)
        {
            return (Mathf.Abs(targetSpeed) > 0.01f) ? movementData.runAccelAmount : movementData.runDeccelAmount;
        }
        else
        {
            return (Mathf.Abs(targetSpeed) > 0.01f)
                ? movementData.runAccelAmount * movementData.accelInAir
                : movementData.runDeccelAmount * movementData.deccelInAir;
        }
    }

    /// <summary>
    /// �ж��Ƿ�Ӧ�ñ��ֶ���
    /// </summary>
    private bool ShouldMaintainMomentum(float targetSpeed, Vector2 moveDirection)
    {
        float currentVelInMoveDir = GetVelocityInMoveDirection(moveDirection);

        return Mathf.Abs(currentVelInMoveDir) > Mathf.Abs(targetSpeed) &&
               Mathf.Sign(currentVelInMoveDir) == Mathf.Sign(targetSpeed) &&
               Mathf.Abs(targetSpeed) > 0.01f &&
               GetTimer(TimerType.LastOnGround) < 0;
    }
    
    
    
    #endregion

    #region ��Ծ����

    /// <summary>
    /// ���������Ծ
    /// </summary>
    private void HandleJumpInput()
    {
        if (isHitBlock && (playerRot == 1 || playerRot == 3)) 
        {
            OpenOrCloseJumpInputWindow(false);
        }
        else
        {
            OpenOrCloseJumpInputWindow(true);
        }
        
        // ���û����Ծ���룬��ֱ�ӷ���
        if (GetTimer(TimerType.LastPressJump) <= 0)
            return;
        
        // ��ͨ��Ծ�߼����ڵ�����δ��Ծ��
        if (CanJump())
        {
            isJumping = true;
            isJumpCut = false;
            SetTimer(TimerType.JumpInterval, 0.1f);
        }
    }
    
    /// <summary>
    /// �ж���Ҵ�ʱ�Ƿ������Ծ
    /// </summary>
    /// <returns>�������ڵ��棬������ʱ���ڣ�������Ծ</returns>
    private bool CanJump()
    {
        return GetTimer(TimerType.LastOnGround) > 0 && !isJumping && canJump && (!isHitBlock || playerRot == 2);
    }

    private void HandleWallJumpInput()
    {
        if (GetTimer(TimerType.LastPressWallJump) <= 0)
            return;

        if (canWallJump())
        {
            isWallJumping = true;
        }
    }

    private bool canWallJump()
    {
        return canJump && isHitBlock && playerRot != 2;
    }
    
    #endregion

    #region �ؿ鷽��
    
    /// <summary>
    /// ��������ɫ����ת
    /// </summary>
    /// <param name="number">0,1,2,3�ֱ���������ײBlock��һ��Ӧ�ö�Ӧ����ת�Ƕȡ�
    /// 0-�� 1-���-90�㣬2-�ϲ�-0�㣬3-�Ҳ�- -90�ȣ�4-�²�-180��</param>
    public void PlayerRotate(int number)
    {
        if (rotatePos == null)
        {
            return;
        }

        if (number == 4)
        {
            transform.localScale = new Vector3(transform.localScale.x, -1f, transform.localScale.z);
            return;
        }
        
        // ����Ŀ��Ƕ�
        float targetAngle = 0f;
        switch (number)
        {
            case 1: targetAngle = 90f; break;    // ���
            case 2: targetAngle = 0f; break;     // �ϲ�
            case 3: targetAngle = -90f; break;   // �Ҳ�
            default: targetAngle = 0f; break;
        }

        // ����Ƕ�û�б仯��ֱ�ӷ���
        if (Mathf.Approximately(transform.eulerAngles.z, targetAngle))
            return;

        // ���㵱ǰ�Ƕ���Ŀ��ǶȵĲ�ֵ
        float currentAngle = transform.eulerAngles.z;
        float angleDif = targetAngle - currentAngle;
        while (angleDif > 180) angleDif -= 360;
        while (angleDif < -180) angleDif += 360;

        // ʹ��RotateAroundΧ��rotatePos��ת
        transform.RotateAround(rotatePos.position, Vector3.forward, angleDif);
    }

    public void ResolveOverlaps()
    {
        Collider2D playerCollider = collider; 
        if (playerCollider == null) return;

        // �ҵ������ص��ĵ��� colliders���� bounds ����һ����ɸѡ��
        Bounds b = playerCollider.bounds;
        Vector2 center = b.center;
        Vector2 size = b.size + Vector3.one * 0.02f; // small padding
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, transform.eulerAngles.z, LayerMask.GetMask("Ground"));

        Vector2 totalNudge = Vector2.zero;
        bool pushed = false;

        foreach (Collider2D other in hits)
        {
            if (other == playerCollider) continue;

            // ����ȷ���� ColliderDistance2D
            ColliderDistance2D dist = playerCollider.Distance(other);
            if (dist.isOverlapped)
            {
                // dist.normal ָ���������� other �ķ��뷽��
                float penetration = -dist.distance; // distance �Ǹ�ֵ��ʾ�ص����
                Vector2 nudge = dist.normal * penetration;

                // ����ÿ�������������ⵯ����Զ
                float maxNudge = 0.5f;
                if (nudge.magnitude > maxNudge) nudge = nudge.normalized * maxNudge;

                totalNudge += nudge;
                pushed = true;
                Debug.Log($"[ResolveOverlaps] overlap with {other.name}, nudge {nudge}");
            }
        }

        if (pushed)
        {
            Vector2 newPos = rb.position + totalNudge;
            rb.MovePosition(newPos);
            Debug.Log($"[ResolveOverlaps] applied total nudge {totalNudge} -> newPos {newPos}");
        }
    }
    
    private void HandlePlayerLeaving()
    {
        if (GetTimer(TimerType.LastPressLeave) <= 0)
            return;
        
        if (CanLeave())
        {
            isLeaving = true;
        }
    }

    private bool CanLeave()
    {
        return isHitBlock && !canJump && playerRot != 2;
    }
    
    public void ResetPlayerBlock()
    {
        isHitBlock = false;
        playerRot = 0;
    }

    #endregion
}

public enum TimerType
{
    LastPressJump,
    LastPressLeave,
    LastPressWallJump,
    JumpInterval,
    LastOnGround,
    LeaveBlockCoolDown,
}