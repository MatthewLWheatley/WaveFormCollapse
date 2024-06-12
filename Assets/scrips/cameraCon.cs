using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraCon : MonoBehaviour
{
    public float normalSpeed = 5.0f; // Normal movement speed.
    public float sprintSpeed = 10.0f; // Speed when Shift is pressed.

    private float currentSpeed; // Current movement speed.

    void Start()
    {
        currentSpeed = normalSpeed; // Start with normal speed.
    }

    void Update()
    {
        // Handle camera movement using WSAD keys.
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Check if Shift key is pressed to sprint.
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = normalSpeed;
        }

        // Calculate the new position for the camera.
        Vector3 newPosition = transform.position + new Vector3(horizontalInput, 0, verticalInput) * currentSpeed * Time.deltaTime;

        // Update the camera's position.
        transform.position = newPosition;
    }
}