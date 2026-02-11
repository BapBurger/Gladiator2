using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Sets up knife (SM_Gladius) grab interaction with hand tracking
/// Automatically adds required components for XR Interaction Toolkit
/// </summary>
public class HandKnifeGrabSetup : MonoBehaviour
{
    [Header("Knife Setup")]
    [Tooltip("The SM_Gladius prefab to attach to the hand")]
    public GameObject knifePrefab;

    [Tooltip("Position offset from hand")]
    public Vector3 knifePositionOffset = new Vector3(0f, 0f, 0.1f);

    [Tooltip("Rotation offset from hand")]
    public Vector3 knifeRotationOffset = new Vector3(0f, 0f, 0f);

    [Header("Hand References")]
    [Tooltip("Reference to OculusHand_R transform")]
    public Transform rightHandTransform;

    [Tooltip("Specific bone to attach knife to (e.g., palm, wrist)")]
    public Transform attachBone;

    [Header("Grab Settings")]
    [Tooltip("Enable grab interaction (false = knife stays attached to hand)")]
    public bool enableGrabInteraction = true;

    [Tooltip("Use hand tracking grab gestures")]
    public bool useHandGestures = true;

    private GameObject knifeInstance;
    private XRGrabInteractable grabInteractable;
    private XRDirectInteractor handInteractor;

    void Start()
    {
        SetupKnife();

        if (enableGrabInteraction)
        {
            SetupHandInteractor();
        }
    }

    /// <summary>
    /// Instantiate knife and add required components
    /// </summary>
    void SetupKnife()
    {
        if (knifePrefab == null)
        {
            Debug.LogError("HandKnifeGrabSetup: Knife prefab not assigned!");
            return;
        }

        // Find hand transform if not assigned
        if (rightHandTransform == null)
        {
            GameObject handObj = GameObject.Find("OculusHand_R");
            if (handObj != null)
            {
                rightHandTransform = handObj.transform;
            }
            else
            {
                Debug.LogError("HandKnifeGrabSetup: Could not find OculusHand_R!");
                return;
            }
        }

        // Use specific bone or hand root
        Transform parent = attachBone != null ? attachBone : rightHandTransform;

        // Instantiate knife
        knifeInstance = Instantiate(knifePrefab, parent);
        knifeInstance.name = "SM_Gladius_Grabbed";

        // Set local position and rotation
        knifeInstance.transform.localPosition = knifePositionOffset;
        knifeInstance.transform.localRotation = Quaternion.Euler(knifeRotationOffset);

        // Add Rigidbody if missing
        Rigidbody rb = knifeInstance.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = knifeInstance.AddComponent<Rigidbody>();
        }

        // Configure Rigidbody for hand interaction
        if (enableGrabInteraction)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.mass = 0.5f; // Light weapon
            rb.drag = 1f;
            rb.angularDrag = 0.5f;
        }
        else
        {
            // Kinematic if just attached to hand
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Add XRGrabInteractable if grab enabled
        if (enableGrabInteraction)
        {
            grabInteractable = knifeInstance.GetComponent<XRGrabInteractable>();
            if (grabInteractable == null)
            {
                grabInteractable = knifeInstance.AddComponent<XRGrabInteractable>();
            }

            // Configure grab settings
            grabInteractable.throwOnDetach = false;
            grabInteractable.trackPosition = true;
            grabInteractable.trackRotation = true;
            grabInteractable.smoothPosition = true;
            grabInteractable.smoothRotation = true;
            grabInteractable.smoothPositionAmount = 5f;
            grabInteractable.smoothRotationAmount = 5f;

            // Movement type: Instantaneous for hand tracking
            grabInteractable.movementType = XRBaseInteractable.MovementType.Instantaneous;

            Debug.Log("HandKnifeGrabSetup: SM_Gladius XRGrabInteractable configured");
        }

        Debug.Log($"HandKnifeGrabSetup: Knife instantiated and attached to {parent.name}");
    }

    /// <summary>
    /// Add XRDirectInteractor to hand for grabbing
    /// </summary>
    void SetupHandInteractor()
    {
        if (rightHandTransform == null)
        {
            Debug.LogError("HandKnifeGrabSetup: Right hand transform not found!");
            return;
        }

        // Try to find palm or wrist bone for interactor
        Transform interactorTransform = rightHandTransform.Find("b_r_wrist");
        if (interactorTransform == null)
        {
            // Fallback to hand root
            interactorTransform = rightHandTransform;
        }

        // Check if interactor already exists
        handInteractor = interactorTransform.GetComponent<XRDirectInteractor>();
        if (handInteractor == null)
        {
            handInteractor = interactorTransform.gameObject.AddComponent<XRDirectInteractor>();
        }

        // Add sphere collider for interaction detection
        SphereCollider interactCollider = interactorTransform.GetComponent<SphereCollider>();
        if (interactCollider == null)
        {
            interactCollider = interactorTransform.gameObject.AddComponent<SphereCollider>();
            interactCollider.radius = 0.1f;
            interactCollider.isTrigger = true;
        }

        // Configure interactor for hand tracking
        handInteractor.attachTransform = interactorTransform;
        handInteractor.selectActionTrigger = XRBaseControllerInteractor.InputTriggerType.State;

        Debug.Log($"HandKnifeGrabSetup: XRDirectInteractor added to {interactorTransform.name}");
    }

    /// <summary>
    /// Manually attach knife to hand (bypass grab interaction)
    /// </summary>
    public void AttachKnifeToHand()
    {
        if (knifeInstance != null && rightHandTransform != null)
        {
            Transform parent = attachBone != null ? attachBone : rightHandTransform;
            knifeInstance.transform.SetParent(parent);
            knifeInstance.transform.localPosition = knifePositionOffset;
            knifeInstance.transform.localRotation = Quaternion.Euler(knifeRotationOffset);

            Rigidbody rb = knifeInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            Debug.Log("HandKnifeGrabSetup: Knife manually attached to hand");
        }
    }

    /// <summary>
    /// Detach knife from hand
    /// </summary>
    public void DetachKnifeFromHand()
    {
        if (knifeInstance != null)
        {
            knifeInstance.transform.SetParent(null);

            Rigidbody rb = knifeInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            Debug.Log("HandKnifeGrabSetup: Knife detached from hand");
        }
    }

    void Update()
    {
        // Manual attach/detach for testing
        if (Input.GetKeyDown(KeyCode.G))
        {
            AttachKnifeToHand();
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            DetachKnifeFromHand();
        }
    }
}
