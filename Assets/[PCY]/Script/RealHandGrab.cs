using UnityEngine;

public class RealHandGrab : MonoBehaviour
{
    [Header("1. 필수 연결")]
    public OVRHand rightHand;
    public Rigidbody swordRb;

    [Header("2. 끈적한 그립 설정 (핵심!)")]
    [Range(0, 1)] public float grabThreshold = 0.8f;   // 잡는 기준 (0.8 이상 꽉 쥐어야 잡힘)
    [Range(0, 1)] public float releaseThreshold = 0.2f; // 놓는 기준 (0.2 이하로 손을 펴야 놓임)

    [Header("3. 기타 설정")]
    public float grabDistance = 0.2f;
    public float throwPower = 1.5f;

    [Header("4. 위치/각도 미세 조정")]
    public Vector3 positionOffset;
    public Vector3 rotationOffset;

    [Header("5. 상태 모니터링")]
    public bool isHeld = false;
    [Range(0, 1)] public float currentGripStrength = 0.0f; // 현재 손을 얼마나 꽉 쥐었는지 보여줌 (0~1)

    // 내부 변수
    private Vector3 lastHandPos;
    private Vector3 handVelocity;

    void Start()
    {
        if (swordRb == null) swordRb = GetComponent<Rigidbody>();
        isHeld = false;
        swordRb.isKinematic = false;
    }

    void Update()
    {
        if (rightHand == null) return;

        // 1. 손 속도 계산
        handVelocity = (rightHand.transform.position - lastHandPos) / Time.deltaTime;
        lastHandPos = rightHand.transform.position;

        // 2. [핵심] 현재 손가락 중 가장 세게 쥔 값(Strength)을 가져옴
        currentGripStrength = GetMaxPinchStrength();

        // 3. 거리 계산
        float distance = Vector3.Distance(transform.position, rightHand.transform.position);

        // [로직] 잡기: 힘이 grabThreshold(0.8)을 넘으면 잡음
        if (!isHeld && currentGripStrength > grabThreshold && distance <= grabDistance)
        {
            Grab();
        }
        // [로직] 놓기: 힘이 releaseThreshold(0.2)보다 작아져야(손을 펴야) 놓음
        else if (isHeld && currentGripStrength < releaseThreshold)
        {
            Release();
        }

        // [상태] 잡고 있는 동안 위치 고정
        if (isHeld)
        {
            StickToHand();
        }
    }

    // 엄지를 제외한 나머지 4손가락 중 가장 세게 쥔 강도를 반환 (0.0 ~ 1.0)
    float GetMaxPinchStrength()
    {
        float maxStrength = 0.0f;

        // 검지, 중지, 약지, 소지 중 가장 큰 값을 찾음
        maxStrength = Mathf.Max(maxStrength, rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index));
        maxStrength = Mathf.Max(maxStrength, rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle));
        maxStrength = Mathf.Max(maxStrength, rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Ring));
        maxStrength = Mathf.Max(maxStrength, rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Pinky));

        return maxStrength;
    }

    void Grab()
    {
        isHeld = true;
        swordRb.isKinematic = true;
        swordRb.useGravity = false;
        // Debug.Log("잡음! (힘: " + currentGripStrength + ")");
    }

    void Release()
    {
        isHeld = false;
        swordRb.isKinematic = false;
        swordRb.useGravity = true;
        swordRb.velocity = handVelocity * throwPower;
        swordRb.angularVelocity = rightHand.transform.right * 5f;
        // Debug.Log("놓음! (힘: " + currentGripStrength + ")");
    }

    void StickToHand()
    {
        transform.position = rightHand.transform.TransformPoint(positionOffset);
        transform.rotation = rightHand.transform.rotation * Quaternion.Euler(rotationOffset);
    }
}