using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System;

public class RecordUserJND : MonoBehaviour
{
    [SerializeField]private TrialSetting trialSetting;
    
    //User 값 기록
    public List<int> CheckLonger = new List<int> {0,0,0,0,0,0};
    public List<int> CheckShorter = new List<int> {0,0,0,0,0,0};
    [SerializeField]private GameObject toggle;
    public void UpdateRecord(int value) //value = 0 ->  작음
    {
        int level = (trialSetting.L0 - trialSetting.currentLength)/trialSetting.delta_L;
        switch(level)
        {
            case -3: 
                if(value ==0 ) CheckShorter[0]++;
                else CheckLonger[0]++;
                break;
            case -2: 
                if(value ==0 ) CheckShorter[1]++;
                else CheckLonger[1]++;
                break;
            case -1: 
                if(value ==0 ) CheckShorter[2]++;
                else CheckLonger[2]++;
                break;
            case 1: 
                if(value ==0 ) CheckShorter[3]++;
                else CheckLonger[3]++;
                break;
            case 2: 
                if(value ==0 ) CheckShorter[4]++;
                else CheckLonger[4]++;
                break;
            case 3: 
                if(value ==0 ) CheckShorter[5]++;
                else CheckLonger[5]++;
                break;
            default:
                break;
        }
        if(trialSetting.sequenceIndex == trialSetting.trialSequence.Count)
        {
            SaveRecordToCSV();
        }
        toggle.SetActive(false);
        //만약 자동으로 남은 trial 실행하려면
        /*
        trialSetting.StartTrial();
        */
    }

    //Excel 저장
    [Header("사용자 정보")]
    public string userName;
    public string rootPath;
    public void SaveRecordToCSV()
    {
        string coord = trialSetting.selectedCoord switch
        {
            TrialSetting.CoordType.X => "X",
            TrialSetting.CoordType.Y => "Y",
            TrialSetting.CoordType.Z => "Z"
        };

        string filePath = Path.Combine(rootPath, $"{userName}_{coord}.csv");

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("Length,CheckLonger,CheckShorter");

        for (int i = 0; i < 6; i++)
        {
            int length;
            if (i < 3)
                length = (int)(trialSetting.L0 - ((3 - i) * trialSetting.delta_L));
            else
                length = (int)(trialSetting.L0 + ((i - 2) * trialSetting.delta_L));

            string row = $"{length},{CheckLonger[i]},{CheckShorter[i]}";
            sb.AppendLine(row);
        }

        try
        {
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
            Debug.Log($"데이터가 전치되어 저장되었습니다: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"파일 저장 중 오류 발생: {e.Message}");
        }
    }
}
