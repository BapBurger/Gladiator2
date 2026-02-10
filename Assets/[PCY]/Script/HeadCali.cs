using UnityEngine;

public class HeadCali : MonoBehaviour
{
    [Header("GameObject")]
    public Transform targetObject;
    public Transform userHead; 
    public Transform ovrCameraRig;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            AlignYawAndPosition();
        }
    }

    void AlignYawAndPosition()
    {

        Vector3 userHeadWorldPos = userHead.position;
        float userHeadYaw = userHead.eulerAngles.y;

        Vector3 targetWorldPos = targetObject.position;
        float targetYaw = targetObject.eulerAngles.y;

        float yawDelta = Mathf.DeltaAngle(userHeadYaw, targetYaw);
        ovrCameraRig.Rotate(0f, yawDelta, 0f, Space.World);

        Vector3 newUserHeadPos = userHead.position;
        Vector3 positionDelta = targetWorldPos - newUserHeadPos;

        ovrCameraRig.position += positionDelta;
    }
}
