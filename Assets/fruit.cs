using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fruit : MonoBehaviour
{

    public float gravity = -9.8f; // Gravity force
    //private Vector2 velocity;
    

    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 lastMousePosition;
    private Vector3 gvelocity;
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

        if (Input.GetMouseButtonDown(0)) // Click detection
        {
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
            if (hit != null && hit.transform == transform)
            {
                grabbed = true;
                offset = transform.position - mouseWorldPos;
                gvelocity = Vector3.zero; // Reset velocity when picking up
                lastMousePosition = mouseWorldPos; // Store initial mouse position
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
            gvelocity = (mouseWorldPos - lastMousePosition) / Time.deltaTime; // Calculate velocity based on mouse movement
            lastMousePosition = mouseWorldPos; // Update last position
            transform.position = newPosition;
            Debug.Log(gvelocity);
            //gvelocity /= 2.0f;
        }
        else
        {
            // Apply momentum after release
            //Debug.Log(new Vector3(gvelocity.x * Time.deltaTime, gvelocity.y * Time.deltaTime, 0));
            transform.position += gvelocity * Time.deltaTime;
            gvelocity *= 0.95f;

            //transform.position += velocity * Time.deltaTime;
            //gvelocity *= 0.95f; // Apply damping to gradually stop the movement
        }


        //if (!IsGrounded() && !grabbed) // Only apply gravity if not grounded
        //{
        //    velocity.y += gravity * Time.deltaTime;
        //}
        //else
        //{
        //    velocity.y = 0; // Stop falling when hitting the ground
        //}

        // Move the object manually
        //transform.position += new Vector3(0, velocity.y * Time.deltaTime, 0);

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
