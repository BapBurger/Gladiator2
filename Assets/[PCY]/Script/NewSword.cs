using UnityEngine;

public class NewSword : MonoBehaviour
{
    [Header("1. Required References")]
    public Transform handBone;        // RightHandAnchor (auto-detected)
    public string handTag = "Player"; // Tag name for hand detection

    [Header("2. Position/Rotation Offset")]
    public Vector3 positionOffset;
    public Vector3 rotationOffset;

    [Header("3. Throwing Parameters")]
    public float throwThreshold = 2.0f;
    public float throwForceMultiplier = 1.2f;
    public float airResistance = 0.98f;           // Air drag
    public float gravityMultiplier = 1.0f;        // Gravity strength
    public float returnForce = 5.0f;              // Force pulling back to hand
    public float returnForceDistance = 2.0f;      // Distance threshold for return force
    public float spinSpeed = 720.0f;              // Y-axis rotation speed (degrees/sec)
    public float catchDistance = 0.5f;

    [Header("State Display (Read-Only)")]
    [Tooltip("Is the knife currently held in hand")]
    public bool isHeld = false;
    [Tooltip("Is the knife currently flying through air")]
    public bool isFlying = false;
    [Tooltip("Current hand movement speed")]
    public float currentHandSpeed = 0.0f;

    private Rigidbody rb;
    private Vector3 currentVelocity;
    private Vector3 lastHandPosition;
    private Vector3 handVelocity;
    private bool initialized = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;  // Disable physics, we control it manually
            rb.useGravity = false;
        }

        isHeld = false;
        isFlying = false;

        if (handBone == null)
        {
            FindHandBone();
        }

        if (handBone != null)
        {
            lastHandPosition = handBone.position;
        }
    }

    void Update()
    {
        if (handBone == null)
        {
            FindHandBone();
            return;
        }

        // Initialize by attaching to hand after first frame
        if (!initialized && handBone != null)
        {
            initialized = true;
            Grab();  // Auto-attach to hand on start
            lastHandPosition = handBone.position;
            return;
        }

        // Calculate hand velocity
        handVelocity = (handBone.position - lastHandPosition) / Time.deltaTime;
        currentHandSpeed = handVelocity.magnitude;
        lastHandPosition = handBone.position;

        if (isHeld)
        {
            StickToHand();

            // Throw detection
            if (currentHandSpeed > throwThreshold)
            {
                Throw(handVelocity);
            }
        }
        else if (isFlying)
        {
            FlyAndReturn();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isHeld || isFlying) return;

        if (other.CompareTag(handTag) || other.name.Contains("Hand"))
        {
            Grab();
        }
    }

    void Grab()
    {
        isHeld = true;
        isFlying = false;
        currentVelocity = Vector3.zero;
    }

    void StickToHand()
    {
        transform.position = handBone.TransformPoint(positionOffset);
        transform.rotation = handBone.rotation * Quaternion.Euler(rotationOffset);
    }

    void FlyAndReturn()
    {
        // Rotate around Y-axis (green axis)
        transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.Self);

        // Apply air resistance
        currentVelocity *= airResistance;

        // Apply gravity: downward force (-Y direction)
        currentVelocity += Physics.gravity * gravityMultiplier * Time.deltaTime;

        // Return force to hand (spring-like behavior)
        Vector3 directionToHand = (handBone.position - transform.position);
        float distance = directionToHand.magnitude;

        // Apply return force based on distance
        // Only pull when beyond certain distance
        if (distance > returnForceDistance)
        {
            float returnStrength = Mathf.Pow(distance - returnForceDistance, 1.5f);
            currentVelocity += directionToHand.normalized * returnForce * returnStrength * Time.deltaTime;
        }

        // Update position
        transform.position += currentVelocity * Time.deltaTime;

        // Auto-catch when close enough
        if (distance < catchDistance)
        {
            Grab();
        }
    }

    public void Throw(Vector3 handVel)
    {
        isHeld = false;
        isFlying = true;
        currentVelocity = handVel * throwForceMultiplier;
    }

    void FindHandBone()
    {
        GameObject hand = GameObject.FindGameObjectWithTag(handTag);
        if (hand != null)
        {
            handBone = hand.transform;
            return;
        }

        GameObject anchor = GameObject.Find("RightHandAnchor");
        if (anchor != null) handBone = anchor.transform;
    }
}
