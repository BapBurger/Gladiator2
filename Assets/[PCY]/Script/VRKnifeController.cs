using UnityEngine;

/// <summary>
/// VR 컨트롤러와 KnifeThrow를 연결하는 스크립트
/// OVR 컨트롤러의 입력을 감지하고 손 속도를 추적합니다
/// </summary>
public class VRKnifeController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("KnifeThrow 스크립트가 붙은 칼 오브젝트")]
    public KnifeThrow knifeThrow;

    [Tooltip("VR 컨트롤러 Transform (OVRControllerHelper 또는 손 Transform)")]
    public Transform controllerTransform;

    [Header("Input Settings")]
    [Tooltip("던지기 버튼 (Oculus: PrimaryIndexTrigger = 트리거)")]
    public OVRInput.Button throwButton = OVRInput.Button.PrimaryIndexTrigger;

    [Tooltip("어느 손을 사용할지 (Left or Right)")]
    public OVRInput.Controller controllerHand = OVRInput.Controller.RTouch;

    [Header("Throw Detection")]
    [Tooltip("던지기로 인식할 최소 속도 (m/s)")]
    public float minThrowSpeed = 2.0f;

    [Tooltip("던지기로 인식할 최소 가속도 (m/s²)")]
    public float minThrowAcceleration = 5.0f;

    [Tooltip("속도 샘플링 횟수 (부드러운 속도 계산)")]
    public int velocitySampleCount = 5;

    [Header("Debug")]
    public bool showDebugInfo = true;

    // Private variables
    private Vector3[] velocityHistory;
    private int velocityHistoryIndex = 0;
    private Vector3 currentVelocity = Vector3.zero;
    private Vector3 lastPosition;
    private bool wasButtonPressed = false;

    void Start()
    {
        if (knifeThrow == null)
        {
            Debug.LogError("VRKnifeController: knifeThrow가 설정되지 않았습니다!");
        }

        if (controllerTransform == null)
        {
            Debug.LogError("VRKnifeController: controllerTransform이 설정되지 않았습니다!");
        }

        // 속도 히스토리 초기화
        velocityHistory = new Vector3[velocitySampleCount];
        lastPosition = controllerTransform != null ? controllerTransform.position : Vector3.zero;
    }

    void Update()
    {
        if (controllerTransform == null || knifeThrow == null) return;

        // 1. 속도 계산 및 히스토리 업데이트
        UpdateVelocity();

        // 2. 버튼 입력 감지
        bool isButtonPressed = OVRInput.Get(throwButton, controllerHand);
        bool buttonJustReleased = wasButtonPressed && !isButtonPressed;

        // 3. 버튼을 놓았을 때 던지기 판정
        if (buttonJustReleased && knifeThrow.IsKnifeAttached())
        {
            TryThrowKnife();
        }

        wasButtonPressed = isButtonPressed;

        // 4. 디버그 정보
        if (showDebugInfo)
        {
            DrawDebugInfo();
        }
    }

    void UpdateVelocity()
    {
        // 현재 속도 계산
        Vector3 instantVelocity = (controllerTransform.position - lastPosition) / Time.deltaTime;
        lastPosition = controllerTransform.position;

        // 히스토리에 추가
        velocityHistory[velocityHistoryIndex] = instantVelocity;
        velocityHistoryIndex = (velocityHistoryIndex + 1) % velocitySampleCount;

        // 평균 속도 계산 (부드럽게)
        Vector3 velocitySum = Vector3.zero;
        for (int i = 0; i < velocitySampleCount; i++)
        {
            velocitySum += velocityHistory[i];
        }
        currentVelocity = velocitySum / velocitySampleCount;
    }

    void TryThrowKnife()
    {
        float speed = currentVelocity.magnitude;

        // 던지기 조건 확인
        if (speed >= minThrowSpeed)
        {
            // 칼 던지기
            knifeThrow.ThrowKnifeWithVelocity(currentVelocity);

            if (showDebugInfo)
            {
                Debug.Log($"칼 던지기 성공! 속도: {speed:F2} m/s, 방향: {currentVelocity.normalized}");
            }

            // 햅틱 피드백 (진동)
            OVRInput.SetControllerVibration(0.5f, 0.3f, controllerHand);
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log($"던지기 속도 부족: {speed:F2} m/s (최소: {minThrowSpeed} m/s)");
            }
        }
    }

    void DrawDebugInfo()
    {
        if (controllerTransform != null)
        {
            // 속도 벡터 그리기
            Debug.DrawRay(controllerTransform.position, currentVelocity, Color.cyan);

            // 속도 크기 표시
            float speed = currentVelocity.magnitude;
            Color speedColor = speed >= minThrowSpeed ? Color.green : Color.red;
            Debug.DrawRay(controllerTransform.position, Vector3.up * 0.1f, speedColor);
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || controllerTransform == null) return;

        // 컨트롤러 위치 표시
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(controllerTransform.position, 0.03f);

        // 속도 벡터 표시
        Gizmos.color = currentVelocity.magnitude >= minThrowSpeed ? Color.green : Color.yellow;
        Gizmos.DrawRay(controllerTransform.position, currentVelocity * 0.1f);
    }

    // 공개 메서드: 외부에서 현재 속도 가져오기
    public Vector3 GetCurrentVelocity()
    {
        return currentVelocity;
    }

    public float GetCurrentSpeed()
    {
        return currentVelocity.magnitude;
    }
}
