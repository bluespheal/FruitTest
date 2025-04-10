using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Pool;

public class fruit : MonoBehaviour
{
    // Event triggered when the fruit has been picked
    public event Action hasBeenPicked;

    // Object pooling reference
    private IObjectPool<fruit> objectPool;
    public IObjectPool<fruit> ObjectPool { set => objectPool = value; }

    private bool picked; // Indicates if the fruit has been picked
    private Vector3 initial_pos;// Stores the initial position of the fruit

    // Physics properties
    public float gravity = -9.8f; 
    private Vector3 velocity;
    private Vector3 highestRecentVel;

    // Grab-related properties
    private bool grabbed;
    private Vector3 offset;
    private Vector3 lastMousePosition;
    public float maxGrabVel = 10.0f;

    public float velResetTimer = 0.2f;
    private float velResetTimerOriginal;

    public LayerMask groundLayer;
    private bool lastFrameGrab = false;

    // Animation properties
    public float angleAmount = 5f;  // How far it twines (± degrees)
    private Quaternion ogAngle;
    public float anim_speed = 2f;

    // Scaling properties
    public float maxScale = 0.2f;
    private Vector3 ogScale;

    public float rotationSpeed = 20f;

    public float grabScaleFactor = 1.5f;  // How much to scale up (1.2 = 120% of original size)
    public float grabScaleDuration = 0.1f;     // Duration of the animation

    private bool grabScaleisAnimating = false;

    // Tree and trunk movement
    private GameObject tree;
    private float currentTreeSkew;
    private float finalTreePos;
    private float etime = 0.0f;

    private GameObject trunk;
    private float currentTrunkSkew;
    private float finalTrunkPos;

    // Scaling animation control
    public float duration = 1f; // Time it takes to scale from 0 to 1
    private float elapsedTime = 0f;
    private bool scaling = true;

    // Start is called before the first frame update
    void Start()
    {
        picked = false;
        ogAngle = transform.rotation;
        ogScale = transform.localScale;
        velResetTimerOriginal = velResetTimer;
        initial_pos = transform.position;

        transform.localScale = Vector3.zero; // Set initial scale to 0

        // Find tree and trunk objects
        tree = GameObject.Find("foliage");
        trunk = GameObject.Find("trunk");

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GrabAnimation();// Handle grab animation

        // Scaling effect when spawned
        if (scaling)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration); // Normalize time
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, progress * 3);

            if (progress >= 1f)
                scaling = false; // Stop updating once fully scaled
        }

        // Check if fruit has moved from its initial position
        if (!picked)
        {

                if (Vector2.Distance(transform.position, initial_pos) > 0.5f)
            {
                picked = true;
                hasBeenPicked?.Invoke();
            }
        }
        
        // Handle grabbing logic
        if (Input.GetMouseButtonDown(0)) // Left-click pressed
        {
            Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
            
            if (hit != null && hit.transform == transform) // Check if clicking on this object
            {
               

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

        if (Input.GetMouseButtonUp(0)) // Release grab
        {
            grabbed = false;
            transform.localScale = ogScale;
        }

        if (!picked)
        {
            if (grabbed)
            {
                lastFrameGrab = true;
                Vector3 newPosition = mouseWorldPos + offset;
                velocity = (mouseWorldPos - lastMousePosition) / Time.deltaTime;


                lastMousePosition = mouseWorldPos;
                transform.position = newPosition;

                currentTreeSkew = -(initial_pos.x - mouseWorldPos.x);
                tree.GetComponent<SpriteRenderer>().material.SetFloat("_Distance", currentTreeSkew);

                currentTrunkSkew = -(initial_pos.x - mouseWorldPos.x)/5;
                trunk.GetComponent<SpriteRenderer>().material.SetFloat("_Distance", currentTrunkSkew);
            }
            finalTreePos = currentTreeSkew;
            finalTrunkPos = currentTrunkSkew;

            float angle = Mathf.Sin(Time.time * anim_speed) * angleAmount;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            return;

        }
        else
        {
            etime += Time.deltaTime; 
            currentTreeSkew = BounceToZero(finalTreePos, etime, 3f, 7f);
            currentTrunkSkew = BounceToZero(finalTrunkPos, etime, 7f, 10f);
           
            tree.GetComponent<SpriteRenderer>().material.SetFloat("_Distance", currentTreeSkew);
            trunk.GetComponent<SpriteRenderer>().material.SetFloat("_Distance", currentTrunkSkew);
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
            }
            else {
                velResetTimer = velResetTimerOriginal;
            }

            if (velResetTimer < 0.0f)
            {
                highestRecentVel = Vector3.zero;
            }

        }
        else
        {
            if (lastFrameGrab)
            {
                velocity = highestRecentVel/10;

                lastFrameGrab = false;
            }

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

        if (velocity.x != 0)
        {
            if (!IsGrounded())
            {
                float direction = Mathf.Sign(velocity.x); // -1 for left, 1 for right
                transform.Rotate(Vector3.forward * direction * (velocity.x * rotationSpeed) * Time.deltaTime);
            }
            else
            {
                float direction = Mathf.Sign(velocity.x); // -1 for left, 1 for right
                transform.Rotate(Vector3.forward * direction * (velocity.x * rotationSpeed * 4) * Time.deltaTime);
            }
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

    public static float BounceToZero(float initialValue, float time, float damping = 0.3f, float frequency = 4f)
    {
        return initialValue * Mathf.Exp(-damping * time) * Mathf.Cos(frequency * time * Mathf.PI);
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
