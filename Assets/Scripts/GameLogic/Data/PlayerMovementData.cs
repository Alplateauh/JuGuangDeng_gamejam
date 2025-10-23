using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PlayerMovementData", menuName = "Data/Player Data")]
public class PlayerMovementData : ScriptableObject
{
    [Header("Move Info")]
    public float runMaxSpeed = 8;
    public float runAcceleration; //水平加速加速度
    [HideInInspector] public float runAccelAmount;
    public float runDecceleration; //水平减速加速度
    [HideInInspector] public float runDeccelAmount;
    [Range(0.01f, 1)] public float accelInAir; //在空中加速时水平加速度会乘该比值
    [Range(0.01f, 1)] public float deccelInAir; //在空中减速时水平加速度会乘该比值
    public float speedThreshold; //最小速度阈值
    public float adsorbeOutTime;
    public float leaveBlockCooldown;
    [Range(0.001f, 0.5f)] public float leaveInputBufferTime;
    
    [Space(20)]
    [Header("Jump Info")]
    public float jumpHeight; //跳跃高度
    public float wallJumpForce;
    public float jumpTimeToApex; //跳跃至最大高度的时间
    public int jumpCount = 2; //最多跳跃次数
    [Range(0f, 1f)] public float coyoteTime; //土狼时间
    [Range(0.01f, 0.5f)] public float jumpInputBufferTime; // 在满足跳跃条件前，输入跳跃的宽限时间。
    public float maxFallSpeed; //坠落最大速度
    [HideInInspector] public float jumpForce;
    
    [Space(20)]
    [Header("Both Jumps")]
    public float jumpCutGravityMult; //在跳跃中断（松开跳跃键）时重力会乘该比值
    [Range(0f, 1)] public float jumpHangGravityMult; //在跳跃调整时重力会乘该比值
    public float jumpHangTimeThreshold; //当玩家竖直速度小于该阈值时，给予玩家一个调整时间
    [Space(0.5f)]
    public float jumpHangAccelerationMult; //调整时间内玩家水平加速度会乘这个比值
    public float jumpHangMaxSpeedMult; //调整时间内玩家最大水平速度会乘这个比值
    public float fallGravityMult;
    public float gravityStrength;
    public float gravityScale;

    [Space(20)]
    [Header("BeDamageInfo")]
    public Vector2 beDamageForce;
    
    private void OnValidate()
    {
        // 使用公式计算重力强度 (gravity = 2 * jumpHeight / timeToJumpApex^2)
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);

        // 计算刚体的重力缩放（即：重力相对于Unity重力的比例，查看项目设置/Physics2D）
        gravityScale = gravityStrength / Physics2D.gravity.y;

        // 使用公式计算跑步加速和减速力：amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed
        runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
        runDeccelAmount = (50 * runDecceleration) / runMaxSpeed;

        // 使用公式计算跳跃力 (initialJumpVelocity = gravity * timeToJumpApex)
        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

        #region 变量范围限制
        runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
        runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, runMaxSpeed);
        #endregion
    }
}