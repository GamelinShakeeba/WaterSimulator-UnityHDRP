using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f; 
    public float waterSpeed = 3.5f; //speed in water
    private float originalMoveSpeed; // To store the original speed

    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;
    public Animator anim;

    [HideInInspector] public float walkSpeed;
    [HideInInspector] public float sprintSpeed;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask TerrainGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;

    Rigidbody rb;

    private bool isInWater;

    bool playingWalking = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;

        originalMoveSpeed = moveSpeed;
        isInWater = false;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, TerrainGround);

        MyInput();
        SpeedControl();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;

        UpdateAnimations();
        Debug.Log("Move Speed = " + moveSpeed);
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
    }

    private void UpdateAnimations()
    {
        if (Mathf.Abs(horizontalInput) > 0 || Mathf.Abs(verticalInput) > 0)
        {
            if (!anim.GetBool("isWalking"))
            {
                anim.SetBool("isWalking", true);
            }

            if(isInWater)
            {
                anim.SetBool("isWalkSlow", true);
                anim.SetBool("isWalking", false);
                playingWalking = false;
            }

            if(!isInWater) 
            { 
                anim.SetBool("isWalkSlow", false); 
            }
        }
        else if (anim.GetBool("isWalking") || anim.GetBool("isWalkSlow"))
        {
            anim.SetBool("isWalking", false);
            playingWalking = false;
            anim.SetBool("isWalkSlow", false);
        }
    }

    private bool IsInWater()
    {
        return isInWater; // Return the water state
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("water")) 
        {
            moveSpeed = waterSpeed; 
            isInWater = true; 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("water")) 
        {
            moveSpeed = originalMoveSpeed; 
            isInWater = false; 
        }
    }
}
