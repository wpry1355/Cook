namespace Ekkorr.Cook
{
    using Ekkorr.Cook.UI;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class BingoEventUI : EventAbstractContentUI
    {

        [SerializeField] public GameObject NotAvailablePanel;
        [SerializeField] public GameObject BingoPanel;
        [SerializeField] public Button PrevBtn;
        [SerializeField] public Button NextBtn;
        [SerializeField] public Button InfoBtn;
        [SerializeField] public Button ReceiveBtn;
        [SerializeField] public GameObject ReceiveBtnDim;
        [SerializeField] public List<BingoUIItem> BingoUIItemList;
        [SerializeField] public List<BingoRewardListItem> BingoRewardUIList;
        [SerializeField] public List<BingoIndexBar> IndexBar;
        [SerializeField] public GameObject prefNotiUI;

        //Text
        [SerializeField] TextMeshProUGUI TitleText;
        [SerializeField] TextMeshProUGUI DescText;
        [SerializeField] TextMeshProUGUI SubTitleText;
        [SerializeField] TextMeshProUGUI RewardReceiveBtnText;
        [SerializeField] TextMeshProUGUI RewardReceiveBtnDimText;

        //DeliveryBox
        [SerializeField] public BingoSubmitUI SubmitBox;

        
        public override void Init(int tabIndex)
        {
            base.Init(tabIndex);
            index = tabIndex;
            BingoControl.BingoEvent = this;
            BingoControl.Init();
            SetTranstion();
            NotAvailablePanel.SetActive(false);
            SubmitBox.DisposeSubmitBox();
        }
        public override void Show()
        {
            base.Show();
            BingoControl.UpdateData();
            BingoControl.EventId = PlayerPrefs.GetString(BingoControl.BingoEventIdKey);
            SetTabNoti(BingoControl.TabNoti());
            AddBtnClickEvent();
        }
        public override void Hide()
        {
            base.Hide();
            Dispose();
           
        }
        public override void Final()
        {
            base.Final();
            Dispose();
            BingoControl.Final();
        }
        public void Dispose()
        {
            PrevBtn.onClick.RemoveAllListeners();
            NextBtn.onClick.RemoveAllListeners();
            SubmitBox.DisposeSubmitBox();
            BingoControl.Dispose();
        }
        public void SetTabNoti(bool isOn)
        {
            tabUI.SetNoti(isOn);
           
        }
       
        private void SetTranstion()
        {
            TitleText.text = StringsHelper.GetMessage(12099);
            DescText.text = StringsHelper.GetMessage(12100);
            SubTitleText.text = StringsHelper.GetMessage(12107);
            RewardReceiveBtnText.text = StringsHelper.GetMessage(10166);
            RewardReceiveBtnDimText.text = StringsHelper.GetMessage(10166);
        }

        private void AddBtnClickEvent()
        {
            PrevBtn.onClick.AddListener(BingoControl.SetPrevBtnEvent);
            NextBtn.onClick.AddListener(BingoControl.SetNextBtnEvent);
        }


        public void RewardBtnClick()
        {
            BingoControl.OnReceiveRewardAsync();
        }
        public void OnClickInfoBtn()
        {
            FacilityInformationControl.ShowFacilityInformationUI(E_FacilityInformationType.Bingo);
        }

        public void SetReceiveBtnDim(bool isActive)
        {
            ReceiveBtnDim.SetActive(isActive);
            ReceiveBtn.interactable = !isActive;
        }
    }
}
