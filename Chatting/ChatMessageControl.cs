using System.Text;
using System.Collections.Generic;
using UnityEngine;

using Protocol.GameWebAndClient;
using Protocol.GameWebAndClient.SharedDataModels;
using Protocol.GameWebAndClient.CGW;
using Protocol.GameWebAndClient.GWC;
namespace Ekkorr.Cook
{
    public static class ChatMessageControl
    {
        // 
        private static Queue<PublicChatData> systemMessageList;
        private static List<ChattingData> personalMessageList;
        private static List<ChattingData> guildMessageList;

        public static long guildChatMinId { get; set; }
        public static long guildSystemMinId { get; set; }
        public static long systemMinId { get; set; }
        public static bool isOpenFriendUI { get; set; }
        public static bool isOpenGuildLobbyChattingUI { get; set; }


        public delegate void OnChatNotice(bool isOn);
        private static event OnChatNotice onGuildChatNotice;
        private static event OnChatNotice onFriendChatNotice;

        private static int checkRepeatTime;
        private const int updateChatDataRepeatTime = 60;

        private static Dictionary<string, long> checkConfirmChatting;

        public static void Init(long publicChatLatestId, List<PrivateChatData> personalMsgList, List<GuildChatData> guildMsgList)
        {
            systemMessageList = new Queue<PublicChatData>();
            personalMessageList = new List<ChattingData>();
            guildMessageList = new List<ChattingData>();

            checkRepeatTime = 0;
            
            UserDataControl.LoadUserData();
            
            systemMinId = publicChatLatestId;

            ParsingPrivateChatDatas();
            if (CheckFriendNewChatData(personalMsgList))
                HeadUpDisplayControl.UpdateFriendChatNotice(true);

            guildChatMinId = UserDataControl.LocalUserData.guildChatDataId;
            guildSystemMinId = UserDataControl.LocalUserData.guildSystemChatDataId;
            if (guildMsgList != null && guildMsgList.Count > 0)
            {
                long guildChatMaxId = GetGuildChatMaxId(guildMsgList, SystemEnums.PostSenderType.User);
                long guildSystemMaxId = GetGuildChatMaxId(guildMsgList, SystemEnums.PostSenderType.System);

                if (guildChatMinId < guildChatMaxId ||
                    guildSystemMinId < guildSystemMaxId)
                {
                    HeadUpDisplayControl.UpdateGuildChatNotice(true);

                    guildChatMinId = guildChatMaxId;
                    guildSystemMinId = guildSystemMaxId;
                }
            }
        }

        public static void Dispose()
        {
            TimeControl.RemoveSecondChangeHandler(LoadChatDataTime);
        }

        public static List<ChattingData> ConvertToPersonalChattingData(List<PrivateChatData> dataList)
        {
            List<ChattingData> chatList = new List<ChattingData>();

            for (int i = 0; i < dataList.Count; i++)
            {
                ChattingData.MessageType messageType;
                if (dataList[i].SenderNickName.Equals(UserDataControl.Name))
                {
                    if (dataList[i].TemplateId > 0)
                        messageType = ChattingData.MessageType.MyDailyPresent;
                    else
                        messageType = ChattingData.MessageType.MyMsg;
                }
                else
                {
                    if (dataList[i].TemplateId > 0)
                        messageType = ChattingData.MessageType.SomeoneDailyPresent;
                    else
                        messageType = ChattingData.MessageType.SomeoneMsg;
                }

                ChattingData data = new PersonalChattingData(dataList[i].Id, dataList[i].CreatedElapsedSeconds, GetMessage(dataList[i].TemplateId, dataList[i].Parameters),
                    dataList[i].SenderProfile, dataList[i].SenderNickName, messageType);

                chatList.Add(data);
            }

            return chatList;
        }

        public static List<ChattingData> ConvertToGuildChattingData(List<GuildChatData> dataList)
        {
            List<ChattingData> chatList = new List<ChattingData>();
            string guildMasterNickname = GuildControl.memberDatas.Find(e => e.GuildClassType == Protocol.GameWebAndClient.SystemEnums.GuildClassType.Master).GuildName;

            for (int i = 0; i < dataList.Count; i++)
            {
                ChattingData.MessageType messageType;
                string senderName = string.Empty;

                if (dataList[i].PostSenderType == Protocol.GameWebAndClient.SystemEnums.PostSenderType.User)
                {
                    messageType = dataList[i].SenderNickname.Equals(UserDataControl.Name) ? ChattingData.MessageType.MyMsg : ChattingData.MessageType.SomeoneMsg;
                    if (dataList[i].SenderNickname.Equals(guildMasterNickname))
                        senderName = string.Format("{0}{1}", UI.StringsHelper.GetMessage(11764), dataList[i].SenderNickname);
                    else
                        senderName = dataList[i].SenderNickname;
                    // 11764 [길드마스터]
                }
                else
                {
                    messageType = ChattingData.MessageType.System;
                    senderName = UI.StringsHelper.GetMessage(11765); // 11765 [길드 알람]
                }

                ChattingData data = new GuildChattingData(dataList[i].Id, dataList[i].CreatedElapsedSeconds, GetMessage(dataList[i].TemplateId, dataList[i].Parameters),
                    dataList[i].SenderProfile, senderName, messageType);

                chatList.Add(data);
            }

            return chatList;
        }

