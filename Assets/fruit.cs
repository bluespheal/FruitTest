using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fruit : MonoBehaviour
{
    private bool picked;

    public float gravity = -9.8f; // Gravity force
    private Vector2 velocity;


    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 lastMousePosition;
    private bool grabbed;

    public LayerMask groundLayer;

    public float angleAmount = 5f;  // How far it twines (± degrees)
    private Quaternion ogAngle;
    public float anim_speed = 2f;

    public float maxScale = 0.2f;
    private Vector3 ogScale;


    public float grabScaleFactor = 1.5f;  // How much to scale up (1.2 = 120% of original size)
    public float grabScaleDuration = 0.1f;     // Duration of the animation

    private bool grabScaleisAnimating = false;


    // Start is called before the first frame update
    void Start()
    {
        picked = false;
        ogAngle = transform.rotation;
        ogScale = transform.localScale;

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
       
        GrabAnimation();

        if (Input.GetMouseButtonDown(0)) // Left-click pressed
        {
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
            if (hit != null && hit.transform == transform) // Check if clicking on this object
            {
                picked = true;
                grabbed = true;
                offset = transform.position - mouseWorldPos;

                if (!grabScaleisAnimating)
                {
                    StartCoroutine(AnimateScale());
                }
            }
        }

        if (Input.GetMouseButtonUp(0)) // Left-click released
        {
            grabbed = false;
            transform.localScale = ogScale;
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


        if (!IsGrounded() && !grabbed && picked) // Only apply gravity if not grounded
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            velocity.y = 0; // Stop falling when hitting the ground
        }

        // Move the object manually
        transform.position += new Vector3(0, velocity.y * Time.deltaTime, 0);


        if (!picked)
        {
            float angle = Mathf.Sin(Time.time * anim_speed) * angleAmount;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

    }

    void GrabAnimation()
    {
        float scaleFactor = 1 + Mathf.Sin(Time.time * anim_speed) *maxScale;
        transform.localScale = ogScale * scaleFactor;
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

    IEnumerator AnimateScale()
    {
        grabScaleisAnimating = true;
        float time = 0f;

        // Scale Up
        while (time < grabScaleDuration)
        {
            time += Time.deltaTime;
            float t = time / grabScaleDuration;
            transform.localScale = Vector3.Lerp(ogScale, ogScale * grabScaleFactor, t);
            yield return null;
        }

        time = 0f;

        // Scale Down
        while (time < grabScaleDuration)
        {
            time += Time.deltaTime;
            float t = time / grabScaleDuration;
            transform.localScale = Vector3.Lerp(ogScale * grabScaleFactor, ogScale, t);
            yield return null;
        }

        transform.localScale = ogScale;  // Ensure it's exactly at the original scale
        grabScaleisAnimating = false;
    }

}
