using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class PlayerMouvement : MonoBehaviour
{
    public CharacterController controller;

    [Header("Mouvement Parameters")]
    float currentSpeed;
    float walkingSpeed = 3f;
    float runningSpeed = 5f;
    float crouchingSpeed = 1.5f;
    float SpeedVelocity;
    
    [Header("Controls Parameters")]
    KeyCode spriteKey = KeyCode.LeftShift;
    KeyCode crouchKey = KeyCode.LeftControl;
    KeyCode jumpKey = KeyCode.Space;

    [Header("Jump Parameters")]
    float jumpHeight = 1f;
    float jumpStart = 0f;
    float jumpCooldown = 0.5f;
    
    [Header("Gravity Parameters")]
    float gravity = -19.58f;
    float timeInAir;

    //bool isGrounded => Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    bool shouldRunning => Input.GetKey(spriteKey) && !isCrouching;
    bool shouldCrouching => Input.GetKey(crouchKey) && !isRunning;
    bool shouldJump => Input.GetKey(jumpKey) && controller.isGrounded;
    bool canMove = true;
    bool isRunning;
    bool isCrouching;
    bool isJumping;

    double velocityX;
    double velocityZ;
    double horizontalVelocity;
    
    UnityEngine.Vector3 crouchScale = new UnityEngine.Vector3(1, 0.7f, 1);
    UnityEngine.Vector3 playerScale = new UnityEngine.Vector3(1, 1, 1);
    UnityEngine.Vector3 crouchVelocity =  new UnityEngine.Vector3();

    UnityEngine.Vector3 velocity;

    Animator animator;
    PlayerStates currentState;

    public enum PlayerStates {
        IDLE,
        WALK,
        RUN,
        CROUCH,
        JUMP,
        FALL
    }

    PlayerStates CurrentState {
        set {
            currentState = value;

            switch(currentState){
                case PlayerStates.IDLE:
                    animator.Play("Idle");
                    break;
                case PlayerStates.WALK:
                    animator.Play("Walk");
                    break;
                case PlayerStates.RUN:
                    animator.Play("Run");
                    break;
                case PlayerStates.JUMP:
                    animator.Play("Jump");
                    break;
                case PlayerStates.FALL:
                    animator.Play("Falling To Landing");
                    break;
            }
        }
    }

    void Start(){
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }
    // Update is called once per frame
    void Update(){

        if (canMove){
            HandleJump();
            HandleRunning();
            HandleCrouch();
        }

        UpdateMouvementPosition();
        HandleStateAnimation();
    }

    private void HandleJump(){

        if (shouldJump && controller.isGrounded){
            isJumping = true;
            if (Time.time > jumpStart + jumpCooldown){
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpStart = Time.time;
            }
        }

        if (controller.isGrounded){
            isJumping = false;
        }
        else {
            isJumping = true;
        }
    }

    private void HandleRunning(){
        if (shouldRunning) {
            isRunning = true;
            currentSpeed = Mathf.SmoothDamp(currentSpeed, runningSpeed, ref SpeedVelocity, 0.3f);
        } else {
            isRunning = false;
        }
    }

    private void HandleCrouch(){
        if (shouldCrouching){
            if (transform.localScale.y < 0.71f){
                transform.localScale = new UnityEngine.Vector3(1, 0.7f, 1);
            }else {
                transform.localScale = UnityEngine.Vector3.SmoothDamp(crouchScale, playerScale, ref crouchVelocity, 1f);
            }
            isCrouching = true;
            currentSpeed = Mathf.SmoothDamp(currentSpeed, crouchingSpeed, ref SpeedVelocity, 0.3f);
            transform.position = new UnityEngine.Vector3(transform.position.x, transform.position.y + 0.7f, transform.position.z);
        }
        else if (!isRunning && !shouldJump){
            if (transform.localScale.y > 0.99f){
                transform.localScale = new UnityEngine.Vector3(1, 1, 1);
            }else {
                transform.localScale = UnityEngine.Vector3.SmoothDamp(playerScale, crouchScale, ref crouchVelocity, 1f);
            }
            isCrouching = false;
            currentSpeed = Mathf.SmoothDamp(currentSpeed, walkingSpeed, ref SpeedVelocity, 0.3f);
            transform.position = new UnityEngine.Vector3(transform.position.x, transform.position.y - 0.7f, transform.position.z);
        }
    }

    private void UpdateMouvementPosition(){
        if (controller.isGrounded && velocity.y < 0){
            velocity.y = -1f;   
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        velocityX = x * currentSpeed * Time.deltaTime;
        velocityZ = z * currentSpeed * Time.deltaTime;
        horizontalVelocity = Math.Sqrt(Math.Pow(velocityX,2) + Math.Pow(velocityZ,2));

        UnityEngine.Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * currentSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleStateAnimation(){

        if (isRunning && !isJumping && controller.isGrounded && horizontalVelocity > 0){
            CurrentState = PlayerStates.RUN;
        }

        else if (!isRunning && !isJumping && controller.isGrounded && horizontalVelocity > 0){
            CurrentState = PlayerStates.WALK;
        }

        else if (isJumping){
            CurrentState = PlayerStates.JUMP;
        }

        else {
            CurrentState = PlayerStates.IDLE;
        }
    }
}
