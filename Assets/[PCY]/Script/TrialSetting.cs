using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrialSetting : MonoBehaviour
{
    //Coord 설정
    public enum CoordType { X, Y, Z }

    [Header("Coord 설정")]
    [Tooltip("드롭다운에서 하나를 선택하세요.")]
    public CoordType selectedCoord;
    private CoordType pastCoord;

    //Length 랜덤화
    [Header("Length 설정")]
    public int L0 = 40;
    public int delta_L = 5;
    public int maxLengthTrial = 5;
    public List<int> trialSequence = new List<int>();

    public void CreateTrialSeq()
    {
        List<int> trialSeq = new List<int>();
        for (int i = -3; i < 4; i++)
        {
            if (i == 0) continue;
            for (int j = 0; j < maxLengthTrial; j++)
            {
                trialSeq.Add(L0 + i * delta_L);
            }
        }

        trialSeq = trialSeq.OrderBy(x => Random.value).ToList();
        Debug.Log("trial sequence: " + string.Join(", ", trialSeq));
        trialSequence = trialSeq;
    }

    //Trial 시작
    [Header("Trial/Interval 조작")]
    public bool isBaseline = true;
    public bool isOngoingSession = false;
    public int sequenceIndex = 0;
    public int currentLength;

    [SerializeField] private GameObject toggle;

    // ====== Rod GameObjects (Inspector에 연결) ======
    [Header("Rod Objects")]
    public GameObject Rod_x_Ref;
    public GameObject Rod_x_Unkn;
    public GameObject Rod_y_Ref;
    public GameObject Rod_y_Unkn;
    public GameObject Rod_z_Ref;
    public GameObject Rod_z_Unkn;

    private GameObject[] allRods;

    private void Awake()
    {
        allRods = new GameObject[]
        {
            Rod_x_Ref, Rod_x_Unkn,
            Rod_y_Ref, Rod_y_Unkn,
            Rod_z_Ref, Rod_z_Unkn
        };
        UpdateRodVisibility();
    }

    private void SetOnlyActive(GameObject target)
    {
        // target만 true, 나머지는 false (target이 null이면 전부 false)
        foreach (var go in allRods)
        {
            if (go == null) continue;
            go.SetActive(go == target);
        }
    }

    private GameObject GetTargetRod()
    {
        // 세션 아니면 전부 off
        if (!isOngoingSession) return null;

        // 요구사항 그대로:
        // isOngoingSession만 true(=isBaseline false) -> Ref
        // isBaseline & isOngoingSession true -> Unkn
        bool showUnknown = isBaseline;

        switch (selectedCoord)
        {
            case CoordType.X: return showUnknown ? Rod_x_Unkn : Rod_x_Ref;
            case CoordType.Y: return showUnknown ? Rod_y_Unkn : Rod_y_Ref;
            case CoordType.Z: return showUnknown ? Rod_z_Unkn : Rod_z_Ref;
            default: return null;
        }
    }

    private void UpdateRodVisibility()
    {
        SetOnlyActive(GetTargetRod());
    }

    public void StartTrial()
    {
        if (sequenceIndex == trialSequence.Count) return;

        isOngoingSession = true;

        if (isBaseline)
        {
            currentLength = L0;
            Debug.Log("Start L0 session");
            isBaseline = false; // 다음은 unknown
        }
        else
        {
            currentLength = trialSequence[sequenceIndex];
            Debug.Log($"Start {trialSequence[sequenceIndex]}!");
            isBaseline = true;  // 다음은 baseline
            sequenceIndex++;
        }

        UpdateRodVisibility();
    }

    public void StopTrial()
    {
        Debug.Log("Stop Trial");
        isOngoingSession = false;

        // isBaseline은 "다음 interval이 baseline인지" 플래그라는 기존 주석 유지
        if (isBaseline)
        {
            //Show UI
            if (toggle != null) toggle.SetActive(true);
        }

        UpdateRodVisibility(); // 세션 종료 -> 전부 off
    }

    void Start()
    {
        CreateTrialSeq();
        UpdateRodVisibility();
    }

    private void Update()
    {
        //Coord 변경 감지
        if (pastCoord != selectedCoord)
        {
            switch (selectedCoord)
            {
                case CoordType.X: Debug.Log(selectedCoord); break;
                case CoordType.Y: Debug.Log(selectedCoord); break;
                case CoordType.Z: Debug.Log(selectedCoord); break;
            }
            pastCoord = selectedCoord;

            UpdateRodVisibility(); // coord 바뀌면 표시도 즉시 갱신
        }

        //Start/Stop trial manually
        if (Input.GetKeyDown(KeyCode.S) && !isOngoingSession)
        {
            StartTrial();
        }
        if (Input.GetKeyDown(KeyCode.K) && isOngoingSession)
        {
            StopTrial();
        }
    }
}
