using Ekkorr.Cook;
using Ekkorr.Cook.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SiriusEraser : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI SiriusEraseText;
    [SerializeField] TextMeshProUGUI SiriusDesriptionText;
    [SerializeField] List<EventDailyActionRecordUIItem> DailyActionRecordData;

    public void init()
    {
        int i = 0;
        foreach (var Data in DailyActionRecordData)
        {
            Data.init(SiriusQuizControl.response.EventMissionDatas[i].DailyActionRecordTemplateId, SiriusQuizControl.response.EventMissionDatas[i].eventMissionStatus);
            i++;
        }
        SetTranslation();
    }

    private void SetTranslation()
    {
        SiriusEraseText.text = StringsHelper.GetMessage(12066);
        SiriusDesriptionText.text = StringsHelper.GetMessage(12067);
    }
}
