namespace Ekkorr.Cook{
    using Protocol.GameWebAndClient;
    using Protocol.GameWebAndClient.SharedDataModels;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using ClientDataContainer;
    using Ekkorr.Cook.UI;

    public class BingoItemData
    {
        public string StringID;
        public string DishName;      
        public int DishAmount;
        public int DishDemandCount;
        public int DishTemplateId;
        public Sprite DishImage;
        public Sprite BackGroundImage;
        private RecipeTier DishTier;
        public SystemEnums.EventMissionStatus BingoItemState;
        private DishesTemplate TemplateData;
       
        public void init(string stringID, string dishName, int dishAmount, int dishDemandCount, RecipeTier dishTier, SystemEnums.EventMissionStatus bingostate)
        {
            StringID = stringID;
            DishName = dishName;
            DishAmount = dishAmount;
            DishDemandCount = dishDemandCount;
            DishTier = dishTier;
            BingoItemState = bingostate;
            SetImage();
        }

        public void init(BingoEventCellData cellData)
        {
            DishTemplateId = cellData.DishTemplateId;
            TemplateData = TemplateContainer<DishesTemplate>.Find(DishTemplateId);
            StringID = TemplateData.StringId;
            DishName = TemplateContainer<ItemsTemplate>.Find(StringID).NameRef.Get();
            DishTier = TemplateData.Tier;
            DishAmount = ItemControl.GetAmount(StringID);
            DishDemandCount = SetDemandCount(cellData);
            SetImage();
            BingoItemState = cellData.CellStatus;
        }

        public void UpdateAccount()
        {
            DishAmount = ItemControl.GetAmount(StringID);
        }
            

        private int SetDemandCount(BingoEventCellData cellData)
        {
            switch (DishTier)
            {
                default:
                    return -1;
                case RecipeTier.Normal:
                    return TemplateContainer<ConstantsTemplate>.Find("BingoCommonNeedCount").Value;
                case RecipeTier.Rare:
                    return TemplateContainer<ConstantsTemplate>.Find("BingoRareNeedCount").Value;
                case RecipeTier.Epic:
                    return TemplateContainer<ConstantsTemplate>.Find("BingoEpicNeedCount").Value;
                case RecipeTier.Legendary:
                    return TemplateContainer<ConstantsTemplate>.Find("BingoLegendaryNeedCount").Value;
            }
        }
        private void SetImage()
        {
            BackGroundImage = AtlasControl.GetBackgroundOfTier(DishTier);
            DishImage = AtlasControl.GetItem(StringID);
        }
    }
}