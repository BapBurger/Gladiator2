using UnityEngine;

public class SwordReturn : MonoBehaviour
{
    [Header("1. 리셋 버튼 설정")]
    // Button.One은 오른손의 'A' 또는 왼손의 'X' 버튼입니다.
    public OVRInput.Button returnButton = OVRInput.Button.One;
    public OVRInput.Controller controller = OVRInput.Controller.RTouch; // 오른손 컨트롤러 기준

    [Header("2. 설정 확인 (자동)")]
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Rigidbody rb;

    void Start()
    {
        // 게임 시작 시점의 칼 위치와 회전값을 기억해둠
        startPosition = transform.position;
        startRotation = transform.rotation;

        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // 설정한 버튼이 눌렸는지 감지 (GetDown: 누르는 순간 1회 발동)
        if (OVRInput.GetDown(returnButton, controller))
        {
            ResetSword();
        }
    }

    public void ResetSword()
    {
        // 1. 물리 속도 초기화 (날아가던 힘 제거)
        if (rb != null)
        {
            // rb.linearVelocity = Vector3.zero;  // 이동 속도 0 (Unity 6 이상)
            rb.velocity = Vector3.zero; // Unity 6 미만 구버전이면 위 주석 풀고 이거 쓰세요
            rb.angularVelocity = Vector3.zero; // 회전 속도 0
        }

        // 2. 위치와 회전을 처음 상태로 되돌림
        transform.position = startPosition;
        transform.rotation = startRotation;

        Debug.Log("칼이 초기 위치로 복귀했습니다!");
    }
}