namespace Ekkorr.Cook
{
    using Ekkorr.Cook.Network;
    using Ekkorr.Cook.UI;
    using Protocol.GameWebAndClient;
    using Protocol.GameWebAndClient.CGW;
    using Protocol.GameWebAndClient.GWC;
    using Protocol.GameWebAndClient.SharedDataModels;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;

    public class SiriusQuizResultUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI TitleText;
        [SerializeField] TextMeshProUGUI DescText;
        [SerializeField] TextMeshProUGUI EraserText;
        [SerializeField] TextMeshProUGUI CorrectionDimText;
        [SerializeField] TextMeshProUGUI CorrectionText;
        [SerializeField] List<SiriusQuizResultAnswerUIItem> AnswerUIItemList;
        [SerializeField] SiriusEraser SiriusEraser;
        [SerializeField] List<SiriusQuizRewardListItem> RewardListUIItem;
        [SerializeField] TextMeshProUGUI SiriusEraserCount;
        [SerializeField] GameObject EraserBtn;
        [SerializeField] GameObject EraserBtnDim;


        private int CorrectCount = 0;
        public void Init(List<QuizOfSiriusEventQuizData> QuizDatas)
        {
            CorrectCount = 0;
            UpdataResultAnswerData(QuizDatas);
            SiriusEraser.init();
            CorrectCount = SiriusQuizControl.CorrectCount(QuizDatas);
            UpdateReward(SiriusQuizControl.EventRewardData, QuizDatas);
            SetEraserBtnDim(QuizDatas);
            SetTranslation();
        }
        public void InitResult(SubmitAnswersForQuizOfSiriusResponse response)
        {
            SiriusQuizControl.RefreshResponse();
            CorrectCount = 0;
            UpdataResultAnswerData(response.QuizDatas);
            SiriusEraser.init();
            CorrectCount = SiriusQuizControl.CorrectCount(response.QuizDatas);
            UpdateReward(response);
            SetEraserBtnDim(response.QuizDatas);
            SetTranslation();
        }

        private void UpdataResultAnswerData(List<QuizOfSiriusEventQuizData> QuizDatas)
        {
            for (int i = 0; i < AnswerUIItemList.Count; i++)
            {
                AnswerUIItemList[i].Init(i + 1, QuizDatas[i].Status);
            }
        }

        private void SetTranslation()
        {
            TitleText.text = StringsHelper.GetMessage(2032700);
            DescText.text = StringsHelper.GetMessage(12074, CorrectCount);
            EraserText.text = StringsHelper.GetMessage(12066);
            CorrectionDimText.text = StringsHelper.GetMessage(12073);
            CorrectionText.text = StringsHelper.GetMessage(12073);
            SiriusEraserCount.text = StringsHelper.GetMessage(10258, SiriusQuizControl.EraseCount, 3);
        }


        public void Dispose()
        {
            for (int i = 0; i < AnswerUIItemList.Count; i++)
            {
                AnswerUIItemList[i].Dispose();
            }
        }

        public void Final()
        {
            for (int i = 0; i < AnswerUIItemList.Count; i++)
            {
                AnswerUIItemList[i].Dispose();
            }
        }
        private void UpdateReward(SubmitAnswersForQuizOfSiriusResponse response)
        {
            int i = 0;
            EventControl.UpdateEvents(response.EventDataDiffs);
            var data = EventControl.EventInfoList[EventConditionType.QuizOfSirius].rewardList;
            foreach (var Data in RewardListUIItem)
            {
                Data.Init(data[i], response.QuizDatas, i);
                Data.AddReceiveReward(OnReceiveRewardAsync);
                i++;
            }
        }
        private void UpdateReward(List<EventRewardInfo> EventRewardData, List<QuizOfSiriusEventQuizData> QuizDatas)
        {
            int i= 0;
            foreach (var Data in RewardListUIItem)
            {
                Data.Init(EventRewardData[i], QuizDatas, i);
                Data.AddReceiveReward(OnReceiveRewardAsync);
                i++;
            }

        }


        public void OnClickEraseBtn()
        {
                if (SiriusQuizControl.EraseCount == 0)
                    CopyCatUIManager.instance.ShowAlert(StringsHelper.GetMessage(2032700), StringsHelper.GetMessage(12077));
                else
                    ShowEraseUI();
        }
        private void SetEraserBtnDim(List<QuizOfSiriusEventQuizData> quizDatas)
        {
            EraserBtnDim.SetActive(SiriusQuizControl.CheckAllCorrect(quizDatas));
        }

        public async void ShowEraseUI()
        {
            RequestQuizOfSiriusEventPage request = new RequestQuizOfSiriusEventPage()
            {
                EventId = SiriusQuizControl.EventData.eventId
            };

            var response = await WebServerClientModule.Instance.CommonRequest<RequestQuizOfSiriusEventPage, RequestQuizOfSiriusEventPageResponse>(request);
            if (response.IsNullOrDestroy() || !response.Ok)
            {
                if (response.IsNullOrDestroy())
                    return;
                MessageControl.FailReason(response.FailReason);
                return;
            }

            SiriusQuizControl.SetWrongQuizData(response.QuizDatas);

            SiriusQuizControl.IsErase = true;
            SiriusQuizControl.SiriusQuizLobby.SiriusQuiz.Init(response.SubmittedAnswers);

            gameObject.SetActive(false);
        }
        public void OnclickHelpBtn()
        {
            FacilityInformationControl.ShowFacilityInformationUI(E_FacilityInformationType.QuizOfSiriusEvent);
        }

        public async void OnReceiveRewardAsync(int step)
        {
            AppsFlyerManager.instance.SendEvent(
                AppsFlyerEventSceneType.Event,
                AppsFlyerEventActionType.ReceiveEventReward);

            TakeEventReward request = new TakeEventReward
            {
                EventId = SiriusQuizControl.response.EventId,
                Step = step
            };

            var response = await WebServerClientModule.Instance.CommonRequest<TakeEventReward, TakeEventRewardResponse>(request);
            if (response.IsNullOrDestroy() == true || response.Ok == false)
            {
                if (response.IsNullOrDestroy() == true) return;

                MessageControl.FailReason(response.FailReason);
                return;
            }

            RewardsControl.ShowUI(new RewardBundle() { MainRewards = response.Rewards });
            EventControl.UpdateEvents(response.Rewards.UpdatedContents.UpdatedEvents);
            EventControl.UpdateNotice(EventConditionType.QuizOfSirius);
            
            for (int i = 0; i < RewardListUIItem.Count; i++)
                RewardListUIItem[i].Refresh(EventControl.EventInfoList[EventConditionType.QuizOfSirius].rewardList[i]);
        }
    }
}