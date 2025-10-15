using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region ����
    
    public Animator animator { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public Check check { get; private set; }
    
    #endregion
    
    [Header("�������")]
    public PlayerMovementData movementData;
    [ReadOnly] public bool isGround = false;
    
    #region ת����ز���
    
    [Header("ת����ز���")]
    [ReadOnly] public int faceDir;
    [ReadOnly] public bool canFlip;
    public bool isFacingRight { get; private set; } // ��ʾ��ҵ�ǰ�Ƿ������Ҳ�
    
    #endregion
    
    #region ��Ծ��ز���

    [Header("��Ծ��ز���")]
    [ReadOnly] public bool isJumping;
    [ReadOnly] public bool isJumpCut;
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
    public Player_MoveState moveState { get; private set; } 
    // �����Ծ״̬
    public Player_JumpState jumpState { get; private set; } 
    // �������״̬
    public Player_FallState fallState { get; private set; }
    
    #endregion
    
    #region ��ʱ��
    
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
        // ��ʼ��״̬��״̬
        stateMachine = new PlayerFSM();
        idleState = new Player_IdleState(this, stateMachine, "Idle");
        moveState = new Player_MoveState(this, stateMachine, "Move");
        jumpState = new Player_JumpState(this, stateMachine, "Jump");
        fallState = new Player_FallState(this, stateMachine, "fall");
    }

    private void Start()
    {
        // ��������
        animator = GetComponentInChildren<Animator>();
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

        // ������־�������ǰ״̬
        Debug.Log(stateMachine.currentState);
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
    
    #region ��ҳ��򷽷�

    /// <summary>
    /// ���������鲢������ҵĳ���
    /// </summary>
    /// <param name="xInput">����ֵ (-1 �� 1)��������������</param>
    public void CheckDirectionToFace(int xInput)
    {
        if (xInput == -1 && isFacingRight)
            Turn();
        if (xInput == 1 && !isFacingRight)
            Turn();
    }

    /// <summary>
    /// ��ת��ҵĳ���ͨ���ı�X���localScale��
    /// </summary>
    private void Turn()
    {
        // ͨ���޸�localScale��X�ᷭת���
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        // ���³���״̬
        isFacingRight = !isFacingRight;
    }

    #endregion
    
    #region �ƶ�����

    public void Run(float lerpAmount)
    {
        // ʹ��Ĭ�ϵ�����ٶ�
        Run(lerpAmount, movementData.runMaxSpeed);
    }

    public void Run(float lerpAmount, float runMaxSpeed)
    {
        // ����Ŀ���ٶ�
        float targetSpeed = faceDir * runMaxSpeed;
        targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed, lerpAmount);

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
        if (ShouldMaintainMomentum(targetSpeed))
        {
            accelRate = 0;
        }

        // ���㵱ǰ�ٶ�
        float currentVel = rb.velocity.x;
        float movement = (targetSpeed - currentVel) * accelRate;
        
        rb.AddForce(movement * Vector2.right , ForceMode2D.Force);
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
    private bool ShouldMaintainMomentum(float targetSpeed)
    {
        return Mathf.Abs(rb.velocity.x) > Mathf.Abs(targetSpeed) &&
               Mathf.Sign(rb.velocity.x) == Mathf.Sign(targetSpeed) &&
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
        // ���û����Ծ���룬��ֱ�ӷ���
        if (GetTimer(TimerType.LastPressJump) <= 0)
            return;

        // ��ͨ��Ծ�߼����ڵ�����δ��Ծ��
        if (CanJump())
        {
            isJumping = true;
            isJumpCut = false;
            SetTimer(TimerType.JumpInterval, 0.1f);
            return;
        }
    }

    /// <summary>
    /// �ж���Ҵ�ʱ�Ƿ������Ծ
    /// </summary>
    /// <returns>�������ڵ��棬������ʱ���ڣ�������Ծ</returns>
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