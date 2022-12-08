
using ClientDataContainer;
using Ekkorr.Cook;
using Ekkorr.Cook.UI;
using Protocol.GameWebAndClient;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BingoRewardListItem : MonoBehaviour
{ 
    [SerializeField] private TextMeshProUGUI txtCount;
    [SerializeField] private Image icon;
    [SerializeField] private GameObject FinishedObj;
    [SerializeField] private GameObject GiveRewardObj;
    [SerializeField] public ParticleSystem ParicleSys;
    SPEventBingoRewardTemplate TemplateData;
    private int Index;
    public SystemEnums.EventMissionStatus Status;
    public void Init(SPEventBingoRewardTemplate templateData, int index, SystemEnums.EventMissionStatus status)
    {
        TemplateData = templateData;
        Index = index;
        txtCount.text = TemplateData.RewardStringIdRef[Index].ValueMax.ToString();
        Status = status;
        SetImage();
        SetClear();
    }

    private void SetImage()
    {
        icon.sprite = AtlasControl.GetRewardIcon(TemplateData.RewardStringIdRef[Index]);
    }
    private void SetClear()
    {
        FinishedObj.SetActive(Status.Equals(SystemEnums.EventMissionStatus.Finished));
        GiveRewardObj.SetActive(Status.Equals(SystemEnums.EventMissionStatus.GiveReward));
    }
    public void Dispose()
    {
        ParicleSys.Stop();
    }
    public void OnShowTooltip()
    {
        ItemTooltipControl.ShowTooltip(TemplateData.RewardStringIdRef[Index], icon.transform);
    }
}
