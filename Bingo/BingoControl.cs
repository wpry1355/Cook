using Protocol.GameWebAndClient.SharedDataModels;

namespace Ekkorr.Cook
{
    using ClientDataContainer;
    using Ekkorr.Cook.Network;
    using Ekkorr.Cook.UI;
    using Protocol.GameWebAndClient;
    using Protocol.GameWebAndClient.CGW;
    using Protocol.GameWebAndClient.GWC;
    using System.Collections.Generic;
    using UnityEngine;


    public static class BingoControl
    {
        private static bool isInit = false;
        private static int DEFAULT_START_BINGOMAP_INDEX = 1000;
        private static int DEFAULT_BINGOMAP_MAX_INDEX = 1009;
        private static string UserName;
        public static int NowBingoIndex;
        public static int NowBingoItemSelectedIndex;
        public static string EventId = "";
        public static BingoEventUI BingoEvent;
        public static string BingoEventIdKey = "BingoEventId";
        public static List<string> BingoNoticeKeys = new List<string>
        {   "EventBingoNotice1", "EventBingoNotice2", "EventBingoNotice3",
            "EventBingoNotice4", "EventBingoNotice5", "EventBingoNotice6",
            "EventBingoNotice7", "EventBingoNotice8", "EventBingoNotice9",
            "EventBingoNotice10" };
        public static List<string> CompletedBingoMapKeys = new List<string>
        {   "EventCompletedBingoMapIndex1", "EventCompletedBingoMapIndex2", "EventCompletedBingoMapIndex3",
            "EventCompletedBingoMapIndex4", "EventCompletedBingoMapIndex5", "EventCompletedBingoMapIndex6",
            "EventCompletedBingoMapIndex7", "EventCompletedBingoMapIndex8", "EventCompletedBingoMapIndex9",
            "EventCompletedBingoMapIndex10" };

        public static List<int> BingoNoticeData = new List<int>();
        public static List<bool> CompleteBingoMapData = new List<bool>();
        public static List<BingoItemData> BingoData;

        

        public static void Init()
        {
            initLocalNoticeData();
            initCompleteBingoMapcheck();
            if (UserName != UserDataControl.LocalUserData.AccountName)
            {
                EventId = "";
                UserName = UserDataControl.LocalUserData.AccountName;
            }


            if (PlayerPrefs.HasKey(BingoEventIdKey) == true)
                EventId = PlayerPrefs.GetString(BingoEventIdKey);

            if (EventId != EventControl.EventInfoList[Protocol.GameWebAndClient.EventConditionType.Bingo].eventId)
            {
                EventId = EventControl.EventInfoList[Protocol.GameWebAndClient.EventConditionType.Bingo].eventId;
                PlayerPrefs.SetString(BingoEventIdKey, EventControl.EventInfoList[Protocol.GameWebAndClient.EventConditionType.Bingo].eventId);
                BingoEvent.SetTabNoti(true);
                foreach (var data in CompletedBingoMapKeys)
                {
                    PlayerPrefs.SetInt(data, 0);
                }
            }
            else
            {
                BingoEvent.SetTabNoti(TabNoti() || false);
            }
        }

        public static bool InitCheckNotice()
        {
            if (EventControl.EventInfoList.ContainsKey(EventConditionType.Bingo))
            {
                initLocalNoticeData();

                return TabNoti();
            }
            else
            { 
                DeleteAllBingoEventLocalData();
                return false; 
            }

        }
        public static void UpdateData()
        {
            NowBingoIndex = FindStartBingoMapIndex();
            UpdateBingoUI();
            UpdateNoticeIndexBar();
            isInit = true;
        }
        public static void Dispose()
        {
            NowBingoItemSelectedIndex = 0;
            isInit = false;
            StopRewardEffect();
            BingoEvent.BingoPanel.SetActive(false);
        }
        public static void Final()
        {
            BingoEvent.SetTabNoti(TabNoti());
            SetMainSceneEventIconNoti(TabNoti());
            EventId = "";
        }

        #region [BingoUI]

