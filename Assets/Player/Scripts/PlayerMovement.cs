using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //Movement Stats
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float sprintSpeed = 10f;
    [SerializeField] float friction = 10f;

    //Stamina Stats
    [SerializeField] float stamina = 100f;
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaRegenRate = 5f;
    [SerializeField] float staminaDrainRate = 10f;

    //Jump Stats
    [SerializeField] float jumpHeight = 1.5f;

    //Crouch Stats
    [SerializeField] float crouchHeight = 1f;
    [SerializeField] float standHeight = 2f;
    [SerializeField] float crouchSpeed = 2.5f;

    //Prone Stats
    [SerializeField] float proneSpeed = 1f;
    [SerializeField] float proneHeight = 0.5f;

    //Misc
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float transitionSpeed = 10f;

    private CharacterController controller;
    private Player movement;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private Transform cameraTransform;
    private bool isSprinting;
    private bool canSprint;
    private bool isCrouching;
    private bool isProne;
    private float originalCenterY;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        movement = new Player();

        movement.Movement.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        movement.Movement.Move.canceled += ctx => moveInput = Vector2.zero;
        movement.Movement.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        movement.Movement.Sprint.performed += ctx => isSprinting = ctx.ReadValueAsButton(); //Sprint input
        movement.Movement.Sprint.canceled += ctx => isSprinting = false; //Reset sprint value
        movement.Movement.Jump.performed += ctx => Jump();
        movement.Movement.Crouch.performed += ctx => ToggleCrouch();
        movement.Movement.Prone.performed += ctx => ToggleProne();
        movement.Enable();

        cameraTransform = Camera.main.transform;
        cameraTransform.position = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
        cameraTransform.parent = transform;

        canSprint = true;
        originalCenterY = controller.center.y;
    }

    // Update is called once per frame
    void Update()
    {
        //Movement
        float currentSpeed = moveSpeed;
        if (isProne)
        {
            currentSpeed = proneSpeed;
        }

        else if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }

        else if (isSprinting && canSprint && stamina > 0)
        {
            currentSpeed = sprintSpeed;
            stamina -= staminaDrainRate * Time.deltaTime;
            if (stamina < 0)
            {
                stamina = 0;
            }
        }

        else
        {
            stamina += staminaRegenRate * Time.deltaTime;
            if (stamina > maxStamina)
            {
                stamina = maxStamina;
            }
        }

        // Checking if stamina is at 50% or more to sprint
        if (stamina == 0)
        {
            canSprint = false;
            //Debug.Log("Cannot sprint");
        }
        else if (stamina >= maxStamina * 0.5f)
        {
            canSprint = true;
            //Debug.Log("You can now sprint");
        }

        //Debug.Log("Current Speed: " + currentSpeed);

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        controller.Move(transform.TransformDirection(move) * moveSpeed * Time.deltaTime);

        //Friction
        if (controller.isGrounded && moveInput == Vector2.zero)
        {
            velocity.x = Mathf.Lerp(velocity.x, 0, friction * Time.deltaTime);
            velocity.z = Mathf.Lerp(velocity.z, 0, friction * Time.deltaTime);
        }

        //Gravity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        //Camera Rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);
        cameraTransform.Rotate(Vector3.left * mouseY);

        //Transitions
        float targetHeight = standHeight;
        float targetCenterY = originalCenterY;
        if (isProne)
        {
            targetHeight = proneHeight;
            targetCenterY = proneHeight / 2f;
        }

        else if (isCrouching)
        {
            targetHeight = crouchHeight;
            targetCenterY = crouchHeight / 2f;
        }
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * transitionSpeed);
        controller.center = new Vector3(controller.center.x, Mathf.Lerp(controller.center.y, targetCenterY, Time.deltaTime * transitionSpeed), controller.center.z);

        cameraTransform.localPosition = new Vector3(cameraTransform.localPosition.x, Mathf.Lerp(cameraTransform.localPosition.y, targetHeight - 0.5f, Time.deltaTime * transitionSpeed), cameraTransform.localPosition.z);
    }

    void Jump()
    {
        if (controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); //Jump calculation
        }
    }

    void ToggleCrouch()
    {
        //prone to crouch
        if (isProne)
        {
            isProne = false;
            isCrouching = true;
        }

        //crouch to stand
        else if (isCrouching)
        {
            isCrouching = false;
        }

        //stand to crouch
        else
        {
            isCrouching = true;
        }

        //isCrouching = !isCrouching;
        //Debug.Log(isCrouching);
    }

    void ToggleProne()
    {
        //prone to stand
        if (isProne)
        {
            isProne = false;
        }

        //crouch to prone
        else if (isCrouching)
        {
            isCrouching = false;
            isProne = true;
        }

        //stand to prone
        else
        {
            isProne = true;
            //isProne = !isProne;
        }
    }
}
