using UnityEngine;

// �� ���� �̸��� NewSword�� �ٲ��� ���� �̸��� ��Ī�˴ϴ�!
public class NewSword : MonoBehaviour
{
    [Header("1. �ʼ� ����")]
    public Transform handBone;        // RightHandAnchor (�ڵ����� ã��)
    public string handTag = "Player"; // �տ� �޾��� �±� �̸�

    [Header("2. ��ġ/ȸ�� ������")]
    public Vector3 positionOffset;
    public Vector3 rotationOffset;

    [Header("3. ���� ����")]
    public float throwThreshold = 2.0f;
    public float throwForceMultiplier = 1.2f;
    public float airResistance = 0.98f;           // ���� ����
    public float gravityMultiplier = 1.0f;        // �߷� ����
    public float returnForce = 5.0f;              // ���� ���ƴ翡�� ��
    public float returnForceDistance = 2.0f;      // ���� ���ƴ翡�� �� �ŷ��� �Ÿ�
    public float spinSpeed = 720.0f;              // Y�� ȸ�� �ӵ� (��/��)
    public float catchDistance = 0.5f;

    [Header("���� ���� ǥ��")]
    [Tooltip("Į�� ���� �ִ� ����")]
    public bool isHeld = false;
    [Tooltip("Į�� ���� ���� �ִ� ����")]
    public bool isFlying = false;
    [Tooltip("���� ���� �ӵ�")]
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
        // Y�� (�ʷϻ� ��) �⺻���� ȸ��
        transform.Rotate(Vector3.up * spinSpeed * Time.deltaTime, Space.Self);

        // ���� ����
        currentVelocity *= airResistance;

        // �߷� ����: ���� �Ʒ��� (-Y ����)
        currentVelocity += Physics.gravity * gravityMultiplier * Time.deltaTime;

        // ���ڷ� ���ƴ翡�� �� (��ġ�� ����)
        Vector3 directionToHand = (handBone.position - transform.position);
        float distance = directionToHand.magnitude;

        // �Ÿ��� ���� �ݺ��Ͽ� ���ƴ翡�� �� ����
        // ����� �Ÿ��� ������ ���� ���� ����
        if (distance > returnForceDistance)
        {
            float returnStrength = Mathf.Pow(distance - returnForceDistance, 1.5f);
            currentVelocity += directionToHand.normalized * returnForce * returnStrength * Time.deltaTime;
        }

        // ��ġ ������Ʈ
        transform.position += currentVelocity * Time.deltaTime;

        // �پ� ������ �ڵ� ����
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