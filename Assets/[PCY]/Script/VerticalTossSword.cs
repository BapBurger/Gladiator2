using UnityEngine;

public class VerticalTossSword : MonoBehaviour
{
    [Header("1. 필수 연결")]
    public OVRHand rightHand;
    public Rigidbody swordRb;

    [Header("2. 퍼포먼스(공중제비) 설정")]
    public float targetHeight = 0.5f;   // 목표 높이 (50cm)
    public int spinCount = 3;           // 회전 횟수 (3바퀴)
    public float returnDelayBuffer = 0.1f; // 내려오고 나서 손으로 오기 전 잠깐 뜸들이기

    [Header("3. 복귀 및 자동 잡기 설정")]
    public float returnPower = 15.0f;    // 손으로 돌아오는 속도

    // ★ 새로 추가됨: 자동 잡기 거리 조절 (0.05m ~ 1.0m)
    [Range(0.05f, 1.0f)]
    public float autoCatchDistance = 0.2f;

    [Header("4. 위치 보정")]
    public Vector3 positionOffset;
    public Vector3 rotationOffset;

    [Header("5. 감도 설정")]
    [Range(0, 1)] public float grabThreshold = 0.8f;   // 잡는 기준 (장전 기준)
    [Range(0, 1)] public float releaseThreshold = 0.3f; // 놓는 기준 (발사 기준)

    // 내부 변수
    private bool isHeld = false;
    private bool isReadyToLaunch = false;
    private bool isPerforming = false;
    private float flightTimer = 0.0f;
    private float totalFlightTime = 0.0f;

    void Start()
    {
        if (swordRb == null) swordRb = GetComponent<Rigidbody>();

        // 초기화
        isHeld = false;
        isReadyToLaunch = false;
        isPerforming = false;
        swordRb.isKinematic = false;
    }

    void Update()
    {
        if (rightHand == null) return;

        // 1. 핀치 강도(주먹 쥐는 힘) 측정
        float currentGripStrength = GetMaxPinchStrength();
        float distance = Vector3.Distance(transform.position, rightHand.transform.position);

        // ---------------------------------------------------------
        // [상태 1] 잡고 있을 때
        // ---------------------------------------------------------
        if (isHeld)
        {
            StickToHand();

            // A. 재장전 (Reload) 확인
            if (currentGripStrength > grabThreshold)
            {
                isReadyToLaunch = true;
            }

            // B. 발사 (Launch)
            if (isReadyToLaunch && currentGripStrength < releaseThreshold)
            {
                StartPerformance();
            }
        }
        // ---------------------------------------------------------
        // [상태 2] 퍼포먼스(공중제비) 중일 때
        // ---------------------------------------------------------
        else if (isPerforming)
        {
            flightTimer += Time.deltaTime;

            // 체류 시간이 끝나면 -> 손으로 복귀 모드 전환
            if (flightTimer > totalFlightTime + returnDelayBuffer)
            {
                ReturnToHand();
            }

            // ★ 수정됨: 거리 체크에 autoCatchDistance 변수 사용
            // 퍼포먼스 중에도 손을 뻗어 잡으면 즉시 잡히게 (인터셉트)
            if (distance < autoCatchDistance && currentGripStrength > grabThreshold)
            {
                Grab();
            }

            // ★ 수정됨: 자동 잡기 로직 (Auto Catch)
            // 조건: 거리 내 진입 + 발사 직후 아님(0.5초 지남)
            if (distance < autoCatchDistance && flightTimer > 0.5f)
            {
                Grab();
            }
        }
        // ---------------------------------------------------------
        // [상태 3] 바닥에 있거나 돌아오는 중일 때
        // ---------------------------------------------------------
        else
        {
            // ★ 수정됨: 거리 체크에 autoCatchDistance 변수 사용
            if (distance < autoCatchDistance)
            {
                if (!isPerforming || currentGripStrength > grabThreshold)
                {
                    Grab();
                }
            }
        }
    }

    void Grab()
    {
        isHeld = true;
        isPerforming = false;
        isReadyToLaunch = false; // 안전장치 ON

        swordRb.isKinematic = true;
        swordRb.useGravity = false;
        swordRb.velocity = Vector3.zero;
        swordRb.angularVelocity = Vector3.zero;

        // Debug.Log("⚔️ 잡았다!");
    }

    void StartPerformance()
    {
        isHeld = false;
        isPerforming = true;
        isReadyToLaunch = false;
        flightTimer = 0.0f;

        swordRb.isKinematic = false;
        swordRb.useGravity = true;

        // [높이]
        float gravity = Mathf.Abs(Physics.gravity.y);
        float jumpVelocity = Mathf.Sqrt(2 * gravity * targetHeight);

        // [시간]
        float timeToApex = jumpVelocity / gravity;
        totalFlightTime = timeToApex * 2.0f;

        // [발사]
        swordRb.velocity = Vector3.up * jumpVelocity;

        // [회전]
        float totalDegrees = 360f * spinCount;
        float totalRadians = totalDegrees * Mathf.Deg2Rad;
        float angularSpeed = totalRadians / totalFlightTime;

        swordRb.angularVelocity = transform.up * angularSpeed;

        Debug.Log($"🚀 퍼포먼스 시작! (자동 잡기 거리: {autoCatchDistance}m)");
    }

    void ReturnToHand()
    {
        swordRb.useGravity = false;

        Vector3 directionToHand = (rightHand.transform.position - transform.position).normalized;
        swordRb.velocity = directionToHand * returnPower;
    }

    void StickToHand()
    {
        transform.position = rightHand.transform.TransformPoint(positionOffset);
        transform.rotation = rightHand.transform.rotation * Quaternion.Euler(rotationOffset);
    }

    float GetMaxPinchStrength()
    {
        float maxStrength = 0.0f;
        maxStrength = Mathf.Max(maxStrength, rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Index));
        maxStrength = Mathf.Max(maxStrength, rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Middle));
        maxStrength = Mathf.Max(maxStrength, rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Ring));
        maxStrength = Mathf.Max(maxStrength, rightHand.GetFingerPinchStrength(OVRHand.HandFinger.Pinky));
        return maxStrength;
    }
}