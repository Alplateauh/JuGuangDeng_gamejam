using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

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
    
    #region 转身相关参数
    
    [Header("转身相关参数")]
    [ReadOnly] public int faceDir;
    [ReadOnly] public bool canFlip;
    public bool isFacingRight { get; private set; } // 表示玩家当前是否面向右侧
    
    #endregion
    
    #region 跳跃相关参数

    [Header("跳跃相关参数")]
    [ReadOnly] public bool isJumping;
    [ReadOnly] public bool isJumpCut;
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
    
    #endregion
    
    #region 计时器
    
    private Dictionary<TimerType, float> timers = new Dictionary<TimerType, float>()
    {
        { TimerType.LastPressJump, 0 },
        { TimerType.JumpInterval, 0 },
        { TimerType.LastOnGround, 0 },
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

        // 调试日志，输出当前状态
        Debug.Log(stateMachine.currentState);
    }

    private void FixedUpdate()
    {
        // 固定时间更新当前状态
        stateMachine.currentState.FixedUpdate();
    }

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
        transform.localScale = isFacingRight ? new Vector3(1f, 1f, 1f) : new Vector3(-1f, 1f, 1f);   
    }

    private void FlipCheck()
    {
        if (((faceDir > 0 && !isFacingRight) || (faceDir < 0 && isFacingRight)) && canFlip)
        {
            HFlip();
        }
    }

    #endregion
    
    #region 玩家朝向方法

    /// <summary>
    /// 根据输入检查并更新玩家的朝向
    /// </summary>
    /// <param name="xInput">输入值 (-1 或 1)，用来决定朝向</param>
    public void CheckDirectionToFace(int xInput)
    {
        if (xInput == -1 && isFacingRight)
            Turn();
        if (xInput == 1 && !isFacingRight)
            Turn();
    }

    /// <summary>
    /// 翻转玩家的朝向（通过改变X轴的localScale）
    /// </summary>
    private void Turn()
    {
        // 通过修改localScale沿X轴翻转玩家
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        // 更新朝向状态
        isFacingRight = !isFacingRight;
    }

    #endregion
    
    #region 移动方法

    public void Run(float lerpAmount)
    {
        // 使用默认的最大速度
        Run(lerpAmount, movementData.runMaxSpeed);
    }

    public void Run(float lerpAmount, float runMaxSpeed)
    {
        // 计算目标速度
        float targetSpeed = faceDir * runMaxSpeed;
        targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed, lerpAmount);

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
        if (ShouldMaintainMomentum(targetSpeed))
        {
            accelRate = 0;
        }

        // 计算当前速度
        float currentVel = rb.velocity.x;
        float movement = (targetSpeed - currentVel) * accelRate;
        
        rb.AddForce(movement * Vector2.right , ForceMode2D.Force);
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
    private bool ShouldMaintainMomentum(float targetSpeed)
    {
        return Mathf.Abs(rb.velocity.x) > Mathf.Abs(targetSpeed) &&
               Mathf.Sign(rb.velocity.x) == Mathf.Sign(targetSpeed) &&
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
        // 如果没有跳跃输入，则直接返回
        if (GetTimer(TimerType.LastPressJump) <= 0)
            return;

        // 普通跳跃逻辑（在地面且未跳跃）
        if (CanJump())
        {
            isJumping = true;
            isJumpCut = false;
            SetTimer(TimerType.JumpInterval, 0.1f);
            return;
        }
    }

    /// <summary>
    /// 判断玩家此时是否可以跳跃
    /// </summary>
    /// <returns>如果玩家在地面，在土狼时间内，可以跳跃</returns>
    private bool CanJump()
    {
        return GetTimer(TimerType.LastOnGround) > 0 && !isJumping;
    }
    
    #endregion
}

public enum TimerType
{
    LastPressJump,
    JumpInterval,
    LastOnGround,
}