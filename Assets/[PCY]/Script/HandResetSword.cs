using UnityEngine;

public class HandResetSword : MonoBehaviour
{
    [Header("1. 왼손 연결 (필수)")]
    public OVRHand leftHand; // 여기에 LeftHandAnchor 안의 OVRHandPrefab을 넣으세요

    [Header("2. 제스처 설정")]
    // 기본값: 검지(Index) 핀치. 원하면 중지나 약지로 변경 가능
    public OVRHand.HandFinger resetFinger = OVRHand.HandFinger.Index;

    // 내부 변수
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;
    private bool wasPinching = false; // 핀치 상태 기억용 (중복 실행 방지)

    void Start()
    {
        // 시작 위치 기억
        startPosition = transform.position;
        startRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 왼손이 없으면 아무것도 안 함
        if (leftHand == null) return;

        // 1. 왼손이 핀치 상태인지 확인
        bool isPinching = leftHand.GetFingerIsPinching(resetFinger);

        // 2. [핵심] "방금 막 핀치를 했고(isPinching)", "직전엔 안 했었다면(!wasPinching)" -> 실행
        // (이렇게 안 하면 핀치하고 있는 동안 칼이 제자리에 멈춰서 덜덜거림)
        if (isPinching && !wasPinching)
        {
            ResetSword();
        }

        // 현재 상태를 저장 (다음 프레임 비교용)
        wasPinching = isPinching;
    }

    void ResetSword()
    {
        // 물리 속도 초기화 (날아가던 힘 0으로)
        if (rb != null)
        {
            rb.velocity = Vector3.zero;        // 이동 멈춤
            rb.angularVelocity = Vector3.zero; // 회전 멈춤
        }

        // 위치 복귀
        transform.position = startPosition;
        transform.rotation = startRotation;

        Debug.Log("왼손 제스처로 칼 소환 완료!");
    }
}