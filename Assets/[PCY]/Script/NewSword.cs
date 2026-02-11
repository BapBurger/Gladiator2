using UnityEngine;

// ▼ 여기 이름이 NewSword로 바뀌어야 파일 이름과 매칭됩니다!
public class NewSword : MonoBehaviour
{
    [Header("1. 필수 설정")]
    public Transform handBone;        // RightHandAnchor (자동으로 찾음)
    public string handTag = "Player"; // 손에 달아줄 태그 이름

    [Header("2. 위치/회전 오프셋")]
    public Vector3 positionOffset;
    public Vector3 rotationOffset;

    [Header("3. 물리 설정")]
    public float throwThreshold = 2.0f;
    public float throwForceMultiplier = 1.2f;
    public float airResistance = 0.95f;
    public float returnGravity = 15.0f;
    public float spinSpeed = 1500.0f;
    public float catchDistance = 0.5f;

    [Header("현재 상태 (수정 X)")]
    public bool isHeld = false;
    public bool isFlying = false;
    public float currentHandSpeed = 0.0f;

    private Vector3 currentVelocity;
    private Vector3 lastHandPosition;
    private Vector3 handVelocity;

    void Start()
    {
        isHeld = false;
        isFlying = false;
        if (handBone == null) FindHandBone();
    }

    void Update()
    {
        if (handBone == null)
        {
            FindHandBone();
            return;
        }

        handVelocity = (handBone.position - lastHandPosition) / Time.deltaTime;
        currentHandSpeed = handVelocity.magnitude;
        lastHandPosition = handBone.position;

        if (isHeld)
        {
            StickToHand();
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
        transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.Self);

        currentVelocity *= airResistance;
        transform.position += currentVelocity * Time.deltaTime;

        Vector3 directionToHand = (handBone.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, handBone.position);

        float dynamicGravity = returnGravity + (distance * 2.0f);
        currentVelocity += directionToHand * dynamicGravity * Time.deltaTime;

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