        // 채팅일 경우 TemplateId가 -1이고 parameters의 인자는 1개이며 채팅 메세지가 담겨 있음
        // 채팅이 아닐 경우 Strings Template의 Id이며 parameters는 해당 문자열의 파라미터가 담겨 있음
        private static string GetMessage(int stringTemplateId, List<string> parameters)
        {
            if (stringTemplateId > 0)
            {
                if(stringTemplateId.Equals(2050207))
                {
                    string[] arrParam = new string[parameters.Count];
                    arrParam[0] = parameters[0];
                    arrParam[1] = TemplateContainer<ClientDataContainer.GuildWarTemplate>.Find(parameters[1]).NameRef.Get();

                    return UI.StringsHelper.GetMessage(stringTemplateId, arrParam);
                }
                else
                    return UI.StringsHelper.GetMessage(stringTemplateId, parameters.ToArray());
            }
            else
                return parameters[0];
        }

        public static void CheckFriendNotice(List<PrivateChatData> personalMsgList)
        {
            onFriendChatNotice?.Invoke(CheckFriendNewChatData(personalMsgList));
        }

        public static bool CheckFriendNewChatData(List<PrivateChatData> personalMsgList)
        {
            // 하위호환 코드 추가
            if (personalMsgList == null)
            {
                return false;
            }

            for (int i = 0; i < personalMsgList.Count; i++)
            {
                if (personalMsgList[i].TemplateId > 0 || personalMsgList[i].SenderNickName.Equals(UserDataControl.Name))
                    continue;

                if (checkConfirmChatting.ContainsKey(personalMsgList[i].SenderNickName) == false)
                    checkConfirmChatting.Add(personalMsgList[i].SenderNickName, 0);

                if (checkConfirmChatting[personalMsgList[i].SenderNickName] < personalMsgList[i].Id)
                    return true;
            }

            return false;
        }

        public static bool CheckFriendNewChatData(string nickname, List<PrivateChatData> personalMsgList)
        {
            for (int i = 0; i < personalMsgList.Count; i++)
            {
                if (personalMsgList[i].TemplateId > 0 || personalMsgList[i].SenderNickName.Equals(nickname) == false)
                    continue;
                if (checkConfirmChatting.ContainsKey(nickname) == false)
                    checkConfirmChatting.Add(nickname, 0);

                if (checkConfirmChatting[nickname] < personalMsgList[i].Id)
                    return true;
            }

            return false;
        }
        
        #region [UPDATE CHAT DATA]
        public static void OnSecondChangeEvent(bool isOn)
        {
            if (isOn)
                TimeControl.AddSecondChangeHandler(LoadChatDataTime);
            else
                TimeControl.RemoveSecondChangeHandler(LoadChatDataTime);
        }

        public static void LoadChatDataTime()
        {
            checkRepeatTime++;

            if (checkRepeatTime >= updateChatDataRepeatTime)
            {
                checkRepeatTime = 0;

                if(isOpenFriendUI == false)
                    CheckNewPrivateChatData();

                if(isOpenGuildLobbyChattingUI == false)
                    CheckNewGuildChatData();

                UpdateNewSystemMessageData();
            }
        }

        private static async void CheckNewPrivateChatData()
        {
            long minId = GetConfirmChatMinId();

            var request = new LoadReceivedPrivateChat
            {
                ReceiveNickname = UserDataControl.Name,
                Index = 1,
                MinId = minId
            };
            await Network.WebServerClientModule.Instance.SequenceRequest<LoadReceivedPrivateChat, LoadReceivedPrivateChatResponse>(request, (response) =>
            {
                if (response.IsNullOrDestroy() || response.Ok == false ||
                response.ChatDatas == null || response.ChatDatas.Count == 0)
                    return;

                if(CheckFriendNewChatData(response.ChatDatas))
                    onFriendChatNotice?.Invoke(true);

            });
        }

        private static async void CheckNewGuildChatData()
        {
            if (GuildControl.IsJoin() == false)
                return;

            var request = new LoadGuildChat
            {
                GuildName = GuildControl.guildName,
                Index = 1,
                MinId = guildChatMinId,
                MinSystemId = guildSystemMinId
            };

            await Network.WebServerClientModule.Instance.SequenceRequest<LoadGuildChat, LoadGuildChatResponse>(request, (response) =>
            {
                if (response.IsNullOrDestroy() || response.Ok == false ||
                response.ChatDatas == null || response.ChatDatas.Count == 0)
                    return;

                long chatMax = GetGuildChatMaxId(response.ChatDatas, SystemEnums.PostSenderType.User);
                long systemMax = GetGuildChatMaxId(response.ChatDatas, SystemEnums.PostSenderType.System);

                bool checkChat = false;
                bool checkSystem = false;

                if (chatMax > guildChatMinId)
                    checkChat = true;

                if (systemMax > guildSystemMinId)
                    checkSystem = true;

                if (checkChat || checkSystem)
                {
                    onGuildChatNotice?.Invoke(true);
                }
            });
        }

