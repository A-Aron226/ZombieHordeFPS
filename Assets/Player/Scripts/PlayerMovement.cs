using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //Movement Speed
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float sprintSpeed = 10f;

    //Stamina Stats
    [SerializeField] float stamina = 100f;
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaRegenRate = 5f;
    [SerializeField] float staminaDrainRate = 10f;

    //Misc
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] float gravity = -9.81f;

    private CharacterController controller;
    private Player movement;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private Transform cameraTransform;
    private bool isSprinting;
    private bool canSprint;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        movement = new Player();

        movement.Movement.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        movement.Movement.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        movement.Movement.Sprint.performed += ctx => isSprinting = ctx.ReadValueAsButton(); //Sprint input
        movement.Movement.Sprint.canceled += ctx => isSprinting = false; //Reset sprint value
        movement.Enable();

        cameraTransform = Camera.main.transform;
        cameraTransform.position = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
        cameraTransform.parent = transform;
    }

    // Update is called once per frame
    void Update()
    {
        //Movement
        float currentSpeed = moveSpeed;
        if (isSprinting && canSprint && stamina > 0)
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
            Debug.Log("Cannot sprint");
        }
        else if (stamina >= maxStamina * 0.5f)
        {
            canSprint = true;
            Debug.Log("You can now sprint");
        }

        Debug.Log("Current Speed: " + currentSpeed);

        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        controller.Move(transform.TransformDirection(move) * moveSpeed * Time.deltaTime);

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
    }
}
