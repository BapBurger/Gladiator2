using UnityEngine;

public class PerformanceSword : MonoBehaviour
{
    [Header("1. 필수 연결")]
    public OVRHand rightHand;
    public Rigidbody swordRb;

    [Header("2. 동작 설정 (건들지 않아도 됨)")]
    public float targetHeight = 0.5f;   // 목표 높이 (50cm)
    public int spinCount = 3;           // 회전 횟수 (3바퀴)
    public float returnDelayBuffer = 0.1f; // 내려오기 시작하고 손으로 오기 전 약간의 딜레이

    [Header("3. 복귀 설정")]
    public float returnPower = 15.0f;    // 손으로 돌아오는 속도

    [Header("4. 위치 보정")]
    public Vector3 positionOffset;
    public Vector3 rotationOffset;

    [Header("5. 감도 설정")]
    [Range(0, 1)] public float grabThreshold = 0.8f;   // 잡는 기준
    [Range(0, 1)] public float releaseThreshold = 0.3f; // 놓는 기준 (이 값 이하로 펴면 바로 발동)

    // 내부 변수
    private bool isHeld = false;
    private bool isPerforming = false; // 공중제비 도는 중인가?
    private float flightTimer = 0.0f;
    private float totalFlightTime = 0.0f;

    void Start()
    {
        if (swordRb == null) swordRb = GetComponent<Rigidbody>();

        // 초기화
        isHeld = false;
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

            // 조건: 그냥 손만 펴면(Release Threshold 이하) 바로 발동!
            // 속도 체크(minThrowSpeed)를 아예 없앴습니다.
            if (currentGripStrength < releaseThreshold)
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

            // 체류 시간이 끝나면 (즉, 올라갔다 내려오면) -> 손으로 복귀 모드 전환
            if (flightTimer > totalFlightTime + returnDelayBuffer)
            {
                ReturnToHand();
            }

            // 퍼포먼스 중에도 손을 뻗어 잡으면 즉시 잡히게
            if (distance < 0.2f && currentGripStrength > grabThreshold)
            {
                Grab();
            }
        }
        // ---------------------------------------------------------
        // [상태 3] 바닥에 있거나 돌아오는 중일 때
        // ---------------------------------------------------------
        else
        {
            // 잡기 시도
            if (distance < 0.2f && (currentGripStrength > grabThreshold || isPerforming == false)) // 돌아올 땐 자동 잡기
            {
                Grab();
            }
        }
    }

    void Grab()
    {
        isHeld = true;
        isPerforming = false;

        swordRb.isKinematic = true;
        swordRb.useGravity = false;
        swordRb.velocity = Vector3.zero;
        swordRb.angularVelocity = Vector3.zero;

        // Debug.Log("잡았다!");
    }

    void StartPerformance()
    {
        isHeld = false;
        isPerforming = true;
        flightTimer = 0.0f;

        swordRb.isKinematic = false;
        swordRb.useGravity = true; // 중력 가속도를 받아야 하므로 중력 켬!

        // [물리학 계산] 
        // 1. 목표 높이(0.5m)까지 올라가는 데 필요한 속도 구하기 (v = sqrt(2gh))
        float gravity = Mathf.Abs(Physics.gravity.y);
        float jumpVelocity = Mathf.Sqrt(2 * gravity * targetHeight);

        // 2. 공중에 머무는 시간 계산 (올라갈 때 시간 * 2)
        // t = v / g
        float timeToApex = jumpVelocity / gravity;
        totalFlightTime = timeToApex * 2.0f; // 올라갔다 내려오는 총 시간

        // 3. 위로 쏘아 올리기 (수직 상승)
        swordRb.velocity = Vector3.up * jumpVelocity;

        // 4. 회전 계산 (초록색 축 = Y축)
        // 총 체류 시간 동안 spinCount만큼 돌려면?
        // 각속도(AngularVelocity)는 라디안 단위입니다.
        // 3바퀴 = 360 * 3 = 1080도
        float totalDegrees = 360f * spinCount;
        float totalRadians = totalDegrees * Mathf.Deg2Rad;
        float angularSpeed = totalRadians / totalFlightTime;

        // Y축(초록색)으로 회전력 적용
        swordRb.angularVelocity = new Vector3(0, angularSpeed, 0);

        // Debug.Log($"퍼포먼스 시작! 예상 체류시간: {totalFlightTime}초");
    }

    void ReturnToHand()
    {
        // 중력 끄고 손으로 직행
        swordRb.useGravity = false;

        Vector3 directionToHand = (rightHand.transform.position - transform.position).normalized;
        swordRb.velocity = directionToHand * returnPower;

        // 돌아올 때는 회전 멈추거나 자연스럽게
        // swordRb.angularVelocity = Vector3.zero; 
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