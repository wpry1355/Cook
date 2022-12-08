using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Protocol.GameWebAndClient.SharedDataModels;
using Protocol.GameWebAndClient.CGW;
using Protocol.GameWebAndClient.GWC;

namespace Ekkorr.Cook
{
    public class FriendChattingUI : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField] private TextMeshProUGUI txtNickname;
        [SerializeField] private TextMeshProUGUI txtProfileLabel;
        [SerializeField] private TextMeshProUGUI txtFriendArenaLabel;
        [SerializeField] private TextMeshProUGUI txtInputFieldPlaceHolder;
        [SerializeField] private TextMeshProUGUI txtMoreViewLabel;
        [SerializeField] private TextMeshProUGUI txtMoreView;

        [SerializeField] private InputField inputField;
        [SerializeField] private Image portraitIcon;

        [Header("Others")]
        [SerializeField] private GameObject arenaDimdGameObject;
        [SerializeField] private GameObject moreViewObj;
        [SerializeField] private ChattingEKFlexibleList ekList;

#pragma warning restore CS0649

        private int chatIndex;
        private long chatDataLastId;
        private string nickname;
        private Func<Task> onShowProfile;
        private Action battleAction;

        public void Init(int chatIndex, string nickname, Sprite portraitIcon, Func<Task> onShowProfile, Action battleAction, List<PrivateChatData> chatDatas)
        {
            this.portraitIcon.sprite = portraitIcon;

            this.chatIndex = chatIndex;
            this.nickname = nickname;
            txtNickname.text = nickname;

            if (this.onShowProfile != null)
                this.onShowProfile = null;
            this.onShowProfile = onShowProfile;
            this.battleAction = battleAction;

            arenaDimdGameObject.SetActive(DeckControl.IsContainDeckInfo((int)Protocol.GameWebAndClient.SystemEnums.DeckIndex.ArenaAttackDeck) == false);

            SetTranslation();

            if (chatDatas != null && chatDatas.Count > 0)
                chatDataLastId = chatDatas[chatDatas.Count - 1].Id;
            else
                chatDataLastId = 0;
            
            ekList.SetData(AddDateTypeMessage(ChatMessageControl.ConvertToPersonalChattingData(chatDatas)));
            ekList.SetCurrentFirstRenderItemIndex(ekList.GetDataCount() - 1);

            // 기존에 채팅중인 Event 제거
            TimeControl.RemoveSecondChangeHandler(UpdateChatData);
            TimeControl.AddSecondChangeHandler(UpdateChatData);
        }

        public async void UpdateChatData()
        {
            var request = new LoadPrivateChat
            {
                Index = 1,
                RequestNickname = nickname,
                MinId = chatDataLastId
            };

            await Network.WebServerClientModule.Instance.SequenceRequest<LoadPrivateChat, LoadPrivateChatResponse>(request, response =>
           {
               if (!response.IsNullOrDestroy() && response.Ok)
               {
                   response.ChatDatas.Sort(CompareMessageId);
                   UpdateNewLoadChatDatas(response.ChatDatas, true);
                   ChatMessageControl.SaveCheckConfirmPersonalChatId(nickname, response.ChatDatas);
               }
           });
            
        }

        public void OnClickClose()
        {
            CommunityUI.CloseObjectClear();
            TimeControl.RemoveSecondChangeHandler(UpdateChatData);
            this.gameObject.SetActive(false);
        }

        public void OnClickProfile()
        {
            onShowProfile?.Invoke();
        }

        public void OnClickBattle()
        {
            battleAction?.Invoke();
        }

        public void OnClickInputMessageClear()
        {
            inputField.text = string.Empty;
        }

        public async void OnClickMoreView()
        {
            chatIndex++;
            
            CopyCatUIManager.instance.ShowLoading();
            var loadResponse = await LoadPrivateChatAsync(chatIndex);
            CopyCatUIManager.instance.HideLoading();

            if (loadResponse == null)
                return;

            var ekListDatas = ekList.GetDatas();
            ekListDatas.InsertRange(0, ChatMessageControl.ConvertToPersonalChattingData(loadResponse.ChatDatas));
            RemoveDateTypeMessage(ekListDatas);
            
            ekList.SetDataWithPositionKeep(AddDateTypeMessage(ekListDatas));
        }

        public async void OnClickSend()
        {
            if (string.IsNullOrEmpty(inputField.text))
                return;

            CopyCatUIManager.instance.ShowLoading();
            var sendResponse = await SendPrivateChatAsync();
            var loadResponse = await LoadPrivateChatAsync(1);
            CopyCatUIManager.instance.HideLoading();

            if (sendResponse == null || loadResponse == null)
                return;

            UpdateNewLoadChatDatas(loadResponse.ChatDatas, false);
            ChatMessageControl.SaveCheckConfirmPersonalChatId(nickname, loadResponse.ChatDatas);

            OnClickInputMessageClear();
        }

        public void OnScrollValueChange(Vector2 value)
        {
            if (value.y >= 0.95f)
                moreViewObj.SetActive(true);
            else
                moreViewObj.SetActive(false);
        }

        private async Task<SendPrivateChatResponse> SendPrivateChatAsync()
        {
            var request = new SendPrivateChat
            {
                Content = inputField.text,
                ReceiverNickname = nickname
            };
            var response = await Network.WebServerClientModule.Instance.CommonRequest<SendPrivateChat, SendPrivateChatResponse>(request);
            if (response.IsNullOrDestroy() || response.Ok == false)
            {
                CopyCatUIManager.instance.HideLoading();
                if (response.IsNullOrDestroy())
                    return null;

                MessageControl.FailReason(response.FailReason);
                return null;
            }

            return response;
        }

        private async Task<LoadPrivateChatResponse> LoadPrivateChatAsync(int chatIndex)
        {
            var request = new LoadPrivateChat
            {
                Index = chatIndex,
                RequestNickname = nickname,
                MinId = chatDataLastId
            };

            var response = await Network.WebServerClientModule.Instance.CommonRequest<LoadPrivateChat, LoadPrivateChatResponse>(request);
            if (response.IsNullOrDestroy() || response.Ok == false)
            {
                CopyCatUIManager.instance.HideLoading();
                if (response.IsNullOrDestroy())
                    return null;

                MessageControl.FailReason(response.FailReason);
                return null;
            }

            response.ChatDatas.Sort(CompareMessageId);
            return response;
        }

        private void UpdateNewLoadChatDatas(List<PrivateChatData> loadChatDatas, bool keepPos)
        {
            if (loadChatDatas.Count == 0 || loadChatDatas[loadChatDatas.Count - 1].Id.Equals(chatDataLastId))
                return;

            var ekListDatas = ekList.GetDatas();
            ekListDatas.AddRange(ChatMessageControl.ConvertToPersonalChattingData(loadChatDatas));
            RemoveDateTypeMessage(ekListDatas);
            var dataList = AddDateTypeMessage(ekListDatas);
            if (keepPos && ekList.GetScrollRect().normalizedPosition.y > 0.01f)
            {
                ekList.SetDataWithPositionKeep(dataList);
            }
            else
            {
                ekList.SetData(dataList);
                ekList.GetScrollRect().normalizedPosition = new Vector2(0f, 0f);
            }
            

            chatDataLastId = loadChatDatas[loadChatDatas.Count - 1].Id;
        }

        private void RemoveDateTypeMessage(List<ChattingData> dataList)
        {
            dataList.RemoveAll(e =>
            {
                return e.messageType == ChattingData.MessageType.Date;
            });
        }

        private List<ChattingData> AddDateTypeMessage(List<ChattingData> dataList)
        {
            if (dataList == null || dataList.Count == 0)
                return new List<ChattingData>();

            DateTime tempDate = DateTime.UtcNow.ToLocalTime() - TimeSpan.FromSeconds(dataList[0].elapsedSeconds);

            dataList.Insert(0, new PersonalChattingData(UI.StringsHelper.GetMessage(11827, tempDate.Month, tempDate.Day), ChattingData.MessageType.Date));
            int index = 2;

            while(index < dataList.Count)
            {
                var date = DateTime.UtcNow.ToLocalTime() - TimeSpan.FromSeconds(dataList[index].elapsedSeconds);
                if (tempDate.Year.Equals(date.Year) == false ||
                    tempDate.Month.Equals(date.Month) == false ||
                    tempDate.Day.Equals(date.Day) == false)
                {
                    dataList.Insert(index, new PersonalChattingData(UI.StringsHelper.GetMessage(11827, date.Month, date.Day), ChattingData.MessageType.Date));
                    tempDate = date;

                    index++;
                }
                index++;
            }

            return dataList;
        }

        private int CompareMessageId(PrivateChatData a, PrivateChatData b)
        {
            if (a.Id < b.Id)
                return -1;
            else if (a.Id > b.Id)
                return 1;
            else
                return 0;
        }

        private void SetTranslation()
        {
            txtProfileLabel.text = UI.StringsHelper.GetMessage(10941);             // 10941 프로필
            txtFriendArenaLabel.text = UI.StringsHelper.GetMessage(10943);         // 10943 친선전
            txtMoreView.text = UI.StringsHelper.GetMessage(11759);                 // 11759 더보기
            txtInputFieldPlaceHolder.text = UI.StringsHelper.GetMessage(11761);    // 11761 이곳을 클릭하여 메시지를 입력하세요.
        }
    }

}