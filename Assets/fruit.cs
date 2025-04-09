using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Pool;

public class fruit : MonoBehaviour
{
    public event Action hasBeenPicked;

    private IObjectPool<fruit> objectPool;
    public IObjectPool<fruit> ObjectPool { set => objectPool = value; }

    private bool picked;

    public float gravity = -9.8f; // Gravity force
    private Vector3 velocity;
    private Vector3 highestRecentVel;


    private bool grabbed;
    private Vector3 offset;
    private Vector3 lastMousePosition;
    public float maxGrabVel = 10.0f;

    public float velResetTimer = 0.2f;
    private float velResetTimerOriginal;

    public LayerMask groundLayer;

    public float angleAmount = 5f;  // How far it twines (± degrees)
    private Quaternion ogAngle;
    public float anim_speed = 2f;

    public float maxScale = 0.2f;
    private Vector3 ogScale;


    public float grabScaleFactor = 1.5f;  // How much to scale up (1.2 = 120% of original size)
    public float grabScaleDuration = 0.1f;     // Duration of the animation

    private bool grabScaleisAnimating = false;

    private bool lastFrameGrab = false;
    private bool duckme;

    // Start is called before the first frame update
    void Start()
    {
        picked = false;
        ogAngle = transform.rotation;
        ogScale = transform.localScale;
        velResetTimerOriginal = velResetTimer;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
       
        GrabAnimation();

        if (Input.GetMouseButtonDown(0)) // Left-click pressed
        {
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
            if (hit != null && hit.transform == transform) // Check if clicking on this object
            {
                if (!picked) 
                {
                    picked = true;
                    Debug.Log("has been picked, coming from the fruit");
                    hasBeenPicked?.Invoke();
                }

                if (!grabScaleisAnimating)
                {
                    StartCoroutine(AnimateScale());
                }

                grabbed = true;
                offset = transform.position - mouseWorldPos;
                velocity = Vector3.zero; // Reset velocity when grabbing
                lastMousePosition = mouseWorldPos;
            }
        }

        if (Input.GetMouseButtonUp(0)) // Left-click released
        {
            grabbed = false;
            transform.localScale = ogScale;
        }

        if (!picked)
        {
            float angle = Mathf.Sin(Time.time * anim_speed) * angleAmount;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            return;
        }

        if (grabbed)
        {
            lastFrameGrab = true;
            Vector3 newPosition = mouseWorldPos + offset;
            velocity = (mouseWorldPos - lastMousePosition) / Time.deltaTime;
            
            
            lastMousePosition = mouseWorldPos;
            transform.position = newPosition;
            // Capture mouse movement velocity


            if (velocity != Vector3.zero)
            {
                highestRecentVel = velocity;
            }

            if (velocity == Vector3.zero) {
                velResetTimer -= Time.deltaTime;
                Debug.Log("vel is zero, resetting...");
            }
            else {
                velResetTimer = velResetTimerOriginal;
            }

            if (velResetTimer < 0.0f)
            {
                Debug.Log("Resetting highest vel to zero");

                highestRecentVel = Vector3.zero;
            }

        }
        else
        {
            if (lastFrameGrab)
            {
                velocity = highestRecentVel/10;

                lastFrameGrab = false;
                Debug.Log("last frame vel was:");
                Debug.Log(highestRecentVel);
            }

            //Debug.Log(velocity);

            if (!IsGrounded()) // Apply gravity if not touching the ground
            {
                velocity.y += gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0; // Stop vertical movement on ground
                velocity.x *= 0.995f; // Gradual slowdown
            }

            // Apply momentum and gravity movement
            transform.position += velocity * Time.deltaTime;
        }

        velocity = Vector3.ClampMagnitude(velocity, maxGrabVel);

        if (!IsGrounded() && !grabbed) // Only apply gravity if not grounded
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            velocity.y = 0; // Stop falling when hitting the ground
        }

        //Vector3 clampedPosition = transform.position;
        //clampedPosition.x = Mathf.Clamp(clampedPosition.x, -10, 10);
        //clampedPosition.y = Mathf.Clamp(clampedPosition.y, -10, 10);
        //transform.position = clampedPosition;

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

    public void Deactivate()
    {
        StartCoroutine(DeactivateRoutine(0.0f));
    }

    IEnumerator DeactivateRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Release the projectile back to the pool
        objectPool.Release(this);
    }

}
