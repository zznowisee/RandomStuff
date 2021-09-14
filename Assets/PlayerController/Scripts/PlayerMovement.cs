using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    Rigidbody rb;

    [SerializeField, Range(0f, 100f)]
    float speed;
    [SerializeField, Range(0f, 100f)]
    float maxAcc, maxAirAcc = 1f;
    [SerializeField, Range(0, 5)]
    int maxAirJumpTimes = 0;
    /*    [SerializeField]
        Rect allowedArea = new Rect(-5f, -5f, 10f, 10f);*/
    Vector3 velocity, desiredVelocity;
    [SerializeField]
    float jumpHeight = 2f;

    [SerializeField]
    bool desiredJump;

    int jumpPhase = 0;

    [SerializeField, Range(0f, 90f)]
    float maxClimbAngle = 25f;
    float minAngleDotProduct;

    Vector3 contactNormal;

    int groundContactCount;
    bool OnGround => groundContactCount > 0;

    private void OnValidate()
    {
        minAngleDotProduct = Mathf.Cos(maxClimbAngle * Mathf.Deg2Rad);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        OnValidate();
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
                    // ��velocity�ķ������ó�0 �����Ͳ���Ҫ�ȴ�ͨ��acc��velocity���͵�0�Ĺ��� �Ӷ��������ʱ�� ������ճ��ǽ���ϵ�
                    //velocity.x = 0f;
                    //ͬʱ�����Խ�velocity�������� �������з����ĸо�
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
        UpdateState(); // ÿһ֡��ʼʱ�� ״̬���� ����velocity���³�rb��velocity �����Ծ��״̬
        AdjustVelocity();
        /*float acc = onGround ? maxAcc : maxAirAcc;
        float maxSpeedChange = acc * Time.deltaTime;
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);*/

        if (desiredJump) Jump();

        rb.velocity = velocity;

        //���������ж�
        ClearState();
    }

    void ClearState()
    {
        contactNormal = Vector3.zero;
        groundContactCount = 0;
    }

    void UpdateState()
    {
        velocity = rb.velocity;
        if (OnGround)
        {
            jumpPhase = 0;
            contactNormal.Normalize();
            if(groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = Vector3.up;
        }
    }

    void Jump()
    {
        desiredJump = false;
        if (OnGround || jumpPhase < maxAirJumpTimes)
        {
            //��Ծ״̬ +1
            jumpPhase++;
            //������Ծ���ٶ�
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            // alignedSpeed ��λ��б����ʱ б�·��߷������ֵ
            float alignedSpeed = Vector3.Dot(velocity, contactNormal);
            if (alignedSpeed > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
            }
            // ���λ��б���ϵĻ� �����б�µ� �Ƕ� ������Ծ�ķ��� ������һζ��������
            velocity += jumpSpeed * contactNormal;
        }
    }

    //���λ��б����ʱ x����ٶȷ���
    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }

    void AdjustVelocity()
    {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);

        float acc = OnGround ? maxAcc : maxAirAcc;
        float maxSpeedChange = acc * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
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
            //��collision�ķ���������y�������ֵ����0.9��ʱ�� ����Ϊ�ǵ���
            if(normal.y >= minAngleDotProduct)
            {
                groundContactCount++;
                contactNormal += normal;
            }
        }
    }
}
