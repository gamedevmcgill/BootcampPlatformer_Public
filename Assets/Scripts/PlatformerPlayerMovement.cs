using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlatformerPlayerMovement : MonoBehaviour
{
    enum AirState{
        grounded,   // character standing on ground
        freefall,   // character not touching ground and not jumping
        jumping,    // character is jumping
    }

    enum WalkState{
        idle,       // character not walking
        walking,    // character walking
    }


    // components
    [SerializeField]
    new Rigidbody rigidbody;
    

    // physics
    [Header("Physics")]
    [SerializeField] 
    Vector3 gravity = Physics.gravity;
    Vector2 velocity = Vector2.zero;
    [SerializeField]
    float walkSpeed = 10f;
    [SerializeField]
    float acceleration = 20f;
    [SerializeField]
    float deceleration = 30f;


    // jump
    [Header("Jump")]
    [SerializeField]
    AnimationCurve jumpCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    AirState currentAirState = AirState.grounded;
    [SerializeField]
    float jumpDuration = 0.5f;
    float jumpTime = 0f;
    bool _touchingGround;


    // ground check
    [Header("Groundedness")]
    [SerializeField]
    float footHeight;
    [SerializeField, Range(0f, 180f)]
    float inclineLimit;

    // move
    WalkState currentWalkState;
    Vector2 moveInput = Vector2.zero;

    private void OnValidate() => rigidbody = GetComponent<Rigidbody>();

    private void FixedUpdate() 
    {
        if (!rigidbody)
        {
            
        }

        // recalculate velocity
        velocity = rigidbody.velocity;

        // update air state
        if (_touchingGround)
        {
            currentAirState = AirState.grounded;
        }
        else if (currentAirState != AirState.jumping || jumpTime >= jumpDuration)
        {
            currentAirState = AirState.freefall;
        }

        switch (currentAirState)
        {
            case AirState.grounded:
            {
                velocity.y = 0f;
                goto case AirState.freefall;
            }
            case AirState.jumping:
            {
                float prevTime = jumpTime;
                jumpTime += Time.fixedDeltaTime;
                if (jumpTime >= jumpDuration)
                {
                    currentAirState = AirState.freefall;
                    goto case AirState.freefall;
                }
                velocity.y = (jumpCurve.Evaluate(jumpTime / jumpDuration) - jumpCurve.Evaluate(prevTime / jumpDuration)) / Time.fixedDeltaTime;
                break;
            }
            case AirState.freefall:
            {
                velocity += (Vector2) gravity * Time.fixedDeltaTime;
                break;
            }
            default:
            {
                break;
            }
        }
        
        switch (currentWalkState)
        {
            case WalkState.idle:
            {
                velocity.x = Mathf.MoveTowards(velocity.x, 0f, deceleration * Time.fixedDeltaTime);
                break;
            }
            case WalkState.walking:
                velocity.x = Mathf.MoveTowards(velocity.x, moveInput.x * walkSpeed, acceleration * Time.fixedDeltaTime);
                break;
            default:
            {
                break;
            }
        }

        rigidbody.velocity = velocity;

        _touchingGround = false;
    }

    private void OnJump(InputValue value){
        if (currentAirState == AirState.grounded)
        {
            currentAirState = AirState.jumping;
            _touchingGround = false;
            jumpTime = 0f;
        }
    }

    private void OnMove(InputValue value){
        moveInput = value.Get<Vector2>();

        if (Mathf.Approximately(moveInput.x, 0f))
        {
            currentWalkState = WalkState.idle;
        }
        else
        {
            currentWalkState = WalkState.walking;
        }
    }

    private void OnCollisionEnter(Collision collisionInfo) => CheckGroundedness(collisionInfo);
    private void OnCollisionStay(Collision collisionInfo) => CheckGroundedness(collisionInfo);

    private void CheckGroundedness(Collision collisionInfo)
    {
        if (velocity.y > 0f || _touchingGround) return;
        foreach (ContactPoint contact in collisionInfo.contacts)
        {
            if (transform.InverseTransformPoint(contact.point).y > footHeight || Vector3.Angle(gravity, contact.normal) < inclineLimit)
            {
                continue;
            }

            _touchingGround = true;

#if UNITY_EDITOR
            Debug.DrawLine(transform.position, contact.point, Color.green, Time.fixedDeltaTime);
#endif
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Vector3 footPosition = new Vector3(0f, footHeight, 0f);
        Gizmos.DrawLine(Vector3.forward + footPosition, Vector3.back + footPosition);
        Gizmos.DrawLine(Vector3.left + footPosition, Vector3.right + footPosition);
    }
#endif
}