        public static void SetPrevBtnEvent()
        {
            StopRewardEffect();
            NowBingoIndex--;
            SetBtnDim();
            UpdateBingoUI();
            BingoEvent.SubmitBox.DisposeSubmitBox();
            PlayRewardEffect();
        }
        public static void SetNextBtnEvent()
        {
            StopRewardEffect();
            NowBingoIndex++;
            SetBtnDim();
            UpdateBingoUI();
            BingoEvent.SubmitBox.DisposeSubmitBox();
            PlayRewardEffect();

        }
        public static void SetBtnDim()
        {

            BingoEvent.NextBtn.gameObject.SetActive(true);
            BingoEvent.PrevBtn.gameObject.SetActive(true);
            //총 정보의 길이 가져와야할듯,
            if (NowBingoIndex == DEFAULT_START_BINGOMAP_INDEX)
                BingoEvent.PrevBtn.gameObject.SetActive(false);
            else if (NowBingoIndex == DEFAULT_BINGOMAP_MAX_INDEX)
                BingoEvent.NextBtn.gameObject.SetActive(false);
        }

        public static async void UpdateBingoUI()
        {
            //서버에 정보 요청하고, 빙고 갱신
            RequestBingoEventPage request = new RequestBingoEventPage()
            {
                EventId = EventControl.EventInfoList[EventConditionType.Bingo].eventId,
                MapId = NowBingoIndex
            };
            await WebServerClientModule.Instance.SequenceRequest<RequestBingoEventPage, RequestBingoEventPageResponse>(request, (response) =>
             {
                 if (response.IsNullOrDestroy() || !response.Ok)
                 {
                     if (!(response.IsNullOrDestroy()))
                         MessageControl.FailReason(response.FailReason);
                 }
                 else if (!(response.IsAvailable))
                 {
                     SetNotAvailable();
                     return;
                 }
                 else
                 {
                     if (BingoData == null)
                     {
                         List<BingoItemData> tmpList = new List<BingoItemData>();
                         for (int i = 0; i < BingoEvent.BingoUIItemList.Count; i++)
                         {
                             var tmp = new BingoItemData();
                             tmp.init(response.BingoCellInfos[i]);
                             tmpList.Add(tmp);
                         }
                         BingoData = tmpList;
                     }
                     for (int i = 0; i < BingoEvent.BingoUIItemList.Count; i++)
                     {
                         BingoData[i].init(response.BingoCellInfos[i]);
                         BingoEvent.BingoUIItemList[i].Dispose();
                         BingoEvent.BingoUIItemList[i].init(BingoData[i], i);
                     }

                     UpdateRewardUI(response.BingoMissions);
                     BingoEvent.SetReceiveBtnDim(RemainingRewardCheck(BingoEvent.BingoRewardUIList));
                     CompleteBingoMapSave(NowBingoIndex,response.BingoMissions);
                     PlayRewardEffect();
                     SaveLocalNoticeData(NowBingoIndex, RewardCheck(response.BingoMissions));
                     UpdateNoticeData();
                     UpdateNoticeIndexBar();
                     BingoEvent.SetTabNoti(TabNoti());
                     BingoEvent.BingoPanel.SetActive(true);
                 }
             });
            SetBtnDim();
            UpdateNoticeIndexBar();
        }

        private static void SetNotAvailable()
        {
            BingoEvent.NotAvailablePanel.SetActive(true);
        }
        private static bool RewardCheck(List<BingoEventMissionData> status)
        {
            foreach (var data in status)
            {
                if (data.status == SystemEnums.EventMissionStatus.Finished)
                    return true;
            }
            return false;
        }
        public static void UpdateRewardUI(List<BingoEventMissionData> statuses)
        {
            //서버에 정보 요청하고
            var TemplateData = TemplateContainer<SPEventBingoRewardTemplate>.Find(NowBingoIndex);
            for (int i = 0; i < BingoEvent.BingoRewardUIList.Count; i++)
            {
                BingoEvent.BingoRewardUIList[i].Init(TemplateData, i, statuses[i].status);
            }
        }

        public static void UpdateIndexBar()
        {
            for (int i = 0; i < BingoEvent.IndexBar.Count; i++)
            {
                BingoEvent.IndexBar[i].SetState(BingoIndexBar.IsBingoStateOfIndexBar.Off);
                if (NowBingoIndex % DEFAULT_START_BINGOMAP_INDEX == i)
                {
                    BingoEvent.IndexBar[i].SetState(BingoIndexBar.IsBingoStateOfIndexBar.On);
                }
            }

        }
        public static void GotoRacipeStorage()
        {
            HeadUpDisplayControl.HeadUpDisplayUI.OnClickRecipeStorage();
            CopyCatUIManager.instance.ShowAlert(StringsHelper.GetMessage(12099), StringsHelper.GetMessage(12102));
            EventControl.eventUI.Close();
        }
        public static void Refresh()
        {
            if (isInit == false)
                return;
            BingoEvent.SubmitBox.DisposeSubmitBox();
            BingoDataUpdate();
        }

