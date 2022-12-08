namespace Ekkorr.Cook
{
    using Ekkorr.Cook.Network;
    using Ekkorr.Cook.UI;
    using Protocol.GameWebAndClient;
    using Protocol.GameWebAndClient.CGW;
    using Protocol.GameWebAndClient.GWC;
    using Protocol.GameWebAndClient.SharedDataModels;
    using System.Collections.Generic;
    using UnityEngine;



    public static class SiriusQuizControl
    {

        //인덱스에 따라 질문 내용, 답안지리스트로 들고 있기

        public static List<SiriusQuizData> QuizDataList = new List<SiriusQuizData>();

        //틀린 문제를 저장할 리스트
        public static List<SiriusQuizData> WrongQuizDataList = new List<SiriusQuizData>();
        //선택한 답안을 저장한다.
        public static int[] MyChoiceAnswer = new int[5] { -1, -1, -1, -1, -1 };
        public static int[] MyEraseChoiceAnswer = new int[5] { -1, -1, -1, -1, -1 };

        public static bool IsFirst = true;
        public static bool IsErase = false;
        public static bool IsErasing = false;
        public static bool IsEnd = false;
        public static SiriusQuizLobbyUI SiriusQuizLobby;
        public static int EraseCount = 0;
        public static int NowQuizIndex;
        public static int NowChoiceNumber;
        public static EventInfo EventData;
        public static List<EventRewardInfo> EventRewardData;
        public static RequestQuizOfSiriusEventPageResponse response;
        private static string UserDataID;
        public static void Init(EventInfo EventInfoData, List<EventRewardInfo> EventRewardInfoData)
        {
            ControlListClear();
            NowQuizIndex = 0;
            IsFirst = true;
            EventData = EventInfoData;
            EventRewardData = EventRewardInfoData;
            SetEventData();

        }

        #region [Init]
        public static async void SetEventData()
        {
            RequestQuizOfSiriusEventPage request = new RequestQuizOfSiriusEventPage()
            {
                EventId = EventData.eventId
            };

            await WebServerClientModule.Instance.SequenceRequest<RequestQuizOfSiriusEventPage, RequestQuizOfSiriusEventPageResponse>(request, 
                (response) => { 
                 
                     if (response.IsNullOrDestroy() || !response.Ok)
                     {
                         if (response.IsNullOrDestroy())
                             return;
                         MessageControl.FailReason(response.FailReason);
                         return;
                     }
                     SiriusQuizControl.response = response;
                     if (UserDataID == null)
                     {
                         UserDataID = UserDataControl.LocalUserData.AccountName;
                     }
                     else
                     {
                         if (UserDataID != UserDataControl.LocalUserData.AccountName)
                         {
                             Debug.Log("새로 로그인됨");
                             ResetData();
                             UserDataID = UserDataControl.LocalUserData.AccountName;
                         }
                     }

                     IsFirst = CheckFirst();
                     IsEnd = CheckSolvedQuestion(response);
                     IsErase = false;
                     EraseCount = response.EraserOfSiriusCount;
                     SetQuizIndex(response.QuizDatas);
                     SetWrongQuizData(response.QuizDatas);
                     QuizLobbyUIInit(response.QuizDatas);
                 });

        }

        private static void ResetData()
        {
            ControlListClear();
            for (int i = 0; i < MyChoiceAnswer.Length; i++)
            {
                MyChoiceAnswer[i] = -1;
                MyEraseChoiceAnswer[i] = -1;
                NowQuizIndex = 0;
            }
        }

        public static async void RefreshResponse()
        {
            RequestQuizOfSiriusEventPage request = new RequestQuizOfSiriusEventPage()
            {
                EventId = EventData.eventId
            };

            response = await WebServerClientModule.Instance.CommonRequest<RequestQuizOfSiriusEventPage, RequestQuizOfSiriusEventPageResponse>(request);
            if (response.IsNullOrDestroy() || !response.Ok)
            {
                if (response.IsNullOrDestroy())
                    return;
                MessageControl.FailReason(response.FailReason);
                return;
            }
        }
        private static void ControlListClear()
        {
            QuizDataList.Clear();
            WrongQuizDataList.Clear();
        }

        public static void QuizLobbyUIInit(List<QuizOfSiriusEventQuizData> QuizDatas)
        {
            if (!IsEnd)
            {
                SiriusQuizLobby.SiriusQuizStart.init();
                SiriusQuizLobby.SiriusQuizStart.gameObject.SetActive(true);
            }
            else
            {
                SiriusQuizLobby.SiriusQuizResult.Init(QuizDatas);
                SiriusQuizLobby.SiriusQuizResult.gameObject.SetActive(true);

            }
        }

        public static void ResetEraseChoiceList()
        {
            for (int i = 0; i < MyEraseChoiceAnswer.Length; i++)
                MyEraseChoiceAnswer[i] = -1;
        }
        public static void SetQuizIndex(List<QuizOfSiriusEventQuizData> QuizData)
        {
            QuizDataList.Clear();
            for (int i = 0; i < QuizData.Count; i++)
            {
                SiriusQuizData Data = new SiriusQuizData();
                Data.init(QuizData[i], i);
                QuizDataList.Add(Data);
            }
        }


        public static void SetWrongQuizData(List<QuizOfSiriusEventQuizData> QuizDatas)
        {
            WrongQuizDataList.Clear();
            for (int i = 0; i < QuizDatas.Count; i++)
            {

                if (QuizDatas[i].Status == SystemEnums.QuizOfSiriusQuizStatus.Wrong)
                {
                    SiriusQuizData Data = new SiriusQuizData();
                    Data.init(QuizDatas[i], i);
                    WrongQuizDataList.Add(Data);
                }

            }
        }
        public static void SetWrongQuizData(QuizOfSiriusEventQuizData QuizData)
        {
            SiriusQuizData Data = new SiriusQuizData();

            for (int i = 0; i < WrongQuizDataList.Count; i++)
            {
                if (WrongQuizDataList[i].QuizId == QuizData.QuizId)
                {
                    Data.init(QuizData, i);
                    WrongQuizDataList[i] = Data;
                }
            }
        }

        public static void SetEraseCount(int Data)
        {
            EraseCount += Data;
        }
        private static bool CheckFirst()
        {
            bool Data = true;
            for (int i = 0; i < MyChoiceAnswer.Length; i++)
            {
                if (MyChoiceAnswer[i] != -1)
                {
                    Data = false;
                }
            }
            return Data;
        }
        private static void LoadQuizIndex()
        {
            if (IsFirst)
                return;
            if (IsErase)
                return;
            for (int i = 0; i < MyChoiceAnswer.Length; i++)
            {
                if (MyChoiceAnswer[i] == -1)
                {
                    NowQuizIndex = i;
                    return;
                }
            }
            NowQuizIndex = MyChoiceAnswer.Length - 1;

        }
        private static bool CheckSolvedQuestion(RequestQuizOfSiriusEventPageResponse response)
        {
            if (response.QuizDatas[0].Status != SystemEnums.QuizOfSiriusQuizStatus.None)
                return true;
            else
                return false;
        }
        #endregion

        #region [QuizLobby]

        public static void NextQuestion()
        {
            NowQuizIndex++;
        }

        public static void PrevQuestion()
        {
            NowQuizIndex--;
        }

        public static void StartQuiz()
        {
            IsFirst = CheckFirst();
            int IsFirstStringNumber = 12068;
            if (!IsFirst)
            {
                IsFirstStringNumber = 12069;
                LoadQuizIndex();
            }
            else
            {
                NowQuizIndex = 0;
            }
            CopyCatUIManager.instance.ShowConfirm(StringsHelper.GetMessage(2032700), StringsHelper.GetMessage(IsFirstStringNumber), StringsHelper.GetMessage(10011), StringsHelper.GetMessage(10010), null, () =>
            {

                SiriusQuizLobby.SiriusQuiz.Init();
                SiriusQuizLobby.SiriusQuizStart.gameObject.SetActive(false);
            });


        }

        #endregion

        public static int CorrectCount(List<QuizOfSiriusEventQuizData> QuizDatas)
        {
            int Count = 0;
            for (int i = 0; i < QuizDatas.Count; i++)
            {
                if (QuizDatas[i].Status.Equals(SystemEnums.QuizOfSiriusQuizStatus.Correct))
                    Count++;
            }
            return Count;
        }


        public static List<AnswerForQuizOfSirius> MakeAnswerForQuizOfSirius(List<SiriusQuizData> quizData, int[] choiceData)
        {
            List<AnswerForQuizOfSirius> Data = new List<AnswerForQuizOfSirius>();
            for (int i = 0; i < quizData.Count; i++)
            {
                AnswerForQuizOfSirius tmp = new AnswerForQuizOfSirius()
                {
                    QuizId = quizData[i].QuizId,
                    Answer = choiceData[i] + 1

                };
                Data.Add(tmp);
            }
            return Data;
        }
        public static bool CheckAllCorrect(List<QuizOfSiriusEventQuizData> quizData)
        {
            bool data = true;
            for (int i = 0; i < quizData.Count; i++)
            {
                if (quizData[i].Status != SystemEnums.QuizOfSiriusQuizStatus.Correct)
                {
                    data = false;
                }
            }
            return data;
        }

    }

}