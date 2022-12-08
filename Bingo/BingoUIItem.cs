namespace Ekkorr.Cook
{
    using Ekkorr.Cook.UI;
    using System.Collections;
    using System.Collections.Generic;
    using ClientDataContainer;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class BingoUIItem : MonoBehaviour
    {
        [SerializeField] Image BackGroundImage;
        [SerializeField] Image DishImage;
        [SerializeField] GameObject CompleteDim;
        [SerializeField] GameObject Layout;
        [SerializeField] TextMeshProUGUI Count;

        private int Index;
        private BingoItemData BingoData;
        public bool isSelected = false;
        //private bool isCompleted = false;
        // private  스테이트에따른 상태 변화 체크.

        public void init(BingoItemData bingoData,int index)
        {
            BingoData = bingoData;
            Index = index;
            
            SetImage();
            UpdateLayout();
            SetTranslation();
            
        }
        public void UpdateUIItem(BingoItemData bingoData, int index)
        {
            BingoData = bingoData;
            Index = index;
            UpdateLayout();
            SetTranslation();
        }
        public void Dispose()
        {
            isSelected = false;
            UpdateLayout();
        }

        public void DataUpdate()
        {
            SetTranslation();
        }
        public void SetTranslation()
        {
            if (ItemControl.GetAmount(BingoData.StringID) < 1)
                Count.text = StringsHelper.GetMessage(11171, ItemControl.GetAmount(BingoData.StringID), BingoData.DishDemandCount);
            else
                Count.text = StringsHelper.GetMessage(12101, ItemControl.GetAmount(BingoData.StringID), BingoData.DishDemandCount);
        }
        private void SetImage()
        {
            BackGroundImage.sprite = BingoData.BackGroundImage;
            DishImage.sprite = BingoData.DishImage;
            UpdateData();
        }
        public void UpdateLayout()
        {
            Layout.SetActive(isSelected);
        }

        public void UpdateData()
        {
            if (BingoData.BingoItemState == Protocol.GameWebAndClient.SystemEnums.EventMissionStatus.Finished)
                CompleteDim.SetActive(true);
            else
                CompleteDim.SetActive(false);
        }

        public void ItemBtnClick()
        {
            BingoControl.BingoEvent.SubmitBox.DisposeSubmitBox();
            BingoControl.NowBingoItemSelectedIndex = Index;
            BingoControl.SetDishesLayOut();
            UpdateLayout();
            BingoControl.DishSubmit(Index);

        }
    }
}