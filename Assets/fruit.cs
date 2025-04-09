using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fruit : MonoBehaviour
{

    public float gravity = -9.8f; // Gravity force
    private Vector2 velocity;


    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 lastMousePosition;
    private bool grabbed;

    public LayerMask groundLayer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        
        //Debug.Log(velocity);


        if (Input.GetMouseButtonDown(0)) // Left-click pressed
        {
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
            if (hit != null && hit.transform == transform) // Check if clicking on this object
            {
                grabbed = true;
                offset = transform.position - mouseWorldPos;
            }
        }

        if (Input.GetMouseButtonUp(0)) // Left-click released
        {
            grabbed = false;
        }


        if (grabbed)
        {
            // Move object with mouse while keeping offset
            Vector3 newPosition = mouseWorldPos + offset;
            velocity = (newPosition - transform.position) / Time.deltaTime; // Calculate velocity
            
            transform.position = newPosition;
        }
        else
        {
            // Apply momentum after release
            transform.position += new Vector3(velocity.x * Time.deltaTime, velocity.y * Time.deltaTime, 0);

            //transform.position += velocity * Time.deltaTime;
            //velocity *= 0.95f; // Apply damping to gradually stop the movement
        }


        if (!IsGrounded() && !grabbed) // Only apply gravity if not grounded
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            velocity.y = 0; // Stop falling when hitting the ground
        }

        // Move the object manually
        transform.position += new Vector3(0, velocity.y * Time.deltaTime, 0);

    }

    bool IsGrounded()
    {
        Vector2 rayOrigin = transform.position;
        Vector2 rayDirection = Vector2.down;
        float rayLength = 0.3f; // Distance to check for ground

        // Perform the Raycast
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayLength, groundLayer);

        // Visualize the ray in Scene View (green if hitting, red if not)
        Color rayColor = hit.collider != null ? Color.green : Color.red;
        Debug.DrawRay(rayOrigin, rayDirection * rayLength, rayColor);

        return hit.collider != null;
    }

}