        #endregion

        #region [BingoUIUtility]
        public static void BingoDataUpdate()
        {
            for (int i = 0; i < BingoData.Count; i++)
            {
                BingoData[i].UpdateAccount();
                BingoEvent.BingoUIItemList[i].init(BingoData[i], i);
                BingoEvent.BingoUIItemList[i].SetTranslation();
            }
        }

        public static void BingoDataUpdate(int Index)
        {
            BingoData[Index].UpdateAccount();
            BingoEvent.BingoUIItemList[Index].init(BingoData[Index], Index);
            BingoEvent.BingoUIItemList[Index].SetTranslation();
        }

        public static void SetDishesLayOut()
        {
            for (int i = 0; i < BingoEvent.BingoUIItemList.Count; i++)
            {
                if (NowBingoItemSelectedIndex == i)
                {
                    BingoEvent.BingoUIItemList[i].isSelected = true;

                }
                else
                    BingoEvent.BingoUIItemList[i].isSelected = false;
                BingoEvent.BingoUIItemList[i].UpdateLayout();
            }

        }
        private static bool IsCompleteBingo(List<int> CompletedBingoList)
        {
            bool IsComplete = false;
            if (CompletedBingoList.Count > 0)
                IsComplete = true;
            return IsComplete;
        }

        public static List<int> CheckBingoRewardIndexList()
        {
            List<int> RewardList = new List<int>();
            for (int i = 0; i < BingoEvent.BingoRewardUIList.Count; i++)
            {
                if (BingoEvent.BingoRewardUIList[i].Status == SystemEnums.EventMissionStatus.Finished)
                    RewardList.Add(i);

            }
            return RewardList;

        }
        #endregion

        #region [BingoDishSubmit]
        public static void DishSubmit(int Index)
        {
            if (BingoData[Index].DishAmount < 1)
            {
                var TemplateData = TemplateContainer<ItemsTemplate>.Find(BingoData[Index].StringID);
                ImmediateBuyControl.ShowUI(TemplateData, ItemControl.CreateItemData(TemplateData.Id, BingoData[Index].DishDemandCount - BingoData[Index].DishAmount), false);
            }
            else
            {
                BingoEvent.SubmitBox.SetSubmitBoxBoxTranslation(BingoData[Index].DishName, BingoData[Index].DishDemandCount.ToString());
                BingoEvent.SubmitBox.ShowSubmitBox(BingoEvent.BingoUIItemList[Index].transform.position);
            }
            BingoEvent.BingoUIItemList[Index].UpdateData();
        }

        public static async void DishSubmitBtnClick(int index)
        {
            SubmitBingoEventDish request = new SubmitBingoEventDish
            {
                EventId = EventControl.EventInfoList[EventConditionType.Bingo].eventId,
                DishData = new DishesData
                {
                    Amount = 1,
                    DishType = SystemEnums.DishType.Common,
                    TemplateId = BingoData[index].DishTemplateId
                },
                MapId = NowBingoIndex,
                Index = index
            };
            var response = await WebServerClientModule.Instance.CommonRequest<SubmitBingoEventDish, SubmitBingoEventDishResponse>(request);
            //빙고 아이템 업데이트,
            if (response.IsNullOrDestroy() || !response.Ok)
            {
                if (response.IsNullOrDestroy())
                    return;

                MessageControl.FailReason(response.FailReason);

                return;
            }
            var UpdateDishesData = new DishesData
            {
                Amount = ItemControl.GetAmount(BingoData[index].DishName) + response.DishesDataDiff.Amount,
                DishType = SystemEnums.DishType.Common,
                TemplateId = BingoData[index].DishTemplateId
            };

            ItemControl.UpdateItemAssetData(response.DishesDataDiff);
            BingoData[index].init(response.BingoCellInfo);
            //BingoEvent.BingoUIItemList[index].init(BingoData[index], index);
            BingoEvent.SubmitBox.DisposeSubmitBox();
            BingoDataUpdate();

            //빙고가 됐다면 리워드 업데이트
            if (IsCompleteBingo(response.CompletedBingos))
            {
                //리워드 업데이트
                StopRewardEffect();
                InstanceRewardNotification(StringsHelper.GetMessage(12108));
                var TemplateData = TemplateContainer<SPEventBingoRewardTemplate>.Find(NowBingoIndex);
                for (int i = 0; i < response.CompletedMissions.Count; i++)
                    BingoEvent.BingoRewardUIList[response.CompletedMissions[i].Index].Init(TemplateData, response.CompletedMissions[i].Index, response.CompletedMissions[i].status);
                BingoEvent.SetReceiveBtnDim(false);
                PlayRewardEffect();
                //노티 업데이트,
                SaveLocalNoticeData(NowBingoIndex, true);
                UpdateNoticeData();
                UpdateNoticeIndexBar();
                BingoEvent.SetTabNoti(TabNoti());
            }
        }
        #endregion

