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
    public Check check { get; private set; }

    #endregion

    [Header("玩家数据")]
    public PlayerMovementData movementData;
    [ReadOnly] public bool isGround = false;

    #region 地块相关

    [Header("地块相关参数")]
    public float playerHeight;
    
    public Block block;
    public Vector2[] blockCornerPos;
    public Transform rotatePos;
    public Transform changePos;
    public bool hitBlock = false;
    public int hitSide; // 代表玩家碰撞到的Block那一边。0-无，1-左侧，2-上侧，3-右侧，4-下侧

    public bool isWallMove = false;
    public bool isRightChange = false;
    public bool isLeftChange = false;

    #endregion

    #region 转身相关参数

    [Header("转身相关参数")]
    [HideInInspector] public int faceDir;
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
    // 玩家移动状态
    public Player_MoveState moveState { get; private set; }
    // 玩家跳跃状态
    public Player_JumpState jumpState { get; private set; }
    // 玩家下落状态
    public Player_FallState fallState { get; private set; }
    // 玩家在墙壁移动上的状态
    public Player_WallMoveState wallMoveState { get; private set; }
    // 玩家转移Block的边的状态
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
        moveState = new Player_MoveState(this, stateMachine, "Move");
        jumpState = new Player_JumpState(this, stateMachine, "Jump");
        fallState = new Player_FallState(this, stateMachine, "fall");
        wallMoveState = new Player_WallMoveState(this, stateMachine, "wallMove");
        changeSideState = new Player_ChangeSideState(this, stateMachine, "changeSide");
    }

    private void Start()
    {
        // 设置引用
        animator = GetComponentInChildren<Animator>();
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
        HandlePlayerRotation();
        HandleBlockSideChange();

        // 调试日志，输出当前状态
        Debug.Log(stateMachine.currentState);
        // Debug.Log(isFacingRight);
        //Debug.Log(GetTimer(TimerType.LeaveBlockCoolDown));
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
        isFacingRight = !isFacingRight;
        faceDir = isFacingRight ? 1 : -1;
        transform.localScale = isFacingRight
            ? new Vector3(1f, transform.localScale.y, 1f)
            : new Vector3(-1f, transform.localScale.y, 1f);
    }

    private void FlipCheck()
    {
        if (((faceDir > 0 && !isFacingRight) || (faceDir < 0 && isFacingRight)) && canFlip)
        {
            HFlip();
            hasChanged = false;
        }
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

        // 计算目标速度
        float targetSpeed = faceDir * runMaxSpeed;
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

        // 将旋转角度标准化到0-360度
        float normalizedRotation = (currentRotation % 360 + 360) % 360;

        // 根据旋转角度返回对应的移动方向
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
            // 180度方向 - 向左移动
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
        if (isHitBlock && (hitSide == 1 || hitSide == 3)) 
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
        return GetTimer(TimerType.LastOnGround) > 0 && !isJumping && canJump && (!isHitBlock || hitSide == 2);
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
        return canJump && isHitBlock && hitSide != 2;
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
        if (isHitBlock)
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

        if (isFacingRight) 
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
        else
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
        return isHitBlock && !canJump && hitSide != 2;
    }
    
    public void ResetPlayerBlock()
    {
        isHitBlock = false;
        hitSide = 0;
        hasChanged = false;
        block = null;
        blockCornerPos = null;
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