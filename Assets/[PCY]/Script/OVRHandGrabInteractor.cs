using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

/// <summary>
/// Custom XR Interactor for Oculus Hand Tracking
/// Integrates Meta OVR Hand with XR Interaction Toolkit
/// Detects pinch gestures for grabbing objects
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class OVRHandGrabInteractor : XRDirectInteractor
{
    [Header("OVR Hand Tracking")]
    [Tooltip("Is this for right hand? (false = left hand)")]
    public bool isRightHand = true;

    [Tooltip("Pinch strength threshold to trigger grab (0-1)")]
    [Range(0f, 1f)]
    public float pinchThreshold = 0.7f;

    [Tooltip("Use index pinch gesture")]
    public bool useIndexPinch = true;

    [Tooltip("Use grip/fist gesture")]
    public bool useGrip = true;

    [Header("Hand Bones")]
    [Tooltip("Index finger tip bone for pinch detection")]
    public Transform indexTip;

    [Tooltip("Thumb tip bone for pinch detection")]
    public Transform thumbTip;

    [Tooltip("Palm center for grip detection")]
    public Transform palmCenter;

    private bool isGrabbing = false;
    private bool wasPinching = false;

    // OVR Hand Tracking API (Meta XR SDK)
    // Note: This requires OVRHand component to be present in the scene
    private OVRHand ovrHand;

    protected override void Awake()
    {
        base.Awake();

        // Try to find OVRHand component
        ovrHand = GetComponentInParent<OVRHand>();

        // Auto-find hand bones if not assigned
        if (indexTip == null || thumbTip == null || palmCenter == null)
        {
            AutoFindHandBones();
        }
    }

    /// <summary>
    /// Automatically find hand bones from OculusHand_R hierarchy
    /// </summary>
    void AutoFindHandBones()
    {
        Transform handRoot = transform.root;

        if (isRightHand)
        {
            if (indexTip == null)
            {
                indexTip = FindDeepChild(handRoot, "b_r_index3");
                if (indexTip != null) Debug.Log($"OVRHandGrabInteractor: Found index tip at {indexTip.name}");
            }

            if (thumbTip == null)
            {
                thumbTip = FindDeepChild(handRoot, "b_r_thumb3");
                if (thumbTip != null) Debug.Log($"OVRHandGrabInteractor: Found thumb tip at {thumbTip.name}");
            }

            if (palmCenter == null)
            {
                palmCenter = FindDeepChild(handRoot, "b_r_wrist");
                if (palmCenter != null) Debug.Log($"OVRHandGrabInteractor: Found palm center at {palmCenter.name}");
            }
        }
        else
        {
            // Left hand bones
            if (indexTip == null) indexTip = FindDeepChild(handRoot, "b_l_index3");
            if (thumbTip == null) thumbTip = FindDeepChild(handRoot, "b_l_thumb3");
            if (palmCenter == null) palmCenter = FindDeepChild(handRoot, "b_l_wrist");
        }
    }

    /// <summary>
    /// Recursively find child transform by name
    /// </summary>
    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractor(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
        {
            CheckHandGesture();
        }
    }

    /// <summary>
    /// Check hand gesture for grab/release
    /// </summary>
    void CheckHandGesture()
    {
        bool shouldGrab = false;

        // Method 1: Check pinch using OVRHand (if available)
        if (ovrHand != null && useIndexPinch)
        {
            float pinchStrength = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
            shouldGrab = pinchStrength >= pinchThreshold;
        }
        // Method 2: Manual distance check between thumb and index
        else if (useIndexPinch && indexTip != null && thumbTip != null)
        {
            float distance = Vector3.Distance(indexTip.position, thumbTip.position);
            // Pinch detected if fingers are close (< 3cm)
            shouldGrab = distance < 0.03f;
        }

        // Method 3: Grip detection (all fingers curled)
        if (!shouldGrab && useGrip && ovrHand != null)
        {
            // Check if multiple fingers are pinched (grip gesture)
            float gripStrength = 0f;
            gripStrength += ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle);
            gripStrength += ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Ring);
            gripStrength += ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Pinky);

            shouldGrab = gripStrength / 3f >= pinchThreshold;
        }

        // Update grab state
        if (shouldGrab && !wasPinching)
        {
            // Start grab
            isGrabbing = true;
            Debug.Log("OVRHandGrabInteractor: Pinch detected - attempting grab");
        }
        else if (!shouldGrab && wasPinching)
        {
            // Release grab
            isGrabbing = false;
            Debug.Log("OVRHandGrabInteractor: Pinch released");
        }

        wasPinching = shouldGrab;
    }

    /// <summary>
    /// Override to use hand gesture instead of input action
    /// </summary>
    public override bool isSelectActive
    {
        get { return isGrabbing; }
    }

    void OnDrawGizmos()
    {
        // Visualize interaction sphere
        if (attachTransform != null)
        {
            Gizmos.color = isGrabbing ? Color.green : Color.yellow;
            SphereCollider col = GetComponent<SphereCollider>();
            if (col != null)
            {
                Gizmos.DrawWireSphere(attachTransform.position, col.radius);
            }
        }

        // Visualize pinch detection
        if (indexTip != null && thumbTip != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(indexTip.position, thumbTip.position);
        }
    }
}
