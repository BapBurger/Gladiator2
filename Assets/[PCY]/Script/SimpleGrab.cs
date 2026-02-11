using UnityEngine;

public class SimpleGrab : MonoBehaviour
{
    [Header("설정")]
    public OVRHand rightHand;       // OVRHand 스크립트가 있는 오브젝트 (손 제스처 감지용)
    public Rigidbody swordRb;       // 칼의 리지드바디

    [Header("잡기 옵션")]
    public float grabDistance = 0.2f; // 손과 칼이 이 거리 안에 있어야 잡힘
    public float throwPower = 1.5f;   // 던질 때 힘 배율

    // 내부 변수
    private bool isGrabbing = false;
    private Vector3 lastHandPos;
    private Vector3 handVelocity;

    void Start()
    {
        // 리지드바디 자동 찾기
        if (swordRb == null) swordRb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 1. 손이 없으면 아무것도 안 함
        if (rightHand == null) return;

        // 2. 손의 속도 계산 (던질 때 쓰려고 매 프레임 계산)
        // (현재 위치 - 이전 위치) / 시간 = 속도
        handVelocity = (rightHand.transform.position - lastHandPos) / Time.deltaTime;
        lastHandPos = rightHand.transform.position;

        // 3. 핀치(검지+엄지 꼬집기) 감지
        // GetFingerIsPinching(검지)가 True면 잡고 있는 것
        bool isPinching = rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index);

        // 4. 거리 계산 (칼과 손 사이의 거리)
        float distance = Vector3.Distance(transform.position, rightHand.transform.position);

        // [로직] 잡기 시도
        if (isPinching && !isGrabbing)
        {
            // 거리가 가까울 때만 잡기 허용
            if (distance <= grabDistance)
            {
                Grab();
            }
        }
        // [로직] 놓기 (잡고 있었는데 핀치를 풀었을 때)
        else if (!isPinching && isGrabbing)
        {
            Release();
        }

        // [상태] 잡고 있는 동안 위치 고정
        if (isGrabbing)
        {
            // 칼을 손 위치로 계속 이동 (물리 무시)
            transform.position = rightHand.transform.position;
            transform.rotation = rightHand.transform.rotation;

            // 칼날 방향을 맞추고 싶다면 아래처럼 회전값 추가 조정 가능
            // transform.Rotate(Vector3.right * 90f); 
        }
    }

    void Grab()
    {
        isGrabbing = true;

        // 물리를 끕니다 (손에 붙어야 하니까)
        swordRb.isKinematic = true;
        swordRb.useGravity = false;

        // 손 위치로 순간이동
        transform.position = rightHand.transform.position;
    }

    void Release()
    {
        isGrabbing = false;

        // 물리를 다시 켭니다 (떨어져야 하니까)
        swordRb.isKinematic = false;
        swordRb.useGravity = true;

        // [핵심] 손의 속도를 칼에 그대로 전달 (던지기)
        swordRb.velocity = handVelocity * throwPower;

        // 회전력도 조금 주면 리얼함 (선택)
        swordRb.angularVelocity = Vector3.right * 5f;
    }
}