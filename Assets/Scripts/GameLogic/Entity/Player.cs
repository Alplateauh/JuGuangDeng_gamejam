using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
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

    #region �ؿ����

    [Header("�ؿ���ز���")]
    public float playerHeight;
    
    public Block block;
    public Vector2[] blockCornerPos;
    public Transform rotatePos;
    public Transform changePos;
    public bool hitBlock = false;
    public int hitSide; // ���������ײ����Block��һ�ߡ�0-�ޣ�1-��࣬2-�ϲ࣬3-�Ҳ࣬4-�²�

    public bool isWallMove = false;
    public bool isRightChange = false;
    public bool isLeftChange = false;

    #endregion

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
    // �����ǽ���ƶ��ϵ�״̬
    public Player_WallMoveState wallMoveState { get; private set; }
    // ���ת��Block�ıߵ�״̬
    public Player_ChangeSideState changeSideState { get; private set; }

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
        wallMoveState = new Player_WallMoveState(this, stateMachine, "wallMove");
        changeSideState = new Player_ChangeSideState(this, stateMachine, "changeSide");
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
        HandlePlayerRotation();
        HandleBlockSideChange();

        // ������־�������ǰ״̬
        Debug.Log(stateMachine.currentState);
    }

    private void FixedUpdate()
    {
        // �̶�ʱ����µ�ǰ״̬
        stateMachine.currentState.FixedUpdate();
    }

    // ���Player�Ƿ���ײ��Block
    // private void OnCollisionEnter2D(Collision2D blockColl)
    // {
    //     if ((1 << blockColl.gameObject.layer & blockLayerMask) != 0 && hitBlock == false) 
    //     {
    //         hitBlock = true;
    //         block = blockColl.gameObject.GetComponent<Block>();
    //     }
    // }

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
        transform.localScale = isFacingRight
            ? new Vector3(1f, transform.localScale.y, 1f)
            : new Vector3(-1f, transform.localScale.y, 1f);
    }

    private void FlipCheck()
    {
        if (((faceDir > 0 && !isFacingRight) || (faceDir < 0 && isFacingRight)) && canFlip)
        {
            HFlip();
        }
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
        // ��ȡ��ǰ�ƶ����򣨿�����ת��
        Vector2 moveDirection = GetCurrentMoveDirection();

        // ����Ŀ���ٶ�
        float targetSpeed = faceDir * runMaxSpeed;
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
            // 0�ȷ��� - �����ƶ�
            return Vector2.right;
        }
        else if (normalizedRotation >= 45 && normalizedRotation < 135)
        {
            // 90�ȷ��� - �����ƶ�
            return Vector2.up;
        }
        else if (normalizedRotation >= 135 && normalizedRotation < 225)
        {
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

        // ����Ŀ��Ƕ�
        float targetAngle = 0f;
        switch (number)
        {
            case 1: targetAngle = 90f; break;    // ���
            case 2: targetAngle = 0f; break;     // �ϲ�
            case 3: targetAngle = -90f; break;   // �Ҳ�
            case 4: targetAngle = 180f; break;   // �²�
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
}

public enum TimerType
{
    LastPressJump,
    JumpInterval,
    LastOnGround,
}