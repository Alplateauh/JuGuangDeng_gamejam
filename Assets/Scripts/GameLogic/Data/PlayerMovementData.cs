using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "PlayerMovementData", menuName = "Data/Player Data")]
public class PlayerMovementData : ScriptableObject
{
    [Header("Move Info")]
    public float runMaxSpeed = 8;
    public float runAcceleration; //ˮƽ���ټ��ٶ�
    [HideInInspector] public float runAccelAmount;
    public float runDecceleration; //ˮƽ���ټ��ٶ�
    [HideInInspector] public float runDeccelAmount;
    [Range(0.01f, 1)] public float accelInAir; //�ڿ��м���ʱˮƽ���ٶȻ�˸ñ�ֵ
    [Range(0.01f, 1)] public float deccelInAir; //�ڿ��м���ʱˮƽ���ٶȻ�˸ñ�ֵ
    public float speedThreshold; //��С�ٶ���ֵ
    public float adsorbeOutTime;
    public float leaveBlockCooldown;
    [Range(0.001f, 0.5f)] public float leaveInputBufferTime;
    
    [Space(20)]
    [Header("Jump Info")]
    public float jumpHeight; //��Ծ�߶�
    public float wallJumpForce;
    public float jumpTimeToApex; //��Ծ�����߶ȵ�ʱ��
    public int jumpCount = 2; //�����Ծ����
    [Range(0f, 1f)] public float coyoteTime; //����ʱ��
    [Range(0.01f, 0.5f)] public float jumpInputBufferTime; // ��������Ծ����ǰ��������Ծ�Ŀ���ʱ�䡣
    public float maxFallSpeed; //׹������ٶ�
    [HideInInspector] public float jumpForce;
    
    [Space(20)]
    [Header("Both Jumps")]
    public float jumpCutGravityMult; //����Ծ�жϣ��ɿ���Ծ����ʱ������˸ñ�ֵ
    [Range(0f, 1)] public float jumpHangGravityMult; //����Ծ����ʱ������˸ñ�ֵ
    public float jumpHangTimeThreshold; //�������ֱ�ٶ�С�ڸ���ֵʱ���������һ������ʱ��
    [Space(0.5f)]
    public float jumpHangAccelerationMult; //����ʱ�������ˮƽ���ٶȻ�������ֵ
    public float jumpHangMaxSpeedMult; //����ʱ����������ˮƽ�ٶȻ�������ֵ
    public float fallGravityMult;
    public float gravityStrength;
    public float gravityScale;

    [Space(20)]
    [Header("BeDamageInfo")]
    public Vector2 beDamageForce;
    
    private void OnValidate()
    {
        // ʹ�ù�ʽ��������ǿ�� (gravity = 2 * jumpHeight / timeToJumpApex^2)
        gravityStrength = -(2 * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);

        // ���������������ţ��������������Unity�����ı������鿴��Ŀ����/Physics2D��
        gravityScale = gravityStrength / Physics2D.gravity.y;

        // ʹ�ù�ʽ�����ܲ����ٺͼ�������amount = ((1 / Time.fixedDeltaTime) * acceleration) / runMaxSpeed
        runAccelAmount = (50 * runAcceleration) / runMaxSpeed;
        runDeccelAmount = (50 * runDecceleration) / runMaxSpeed;

        // ʹ�ù�ʽ������Ծ�� (initialJumpVelocity = gravity * timeToJumpApex)
        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

        #region ������Χ����
        runAcceleration = Mathf.Clamp(runAcceleration, 0.01f, runMaxSpeed);
        runDecceleration = Mathf.Clamp(runDecceleration, 0.01f, runMaxSpeed);
        #endregion
    }
}