using UnityEngine;

public class KnifeThrow : MonoBehaviour
{
    [Header("Knife State")]
    [Tooltip("칼이 손에 붙어있는지 여부 (True: 손에 있음, False: 공중에 있음)")]
    public bool isAttached = true;

    [Header("Hand References")]
    [Tooltip("칼을 던지는 손 Transform (예: OVRHand or Controller)")]
    public Transform handTransform;

    [Tooltip("칼이 돌아올 타겟 위치 (손바닥 중심)")]
    public Transform handTargetPoint;

    [Header("Throw Settings")]
    [Tooltip("던지기 입력 키")]
    public KeyCode throwKey = KeyCode.Space;

    [Tooltip("속도 증폭 계수 (손 속도에 곱해짐)")]
    public float velocityMultiplier = 2.0f;

    [Tooltip("최소 던지기 속도 (너무 약하게 던지는 것 방지)")]
    public float minThrowVelocity = 1.0f;

    [Header("Physics Settings")]
    [Tooltip("중력 가속도 (위로 갈수록 감속, 내려올수록 가속)")]
    public float gravity = 9.8f;

    [Tooltip("공기 저항 계수")]
    public float airResistance = 0.05f;

    [Header("Rotation Settings")]
    [Tooltip("비행 중 회전 속도 (도/초)")]
    public float rotationSpeed = 360.0f;

    [Tooltip("회전 축 (기본: Y축 = 초록색)")]
    public Vector3 rotationAxis = Vector3.up;

    [Header("Return Settings")]
    [Tooltip("손으로 돌아올 때 잡히는 거리")]
    public float catchDistance = 0.15f;

    [Tooltip("자동으로 손으로 돌아오는 인력 강도 (최고점 이후)")]
    public float returnForce = 5.0f;

    [Header("Debug")]
    public bool showDebugInfo = true;

    // Private variables
    private Vector3 knifeVelocity = Vector3.zero;
    private Vector3 lastHandPosition;
    private Vector3 handVelocity;
    private Quaternion attachedRotationOffset;
    private Vector3 attachedPositionOffset;
    private float flightTime = 0f;
    private bool hasReachedPeak = false;

    void Start()
    {
        if (handTransform == null)
        {
            Debug.LogError("KnifeThrow: handTransform이 설정되지 않았습니다!");
        }

        if (handTargetPoint == null)
        {
            handTargetPoint = handTransform;
            Debug.LogWarning("KnifeThrow: handTargetPoint가 설정되지 않아 handTransform을 사용합니다.");
        }

        // 초기 손에 붙어있을 때의 상대 위치/회전 저장
        if (isAttached && handTransform != null)
        {
            attachedPositionOffset = transform.position - handTransform.position;
            attachedRotationOffset = Quaternion.Inverse(handTransform.rotation) * transform.rotation;
        }

        lastHandPosition = handTransform != null ? handTransform.position : Vector3.zero;
    }

    void Update()
    {
        // 손 속도 계산 (던지기 위해 필요)
        if (handTransform != null)
        {
            handVelocity = (handTransform.position - lastHandPosition) / Time.deltaTime;
            lastHandPosition = handTransform.position;
        }

        // 던지기 입력 처리
        if (Input.GetKeyDown(throwKey) && isAttached)
        {
            ThrowKnife();
        }

        // 상태에 따른 업데이트
        if (isAttached)
        {
            UpdateAttachedState();
        }
        else
        {
            UpdateFlyingState();
        }

        // 디버그 정보 표시
        if (showDebugInfo)
        {
            DrawDebugInfo();
        }
    }

    void UpdateAttachedState()
    {
        // 칼을 손에 붙여놓기
        if (handTransform != null)
        {
            transform.position = handTransform.position + handTransform.rotation * attachedPositionOffset;
            transform.rotation = handTransform.rotation * attachedRotationOffset;
        }
    }

