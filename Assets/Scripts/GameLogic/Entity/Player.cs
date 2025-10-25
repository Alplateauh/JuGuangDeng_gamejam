using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class Player : MonoBehaviour
{
    #region 引用

    public Animator animator { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public Collider2D collider { get; private set; }
    public Check check { get; private set; }

    #endregion

    [Header("玩家数据")]
    public PlayerMovementData movementData;
    [ReadOnly] public bool isGround = false;

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

    #region 转身相关参数

    [Header("ת����ز���")]
    public int faceDir;
    [HideInInspector] public bool canFlip;
    public bool isFacingRight { get; private set; } // 表示玩家当前是否面向右侧

    #endregion

    #region 跳跃相关参数

    [Header("跳跃相关参数")]
    public bool canJump;
    public bool isJumping;
    public bool isJumpCut;
    public bool isWallJumping;
    public ParticleSystem jumpParticle;

    #endregion

    #region 输入窗口

    // 当变量为false，在InputHandle中屏蔽输入
    public bool jumpInputWindow { get; private set; }

    #endregion

    #region 状态机

    public PlayerFSM stateMachine { get; private set; }
    // 玩家空闲状态
    public Player_IdleState idleState { get; private set; }
    // ����ƶ�״̬
    //public Player_MoveState moveState { get; private set; }
    // �����Ծ״̬
    public Player_JumpState jumpState { get; private set; }
    // 玩家下落状态
    public Player_FallState fallState { get; private set; }
    // 玩家在墙壁移动上的状态
    public Player_WallMoveState wallMoveState { get; private set; }
    // �����ǽ������Ծ/�뿪��״̬
    public Player_WallJumpState wallJumpState { get; private set; }
    // �����ǽ�����뿪��״̬
    public Player_WallLeaveState wallLeaveState { get; private set; }
    // ���ת��Block�ıߵ�״̬
    public Player_ChangeRotation changeRotationState { get; private set; }
    public Player_ChangeSideState changeSideState { get; private set; }

    #endregion

    #region 计时器

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
        // 初始化状态机状态
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
        // 设置引用
        animator = GetComponentInChildren<Animator>();
        collider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        check = GetComponent<Check>();

        // 初始化状态机
        stateMachine.Init(idleState);
        keys = new List<TimerType>(timers.Keys);
        jumpInputWindow = true;
        faceDir = 0;

        // 设置初始重力与朝向
        SetPlayerGravityScale(movementData.gravityScale);
        isFacingRight = true;
        canFlip = true;
        
        canJump = true;
    }

    private void Update()
    {
        // 更新当前状态
        stateMachine.currentState.Update();
        // 更新地面检测时间
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
        // 固定时间更新当前状态
        stateMachine.currentState.FixedUpdate();
    }

    // 检测Player是否碰撞到Block
    // private void OnCollisionEnter2D(Collision2D blockColl)
    // {
    //     if ((1 << blockColl.gameObject.layer & blockLayerMask) != 0 && hitBlock == false) 
    //     {
    //         hitBlock = true;
    //         block = blockColl.gameObject.GetComponent<Block>();
    //     }
    // }

    #region 常用方法

    /// <summary>
    /// 设置玩家的重力
    /// </summary>
    /// <param name="gravityScale">重力</param>
    public void SetPlayerGravityScale(float gravityScale)
    {
        if (TimeMgr.GetInstance().IsStop()) rb.gravityScale = gravityScale / Time.timeScale / Time.timeScale;
        else rb.gravityScale = gravityScale;
    }

    /// <summary>
    /// 打开或者关闭跳跃输入窗口
    /// </summary>
    /// <param name="isOpen"></param>
    public void OpenOrCloseJumpInputWindow(bool isOpen)
    {
        jumpInputWindow = isOpen;
    }

    /// <summary>
    /// 统一的计时器更新方法
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

    #region 检测方法

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

    #region 移动方法

    public void Run(float lerpAmount)
    {
        // 使用默认的最大速度
        Run(lerpAmount, movementData.runMaxSpeed);
    }

    private void Run(float lerpAmount, float runMaxSpeed)
    {
        // 获取当前移动方向（考虑旋转）
        Vector2 moveDirection = GetCurrentMoveDirection();

        float targetSpeed;
        // ����Ŀ���ٶ�
        if (isContinuousMove) 
            targetSpeed = continuousMoveDir * runMaxSpeed;
        else
            targetSpeed = faceDir * runMaxSpeed;
        
        targetSpeed = Mathf.Lerp(GetVelocityInMoveDirection(moveDirection), targetSpeed, lerpAmount);

        // 根据目标速度计算加速率
        float accelRate = CalculateAccelRate(targetSpeed);

        // 跳跃顶点时的特殊处理
        if ((isJumping || stateMachine.currentState == fallState) &&
            Mathf.Abs(rb.velocity.y) < movementData.jumpHangTimeThreshold)
        {
            accelRate *= movementData.jumpHangAccelerationMult;
            targetSpeed *= movementData.jumpHangMaxSpeedMult;
        }

        // 如果符合保持动量的条件，则不进行加速
        if (ShouldMaintainMomentum(targetSpeed, moveDirection))
        {
            accelRate = 0;
        }

        // 计算当前速度在移动方向上的分量
        float currentVelInMoveDir = GetVelocityInMoveDirection(moveDirection);
        float movement = (targetSpeed - currentVelInMoveDir) * accelRate;

        // 沿正确的方向施加力
        rb.AddForce(movement * moveDirection, ForceMode2D.Force);
    }

    /// <summary>
    /// 获取当前移动方向（考虑角色旋转）
    /// </summary>
    private Vector2 GetCurrentMoveDirection()
    {
        // 根据角色旋转角度确定移动方向
        float currentRotation = transform.eulerAngles.z;
    
        // ����ת�Ƕȱ�׼����0-360��
        float normalizedRotation = (currentRotation % 360 + 360) % 360;
    
        // ������ת�Ƕȷ��ض�Ӧ���ƶ�����
        if (normalizedRotation >= 315 || normalizedRotation < 45)
        {
            // 0度方向 - 向右移动
            return Vector2.right;
        }
        else if (normalizedRotation >= 45 && normalizedRotation < 135)
        {
            // 90度方向 - 向上移动
            return Vector2.up;
        }
        else if (normalizedRotation >= 135 && normalizedRotation < 225)
        {
            // ʵ��������ҿ�����ת�ĽǶ��ǽ���������ģ�����Ϊ�˶�������
            // 180�ȷ��� - �����ƶ�
            return Vector2.left;
        }
        else // 225-315度
        {
            // 270度方向 - 向下移动
            return Vector2.down;
        }
    }

    /// <summary>
    /// 获取当前速度在指定方向上的分量
    /// </summary>
    private float GetVelocityInMoveDirection(Vector2 moveDirection)
    {
        return Vector2.Dot(rb.velocity, moveDirection);
    }

    /// <summary>
    /// 根据目标速度和玩家状态计算加速度
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
    /// 判断是否应该保持动量
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

    #region 跳跃方法

    /// <summary>
    /// 处理玩家跳跃
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
        
        // 如果没有跳跃输入，则直接返回
        if (GetTimer(TimerType.LastPressJump) <= 0)
            return;
        
        // 普通跳跃逻辑（在地面且未跳跃）
        if (CanJump())
        {
            isJumping = true;
            isJumpCut = false;
            SetTimer(TimerType.JumpInterval, 0.1f);
        }
    }
    
    /// <summary>
    /// 判断玩家此时是否可以跳跃
    /// </summary>
    /// <returns>如果玩家在地面，在土狼时间内，可以跳跃</returns>
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

    #region 地块方法

    /// <summary>
    /// 用来做角色的旋转
    /// </summary>
    /// <param name="number">0,1,2,3分别代表玩家碰撞Block哪一侧应该对应的旋转角度。
    /// 0-无 1-左侧-90°，2-上侧-0°，3-右侧- -90度，4-下侧-180°</param>
    public void PlayerRotate(int number)
    {
        if (rotatePos == null)
        {
            return;
        }

        // 计算目标角度
        float targetAngle = 0f;
        switch (number)
        {
            case 1: targetAngle = 90f; break;    // 左侧
            case 2: targetAngle = 0f; break;     // 上侧
            case 3: targetAngle = -90f; break;   // 右侧
            case 4: targetAngle = 180f; break;   // 下侧
            default: targetAngle = 0f; break;
        }

        // 如果角度没有变化，直接返回
        if (Mathf.Approximately(transform.eulerAngles.z, targetAngle))
            return;

        // 计算当前角度与目标角度的差值
        float currentAngle = transform.eulerAngles.z;
        float angleDif = targetAngle - currentAngle;
        while (angleDif > 180) angleDif -= 360;
        while (angleDif < -180) angleDif += 360;

        // 使用RotateAround围绕rotatePos旋转
        transform.RotateAround(rotatePos.position, Vector3.forward, angleDif);
    }

    private void HandlePlayerRotation()
    {
        if (hitBlock)
        {
            isWallMove = true;
        }
        else
        {
            isWallMove = false;
        }
    }

    private void HandleBlockSideChange()
    {
        if (block == null) 
            return;

        blockCornerPos = new Vector2[4];
        blockCornerPos = block.GetCornerPos();

        if (faceDir == 1) 
        {
            switch (hitSide)
            {
                case 1:
                    if (changePos.position.y > blockCornerPos[0].y)
                    {
                        isRightChange = true;
                    }
                    break;
                case 2:
                    if (changePos.position.x > blockCornerPos[1].x)
                    {
                        isRightChange = true;
                    }
                    break;
                case 3:
                    if (changePos.position.y < blockCornerPos[2].y)
                    {
                        isRightChange = true;
                    }
                    break;
                case 4:
                    if (changePos.position.x > blockCornerPos[2].x) 
                    {
                        isLeftChange = true;
                    }
                    break;
            }
        }
        else if (faceDir == -1)
        {
            switch (hitSide)
            {
                case 1:
                    if (changePos.position.y < blockCornerPos[3].y) 
                    {
                        isLeftChange = true;
                    }
                    break;
                case 2:
                    if (changePos.position.x < blockCornerPos[0].x)
                    {
                        isLeftChange = true;
                    }
                    break;
                case 3:
                    if (changePos.position.y > blockCornerPos[1].y) 
                    {
                        isLeftChange = true;
                    }
                    break;
                case 4:
                    if (changePos.position.x < blockCornerPos[3].x) 
                    {
                        isRightChange = true;
                    }
                    break;
            }
        }
    }

    #endregion

    #region �ؿ鷽��
    
    /// <summary>
    /// 用来做角色的旋转
    /// </summary>
    /// <param name="number">0,1,2,3分别代表玩家碰撞Block哪一侧应该对应的旋转角度。
    /// 0-无 1-左侧-90°，2-上侧-0°，3-右侧- -90度，4-下侧-180°</param>
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

        // 如果角度没有变化，直接返回
        if (Mathf.Approximately(transform.eulerAngles.z, targetAngle))
            return;

        // 计算当前角度与目标角度的差值
        float currentAngle = transform.eulerAngles.z;
        float angleDif = targetAngle - currentAngle;
        while (angleDif > 180) angleDif -= 360;
        while (angleDif < -180) angleDif += 360;

        // 使用RotateAround围绕rotatePos旋转
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