        #region[BingoReward]
        private static void StopRewardEffect()
        {
            foreach (var data in BingoEvent.BingoRewardUIList)
            {
                data.Dispose();
            }
        }

        private static void PlayRewardEffect()
        {
            foreach (var data in BingoEvent.BingoRewardUIList)
            {
                data.ParicleSys.Play();
            }
        }

        public static async void OnReceiveRewardAsync()
        {
            RequestReceiveBingoMissionReward request = new RequestReceiveBingoMissionReward()
            {
                EventId = EventControl.EventInfoList[EventConditionType.Bingo].eventId,
                MapId = NowBingoIndex,
                TargetIndexes = CheckBingoRewardIndexList()
            };
            var response = await WebServerClientModule.Instance.CommonRequest<RequestReceiveBingoMissionReward, RequestReceiveBingoMissionRewardResponse>(request);
            if (response.IsNullOrDestroy() || !response.Ok)
            {
                if (response.IsNullOrDestroy())
                    return;

                MessageControl.FailReason(response.FailReason);

                return;
            }
            RewardsControl.ShowUI(new RewardBundle() { MainRewards = response.MissionRewards });
            var TemplateData = TemplateContainer<SPEventBingoRewardTemplate>.Find(NowBingoIndex);
            for (int i = 0; i < BingoEvent.BingoRewardUIList.Count; i++)
            {
                BingoEvent.BingoRewardUIList[i].Init(TemplateData, i, response.RewardReceivedMissions[i].status);
            }
            BingoEvent.SetReceiveBtnDim(true);
            SaveLocalNoticeData(NowBingoIndex, false);
            UpdateNoticeData();
            UpdateNoticeIndexBar();
            CompleteBingoMapSave(NowBingoIndex, response.RewardReceivedMissions);
            BingoEvent.SetTabNoti(TabNoti());
        }

        public static bool RemainingRewardCheck(List<BingoRewardListItem> rewards)
        {
            for (int i = 0; i < rewards.Count; i++)
            {
                if (rewards[i].Status.Equals(SystemEnums.EventMissionStatus.Finished))
                    return false;
            }
            return true;
        }

        private static void InstanceRewardNotification(string desc)
        {
            var NotiUI = CopyCatUIManager.instance.ShowPopup(BingoEvent.prefNotiUI, false).GetComponent<BingoRewardNotificationUI>();
            NotiUI.init(desc);
        }
        #endregion

        #region [LocalData]
        private static void initCompleteBingoMapcheck()
        {
            if (!(PlayerPrefs.HasKey(CompletedBingoMapKeys[0])))
            {
                foreach (var data in CompletedBingoMapKeys)
                {
                    PlayerPrefs.SetInt(data, 0);
                }
            }
            UpdateCompleteBingoMapData();
        }
        private static void UpdateCompleteBingoMapData()
        {
            List<bool> tmp = new List<bool>();
            for (int i = 0; i < CompletedBingoMapKeys.Count; i++)
            {
                if (PlayerPrefs.GetInt(CompletedBingoMapKeys[i]) == 1)
                    tmp.Add(true);
                else
                    tmp.Add(false);
            }
            CompleteBingoMapData = tmp;
        }

        private static void CompleteBingoMapSave(int Index, List<BingoEventMissionData> bingoEventMissionDatas)
        {
            if (CompleteCheck(bingoEventMissionDatas))
                PlayerPrefs.SetInt(CompletedBingoMapKeys[Index%DEFAULT_START_BINGOMAP_INDEX], 1);
            else
                PlayerPrefs.SetInt(CompletedBingoMapKeys[Index % DEFAULT_START_BINGOMAP_INDEX], 0);
            UpdateCompleteBingoMapData();
        }

