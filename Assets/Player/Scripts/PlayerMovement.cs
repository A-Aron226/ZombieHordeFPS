using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float mouseSensitivity = 2f;
    [SerializeField] float gravity = -9.81f;
    private CharacterController controller;
    private Player movement;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector3 velocity;
    private Transform cameraTransform;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        movement = new Player();

        movement.Movement.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        movement.Movement.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        movement.Enable();

        cameraTransform = Camera.main.transform;
        cameraTransform.position = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
        cameraTransform.parent = transform;
    }

    // Update is called once per frame
    void Update()
    {
        //Movement
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        controller.Move(transform.TransformDirection(move) * moveSpeed * Time.deltaTime);

        /*float moveX = Input.GetAxis("Horizontal") * moveSpeed;
        float moveZ = Input.GetAxis("Vertical") * moveSpeed;
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * Time.deltaTime);*/ //Old Input system

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