    void UpdateFlyingState()
    {
        flightTime += Time.deltaTime;

        // 1. 중력 적용 (Y축 방향으로만)
        knifeVelocity.y -= gravity * Time.deltaTime;

        // 2. 공기 저항
        knifeVelocity -= knifeVelocity * airResistance * Time.deltaTime;

        // 3. 최고점 도달 확인
        if (!hasReachedPeak && knifeVelocity.y <= 0)
        {
            hasReachedPeak = true;
        }

        // 4. 최고점 이후 손으로 돌아오는 인력 적용
        if (hasReachedPeak && handTargetPoint != null)
        {
            Vector3 directionToHand = handTargetPoint.position - transform.position;
            float distanceToHand = directionToHand.magnitude;

            // 손으로 끌어당기는 힘
            Vector3 returnAcceleration = directionToHand.normalized * returnForce * Time.deltaTime;
            knifeVelocity += returnAcceleration;

            // 손에 가까워지면 잡기
            if (distanceToHand < catchDistance)
            {
                CatchKnife();
                return;
            }
        }

        // 5. 위치 업데이트
        transform.position += knifeVelocity * Time.deltaTime;

        // 6. Y축 기준 회전 (빙글빙글)
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.World);
    }

    void ThrowKnife()
    {
        // 상태 변경
        isAttached = false;
        hasReachedPeak = false;
        flightTime = 0f;

        // 손의 속도를 기반으로 초기 속도 설정
        knifeVelocity = handVelocity * velocityMultiplier;

        // 최소 속도 보장 (위쪽 방향)
        if (knifeVelocity.magnitude < minThrowVelocity)
        {
            knifeVelocity = Vector3.up * minThrowVelocity;
        }

        if (showDebugInfo)
        {
            Debug.Log($"칼 던지기! 초기 속도: {knifeVelocity.magnitude:F2} m/s, 방향: {knifeVelocity.normalized}");
        }
    }

    void CatchKnife()
    {
        // 상태 변경
        isAttached = true;
        knifeVelocity = Vector3.zero;
        hasReachedPeak = false;
        flightTime = 0f;

        // 손 위치로 정확히 스냅
        if (handTransform != null)
        {
            transform.position = handTransform.position + handTransform.rotation * attachedPositionOffset;
            transform.rotation = handTransform.rotation * attachedRotationOffset;
        }

        if (showDebugInfo)
        {
            Debug.Log("칼 잡기 완료!");
        }
    }

    void DrawDebugInfo()
    {
        if (handTargetPoint != null)
        {
            // 손 타겟 포인트 표시
            Debug.DrawLine(transform.position, handTargetPoint.position, Color.yellow);

            // 속도 벡터 표시
            if (!isAttached)
            {
                Debug.DrawRay(transform.position, knifeVelocity, Color.red);
            }
        }
    }

    // Inspector에서 실시간 확인을 위한 Gizmos
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // 손에 있을 때: 초록색 구
        // 날아가고 있을 때: 빨간색 구
        Gizmos.color = isAttached ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.05f);

        // 잡기 범위 표시
        if (!isAttached && handTargetPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(handTargetPoint.position, catchDistance);
        }
    }

    // 외부에서 강제로 던지기 (VR 컨트롤러 등에서 호출 가능)
    public void ThrowKnifeWithVelocity(Vector3 velocity)
    {
        if (!isAttached) return;

        isAttached = false;
        hasReachedPeak = false;
        flightTime = 0f;
        knifeVelocity = velocity;

        if (showDebugInfo)
        {
            Debug.Log($"외부에서 칼 던지기! 속도: {velocity.magnitude:F2} m/s");
        }
    }

    // 외부에서 상태 확인 (다른 스크립트에서 사용 가능)
    public bool IsKnifeAttached()
    {
        return isAttached;
    }

    public Vector3 GetKnifeVelocity()
    {
        return knifeVelocity;
    }

    public float GetFlightTime()
    {
        return flightTime;
    }
}
