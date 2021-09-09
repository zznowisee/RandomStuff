using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    Rigidbody rb;

    [SerializeField, Range(0f, 100f)]
    float speed;
    [SerializeField, Range(0f, 100f)]
    float maxAcc;
    [SerializeField, Range(0, 5)]
    int maxAirJumpTimes = 0;
    /*    [SerializeField]
        Rect allowedArea = new Rect(-5f, -5f, 10f, 10f);*/
    Vector3 velocity, desiredVelocity;
    [SerializeField]
    float jumpHeight = 2f;
    bool desiredJump, onGround;
    int jumpPhase = 0;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        desiredJump |= Input.GetButtonDown("Jump");
        velocity = rb.velocity;
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        Vector3 dir = Vector3.ClampMagnitude(input, 1f);
        //input.Normalize();
        //Vector3 acc = dir * speed * Time.deltaTime;
        desiredVelocity = dir * speed;
        float maxSpeedChange = maxAcc * Time.deltaTime;

        /*Vector3 targetPosition = transform.localPosition + deltaMove;
        if (!allowedArea.Contains(new Vector2(targetPosition.x, targetPosition.z)))
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, allowedArea.xMin, allowedArea.xMax);
            targetPosition.z = Mathf.Clamp(targetPosition.z, allowedArea.yMin, allowedArea.yMax);
        }*/

        #region Rect
        /*        if(targetPosition.x < allowedArea.xMin)
                {
                    targetPosition.x = allowedArea.xMin;
                    // 将velocity的分量设置成0 这样就不需要等待通过acc将velocity降低到0的过程 从而导致这段时间 球球是粘在墙壁上的
                    //velocity.x = 0f;
                    //同时还可以将velocity分量反向 这样就有反弹的感觉
                    velocity.x = -velocity.x;
                }
                else if(targetPosition.x > allowedArea.xMax)
                {
                    targetPosition.x = allowedArea.xMax;
                    velocity.x = -velocity.x;
                }
                if(targetPosition.z < allowedArea.yMin)
                {
                    targetPosition.z = allowedArea.yMin;
                    velocity.z = -velocity.z;
                }
                else if(targetPosition.z > allowedArea.yMax)
                {
                    targetPosition.z = allowedArea.yMax;
                    velocity.z = -velocity.z;
                }*/
        #endregion
    }

    private void FixedUpdate()
    {
        UpdateState(); // 每一帧开始时的 状态更新 ：将velocity更新成rb的velocity 检测跳跃的状态
        float maxSpeedChange = maxAcc * Time.deltaTime;
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

        if (desiredJump) Jump();

        rb.velocity = velocity;
    }

    void UpdateState()
    {
        velocity = rb.velocity;
        if (onGround) jumpPhase = 0;
    }

    void Jump()
    {
        if(onGround || jumpPhase < maxAirJumpTimes)
        {
            jumpPhase++;
            desiredJump = false;
            velocity.y += Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            //当collision的法线向量的y轴分量的值大于0.9的时候 就视为是地面
            onGround |= normal.y >= .9f;
        }
    }
}
