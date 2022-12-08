using Ekkorr.Cook;
using Ekkorr.Cook.UI;
using Protocol.GameWebAndClient;
using Protocol.GameWebAndClient.GWC;
using Protocol.GameWebAndClient.SharedDataModels;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SiriusQuizRewardListItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtAmount;
    [SerializeField] private TextMeshProUGUI txtProgressCount;
    [SerializeField] private TextMeshProUGUI txtReceiveBtn;
    [SerializeField] private TextMeshProUGUI txtDisableBtn;
    [SerializeField] private Image icon;
    [SerializeField] private GameObject receiveBtnObj;
    [SerializeField] private GameObject disableBtnObj;
    [SerializeField] private GameObject completeObj;
    public Action<int> OnClickRewardItem;
    private EventInfo EventData;
    private EventRewardInfo EventRewardData;
    private int templateId;
    private int step;
    private int Count;
    private int Index;
    private int RightCount;
    public void Init(EventRewardInfo EventRewardData, List<QuizOfSiriusEventQuizData> QuizDatas, int Index)
    {
        this.Index = Index;
        Count = Index + 1;
        RightCount = CountingCorrect(QuizDatas);
        this.EventRewardData = EventRewardData;
        Refresh(EventRewardData);

        templateId = EventRewardData.templateId;
        step = EventRewardData.step;
        this.icon.sprite = EventRewardData.icon;

        txtProgressCount.text = StringsHelper.GetMessage(12090, Count);
        txtAmount.text = $"x{CommonControl.ParseProperty(EventRewardData.amount)}";
        completeObj.SetActive(EventRewardData.state == EventRewardInfo.State.COMPLETE);     
        SetTranslation();

    }

    public void Refresh(EventRewardInfo eventRewardData)
    {
        if (RightCount >= Count)
        {
            receiveBtnObj.SetActive(true);
            disableBtnObj.SetActive(false);
        }
        else if (RightCount < Count)
        {
            disableBtnObj.SetActive(true);
            receiveBtnObj.SetActive(false);
        }
        completeObj.SetActive(eventRewardData.state == EventRewardInfo.State.COMPLETE);

        if (eventRewardData.state == EventRewardInfo.State.COMPLETE)
        {
            disableBtnObj.SetActive(false);
            receiveBtnObj.SetActive(false);
        }    
    }

    private int CountingCorrect(List<QuizOfSiriusEventQuizData> QuizDatas)
    {
        
        int Count = 0;
        for (int j = 0; j < SiriusQuizControl.MyChoiceAnswer.Length; j++)
        {
            if (QuizDatas[j].Status == SystemEnums.QuizOfSiriusQuizStatus.Correct)
                Count++;
        }
        return Count;
    }

    private void SetTranslation()
    {
        txtReceiveBtn.text = txtDisableBtn.text = StringsHelper.GetMessage(10252);
    }

    public void OnShowTooltip()
    {
        ItemTooltipControl.ShowTooltip(EventRewardData.tooltipTitle, EventRewardData.tooltipDesc, icon.transform);
    }

    public void AddReceiveReward(Action<int> getReawrdItem)
    {
        OnClickRewardItem -= getReawrdItem;
        OnClickRewardItem += getReawrdItem;
    }

    public void getRewardItem()
    {
        OnClickRewardItem?.Invoke(step);
    }
}