        private static bool CompleteCheck(List<BingoEventMissionData> bingoEventMissionDatas)
        {
            foreach (var data in bingoEventMissionDatas)
            {
                if (!data.status.Equals(SystemEnums.EventMissionStatus.GiveReward))
                    return false;
            }
            return true;
        }

        private static int FindStartBingoMapIndex()
        {
            for (int i = 0; i < CompletedBingoMapKeys.Count; i++)
            {
                if (CompleteBingoMapData[i] == false)
                {
                    return DEFAULT_START_BINGOMAP_INDEX + i;
                }
            }
            return DEFAULT_BINGOMAP_MAX_INDEX;
        }
        private static void DeleteAllBingoEventLocalData()
        {
            DeleteBingoEventIdLocalData();
            DeleteBingoNoticeLocalData();
            DeleteCompleteBingoMapLocalData();

        }

        private static void DeleteCompleteBingoMapLocalData()
        {
            foreach(var Data in CompletedBingoMapKeys)
            {
                PlayerPrefs.DeleteKey(Data);
            }
        }
        private static void DeleteBingoNoticeLocalData()
        {
            foreach(var Data in BingoNoticeKeys)
            {
                PlayerPrefs.DeleteKey(Data);
            }
        }
        private static void DeleteBingoEventIdLocalData()
        {
            PlayerPrefs.DeleteKey(BingoEventIdKey);
        }

        #region [Notice]
        // 내장 함수에 bool형을 저장하는게 없어서 인트로 저장
        // 1은 true, 0은 false로 설정
        public static bool TabNoti()
        {
            foreach (var Data in BingoNoticeData)
            {
                if (Data == 1)
                    return true;
            }
            return false;
        }
        private static void UpdateNoticeIndexBar()
        {
            UpdateIndexBar();
            for (int i = 0; i < BingoNoticeData.Count; i++)
            {
                if (BingoNoticeData[i] == 1)
                {
                    BingoEvent.IndexBar[i].SetState(BingoIndexBar.IsBingoStateOfIndexBar.Noti);
                }

            }
        }
        private static void initLocalNoticeData()
        {
            if (PlayerPrefs.HasKey(BingoNoticeKeys[0]))
            {
                UpdateNoticeData();
            }
            else
            {
                foreach (var Keys in BingoNoticeKeys)
                    PlayerPrefs.SetInt(Keys, 0);
                UpdateNoticeData();
            }
        }

        private static void UpdateNoticeData()
        {
            BingoNoticeData = LoadAllLocalNoticeData();
        }
        public static void SaveLocalNoticeData(int index, bool isNotice)
        {
            int NowIndex = index % DEFAULT_START_BINGOMAP_INDEX;
            int CheckNotice = 0;
            if (isNotice)
                CheckNotice = 1;

            PlayerPrefs.SetInt(BingoNoticeKeys[NowIndex], CheckNotice);
        }

        public static bool LoadLocalNoticeData(int index)
        {
            if (PlayerPrefs.GetInt(BingoNoticeKeys[index]) == 1)
                return true;
            return false;
        }

        public static List<int> LoadAllLocalNoticeData()
        {
            List<int> tmp = new List<int>();
            foreach (var Keys in BingoNoticeKeys)
                tmp.Add(PlayerPrefs.GetInt(Keys));
            return tmp;
        }
        public static void SetMainSceneEventIconNoti(bool isOn)
        {
            bool checkHaveReward = false;

            foreach (var item in EventControl.EventInfoList.Values)
            {
                if (item.HaveReward() == true)
                {
                    checkHaveReward = true;
                    break;
                }
            }

            // 모든 이벤트 확인했는지 체크
            bool checkAllEventConfirm = true;
            foreach (var item in EventControl.checkEventConfirmList.Values)
            {
                if (item == false)
                {
                    // 만약 추가된 이벤트를 아직 확인하지 않았다면
                    checkAllEventConfirm = false;
                    break;
                }
            }
            EventControl.OnCheckNoticeHUD(isOn || checkHaveReward || checkAllEventConfirm == false);
        }
        #endregion
        #endregion
    }
}