        private static async void UpdateNewSystemMessageData()
        {
            var request = new LoadPublicChat
            {
                Index = 1,
                MinId = systemMinId
            };
            await Network.WebServerClientModule.Instance.SequenceRequest<LoadPublicChat, LoadPublicChatResponse>(request, (response) =>
            {
                if (response.IsNullOrDestroy() || response.Ok == false ||
                response.ChatDatas == null || response.ChatDatas.Count == 0)
                    return;

                for (int i = 0; i < response.ChatDatas.Count; i++)
                    systemMessageList.Enqueue(response.ChatDatas[i]);

                systemMinId = GetSystemChatMaxId(response.ChatDatas);
            });
        }
        #endregion

        public static long GetConfirmChatMinId()
        {
            long resultId = long.MaxValue;
            foreach(var item in checkConfirmChatting.Values)
            {
                if (resultId > item)
                    resultId = item;
            }

            return resultId;
        }

        private static long GetPersonalChatMaxId(List<PrivateChatData> chatDatas)
        {
            if (chatDatas == null || chatDatas.Count == 0)
                return 0;

            long resultId = 0;

            for (int i = 0; i < chatDatas.Count; i++)
            {
                if (chatDatas[i].TemplateId < 0 && resultId < chatDatas[i].Id)
                {
                    resultId = chatDatas[i].Id;
                }
            }

            return resultId;
        }

        private static long GetGuildChatMaxId(List<GuildChatData> chatDatas, SystemEnums.PostSenderType type)
        {
            if (chatDatas == null || chatDatas.Count == 0)
                return 0;

            long resultId = 0;

            for(int i = 0; i < chatDatas.Count; i++)
            {
                if (chatDatas[i].PostSenderType == type && resultId < chatDatas[i].Id)
                {
                    resultId = chatDatas[i].Id;
                }
            }

            return resultId;
        }

        private static long GetSystemChatMaxId(List<PublicChatData> chatDatas)
        {
            long result = 0;

            for (int i = 0; i < chatDatas.Count; i++)
                result = result < chatDatas[i].Id ? chatDatas[i].Id : result;

            return result;
        }

        public static PublicChatData GetSystemMessageData()
        {
            if (systemMessageList.Count == 0)
                return null;

            return systemMessageList.Dequeue();
        }

        #region [LOCAL DATA]
        private static void ParsingPrivateChatDatas()
        {
            if (checkConfirmChatting == null)
                checkConfirmChatting = new Dictionary<string, long>();
            checkConfirmChatting.Clear();

            if (string.IsNullOrEmpty(UserDataControl.LocalUserData.privateChatDataIdList))
                return;

            string[] friendNameList = UserDataControl.LocalUserData.privateChatDataIdList.Split('|');

            for (int i = 0; i < friendNameList.Length; i++)
            {
                string[] datas = friendNameList[i].Split('_');
                if (FriendControl.IsFriend(datas[0]))
                {
                    checkConfirmChatting.Add(datas[0], long.Parse(datas[1]));
                }
            }
        }

        public static void SaveCheckConfirmPersonalChatId(string senderNickname, List<PrivateChatData> chatDatas)
        {
            if (chatDatas == null || chatDatas.Count == 0)
                return;

            long id = GetPersonalChatMaxId(chatDatas);

            if (checkConfirmChatting.ContainsKey(senderNickname))
                checkConfirmChatting[senderNickname] = id;
            else
                checkConfirmChatting.Add(senderNickname, id);

            List<string> keyList = new List<string>(checkConfirmChatting.Count);

            foreach (var item in checkConfirmChatting)
            {
                string key = $"{item.Key}_{item.Value}";
                keyList.Add(key);
            }

            string saveData = string.Join("|", keyList);
            UserDataControl.LocalUserData.privateChatDataIdList = saveData;
            UserDataControl.SaveUserData();
        }

        public static void SaveGuildChatId(long id, SystemEnums.PostSenderType type)
        {
            switch(type)
            {
                case SystemEnums.PostSenderType.User:
                    guildChatMinId = id;
                    UserDataControl.LocalUserData.guildChatDataId = id;
                    break;

                case SystemEnums.PostSenderType.System:
                    guildSystemMinId = id;
                    UserDataControl.LocalUserData.guildSystemChatDataId = id;
                    break;
            }
            
            UserDataControl.SaveUserData();
        }
        #endregion

        #region [EVENTS]
        public static void AddOnGuildChatNotice(OnChatNotice onEvent)
        {
            onGuildChatNotice -= onEvent;
            onGuildChatNotice += onEvent;
        }

        public static void RemoveOnGuildChatNotice(OnChatNotice onEvent)
        {
            onGuildChatNotice -= onEvent;
        }

        public static void AddOnFriendChatNotice(OnChatNotice onEvent)
        {
            onFriendChatNotice -= onEvent;
            onFriendChatNotice += onEvent;
        }

        public static void RemoveOnFriendChatNotice(OnChatNotice onEvent)
        {
            onFriendChatNotice -= onEvent;
        } 
        #endregion
    }

}