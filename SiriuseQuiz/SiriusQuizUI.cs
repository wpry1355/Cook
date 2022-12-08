namespace Ekkorr.Cook
{
    using Cook.UI;
    using Ekkorr.Cook.Network;
    using Protocol.GameWebAndClient;
    using Protocol.GameWebAndClient.CGW;
    using Protocol.GameWebAndClient.GWC;
    using Protocol.GameWebAndClient.SharedDataModels;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class SiriusQuizUI : MonoBehaviour
    {
        [SerializeField] List<SiriusQuizAnswerUIItem> QuizItemUIList;
        [SerializeField] List<Toggle> QuizPrograss;
        [SerializeField] TextMeshProUGUI QuizIndex;
        [SerializeField] TextMeshProUGUI Question;
        [SerializeField] TextMeshProUGUI NextBtnText;
        [SerializeField] public TextMeshProUGUI NextDimText;
        [SerializeField] TextMeshProUGUI PrevBtnText;
        [SerializeField] public TextMeshProUGUI PrevDimText;
        [SerializeField] TextMeshProUGUI ResultBtnText;
        [SerializeField] TextMeshProUGUI EraseBtnText;
        [SerializeField] TextMeshProUGUI EraseDimText;
        [SerializeField] TextMeshProUGUI GotoMainText;
        [SerializeField] TextMeshProUGUI SiriusEraserText;
        
        [SerializeField] TextMeshProUGUI SiriusEraserCountText;
        [SerializeField] ToggleGroup AnswerItemToggleGroup;
        [SerializeField] GameObject EraseImage;
        [SerializeField] GameObject EraserBtnDim;
        [SerializeField] GameObject IsRightAnimation;
        [SerializeField] Image NextBtnDim;
        [SerializeField] Image PrevBtnDim;
        [SerializeField] Image NonClick;
        [SerializeField] Button ResultBtn;
        [SerializeField] Button EraseBtn;
        [SerializeField] Animator OXAnimator;

        List<int> DisableBtnList;
        int QuestionCount = 0;
        bool IsErase = false;
        bool IsInit = false;
        List<QuizOfSiriusSubmittedAnswerData> SubmittedAnswerData;

        public void Init()
        {
            gameObject.SetActive(true);
            Question.text = "";
            IsRightAnimation.SetActive(false);
            EraseImage.gameObject.SetActive(false);
            QuestionCount = SiriusQuizControl.QuizDataList.Count;
            IsErase = SiriusQuizControl.IsErase;
            QuestionUpdate();
            PrograssUpdate();
            MoveButtonUpdate();
            ResultBtn.gameObject.SetActive(!IsErase);
            EraseBtn.gameObject.SetActive(IsErase);
            SetTranslation();
            IsInit = true;
        }
        public void Init(List<QuizOfSiriusSubmittedAnswerData> submittedAnswerData)
        {
            SubmittedAnswerData = submittedAnswerData;
            gameObject.SetActive(true);
            Question.text = "";
            SiriusQuizControl.NowQuizIndex = 0;
            SiriusQuizControl.ResetEraseChoiceList();
            IsRightAnimation.SetActive(true);
            EraseImage.gameObject.SetActive(true);
            EraseBtn.gameObject.SetActive(true);
            QuestionCount = SiriusQuizControl.WrongQuizDataList.Count;
            IsErase = SiriusQuizControl.IsErase;
            QuestionUpdate();
            PrograssUpdate();
            MoveButtonUpdate();
            ResultBtn.gameObject.SetActive(!IsErase);
            EraseBtn.gameObject.SetActive(IsErase);
            SetTranslation();
        }

        private void SetTranslation()
        {
            PrevBtnText.text = StringsHelper.GetMessage(12070);
            PrevDimText.text = StringsHelper.GetMessage(12070);
            NextBtnText.text = StringsHelper.GetMessage(12071);
            NextDimText.text = StringsHelper.GetMessage(12071);
            ResultBtnText.text = StringsHelper.GetMessage(12072);
            EraseBtnText.text = StringsHelper.GetMessage(12073);
            EraseDimText.text = StringsHelper.GetMessage(12073);
            GotoMainText.text = StringsHelper.GetMessage(12096);
            SiriusEraserText.text = StringsHelper.GetMessage(12066);
            SiriusEraserCountText.text = StringsHelper.GetMessage(10258, SiriusQuizControl.EraseCount, 3);

        }
        private void QuestionUpdate()
        {
            //질문 및 답안 리스트 작성
            QuizIndex.text = StringsHelper.GetMessage(12089, SiriusQuizControl.NowQuizIndex + 1);

            if (IsErase == false)
            {
                InitQuestion();
            }
            else
            {
                InitEraseQuestion();
            }
        }
        private void InitQuestion()
        {
            Question.text = SiriusQuizControl.QuizDataList[SiriusQuizControl.NowQuizIndex].Question;
            for (int i = 0; i < QuizItemUIList.Count; i++)
            {

                QuizItemUIList[i].Init(i, SiriusQuizControl.QuizDataList[SiriusQuizControl.NowQuizIndex].AnswerItem[i], SiriusQuizControl.MyChoiceAnswer);
                if (SiriusQuizControl.MyChoiceAnswer[SiriusQuizControl.NowQuizIndex] == i)
                {
                    QuizItemUIList[i].Toggle.isOn = true;
                }
                else
                {
                    QuizItemUIList[i].ToggleOff();
                }
            }
        }
        private void InitEraseQuestion()
        {
            if (SiriusQuizControl.WrongQuizDataList[SiriusQuizControl.NowQuizIndex].Status == SystemEnums.QuizOfSiriusQuizStatus.Correct)
            {
                NonClick.gameObject.SetActive(true);
                EraserBtnDim.gameObject.SetActive(true);
                OXAnimator.SetTrigger("Correct");
            }
            else if (SiriusQuizControl.WrongQuizDataList[SiriusQuizControl.NowQuizIndex].Status == SystemEnums.QuizOfSiriusQuizStatus.Wrong)
            {
                NonClick.gameObject.SetActive(false);
                EraserBtnDim.gameObject.SetActive(false);
                OXAnimator.SetTrigger("Wrong");
            }


            for (int i = 0; i < QuizItemUIList.Count; i++)
            {
                QuizItemUIList[i].Init(i, SiriusQuizControl.WrongQuizDataList[SiriusQuizControl.NowQuizIndex].AnswerItem[i], SiriusQuizControl.MyEraseChoiceAnswer);
            }
            QuizIndex.text = StringsHelper.GetMessage(12089, SiriusQuizControl.WrongQuizDataList[SiriusQuizControl.NowQuizIndex].QuizId);
            Question.text = SiriusQuizControl.WrongQuizDataList[SiriusQuizControl.NowQuizIndex].Question;

            DisableBtnList = FindSummittdDataByQuizId(SiriusQuizControl.WrongQuizDataList[SiriusQuizControl.NowQuizIndex].QuizId, SubmittedAnswerData);
            //제출했던 문제의 답과 i가 같다면, 까맣게 칠해준다.
            for (int i = 0; i < DisableBtnList.Count; i++)
            {
                QuizItemUIList[DisableBtnList[i] - 1].SetDisable(true);
            }
            if (SiriusQuizControl.WrongQuizDataList[SiriusQuizControl.NowQuizIndex].Status == SystemEnums.QuizOfSiriusQuizStatus.Correct)
                QuizItemUIList[DisableBtnList[DisableBtnList.Count-1] - 1].SetDisable(false);
            for (int i = 0; i < QuizItemUIList.Count; i++)
            {

                if (SiriusQuizControl.MyEraseChoiceAnswer[SiriusQuizControl.NowQuizIndex] == i)
                {
                    QuizItemUIList[i].Toggle.isOn = true;
                }
                else
                {
                    QuizItemUIList[i].ToggleOff();
                }
            }

        }

        private List<int> FindSummittdDataByQuizId(int QuizID, List<QuizOfSiriusSubmittedAnswerData> submittedAnswerDatas)
        {
            List<int> Data = new List<int>();
            for (int i = 0; i < submittedAnswerDatas.Count; i++)
            {
                if (submittedAnswerDatas[i].QuizId == QuizID)
                    Data.Add(submittedAnswerDatas[i].Submitted);

            }
            return Data;
        }
        private void PrograssUpdate()
        {
            if (!IsErase)
                QuizPrograss[SiriusQuizControl.NowQuizIndex].isOn = true;
            else
                QuizPrograss[SiriusQuizControl.WrongQuizDataList[SiriusQuizControl.NowQuizIndex].QuizId-1].isOn = true;
        }

        #region[Button]

        public void MoveButtonUpdate()
        {
            PrevBtnDim.gameObject.SetActive(false);
            NextBtnDim.gameObject.SetActive(false);
            if (SiriusQuizControl.NowQuizIndex == 0)
            {
                PrevBtnDim.gameObject.SetActive(true);
            }
            if (SiriusQuizControl.NowQuizIndex == QuestionCount - 1)
            {
                NextBtnDim.gameObject.SetActive(true);
            }
        }


        public void NotSelectedChoice(bool IsClose)
        {
            if (AnswerItemToggleGroup.AnyTogglesOn())
                return;
            else if (!IsErase)
            {
                SiriusQuizControl.MyChoiceAnswer[SiriusQuizControl.NowQuizIndex] = -1;
            }
            else if (IsErase)
            {
                SiriusQuizControl.MyEraseChoiceAnswer[SiriusQuizControl.NowQuizIndex] = -1;
            }

        }
        public void OnClickNextBtn()
        {
            NotSelectedChoice(false);
            SiriusQuizControl.NextQuestion();
            MoveButtonUpdate();
            QuestionUpdate();
            PrograssUpdate();

        }
        public void OnClickPrevBtn()
        {
            NotSelectedChoice(false);
            SiriusQuizControl.PrevQuestion();
            MoveButtonUpdate();
            QuestionUpdate();
            PrograssUpdate();
        }

        // 2030700 시리우스 퀴즈
        // 10011 취소
        // 10010 확인
        public void OnClickResultBtn()
        {
            CopyCatUIManager.instance.ShowConfirm(StringsHelper.GetMessage(2032700), StringsHelper.GetMessage(12078), StringsHelper.GetMessage(10011), StringsHelper.GetMessage(10010), null, () =>
            {
                if (CheckAllSolve()&&!CheckNotSelected())
                {
                    //서버 연동 해서 체크진행
                    SiriusQuizControl.IsEnd = true;
                    ShowResultUI();
                    gameObject.SetActive(false);
                }
                else
                {
                    CopyCatUIManager.instance.ShowAlert(StringsHelper.GetMessage(2032700), StringsHelper.GetMessage(12079));
                }
            });
        }

        public void OnClickEraseBtn()
        {
            if (!(CheckAllCorrect()))
                SubmitEraseChoice();
            else
                CompleteQuiz();
        }

        public async void OnClickGotoMainBtn()
        {
            if (IsErase)
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
                Dispose();
                SiriusQuizControl.QuizLobbyUIInit(response.QuizDatas);
            }
            else
            {
                Dispose();
                SiriusQuizControl.QuizLobbyUIInit(SiriusQuizControl.response.QuizDatas);
            }


        }


        public void SetMoveBtnActive(bool isActivity)
        {
            NextBtnDim.gameObject.SetActive(isActivity);
            PrevBtnDim.gameObject.SetActive(isActivity);

        }
        #endregion


        public async void ShowResultUI()
        {
            SubmitAnswersForQuizOfSirius request = new SubmitAnswersForQuizOfSirius()
            {
                QuizAnswers = SiriusQuizControl.MakeAnswerForQuizOfSirius(SiriusQuizControl.QuizDataList, SiriusQuizControl.MyChoiceAnswer),
                EventId = SiriusQuizControl.EventData.eventId
            };
            var response = await WebServerClientModule.Instance.CommonRequest<SubmitAnswersForQuizOfSirius, SubmitAnswersForQuizOfSiriusResponse>(request);
            if (response.IsNullOrDestroy() || !response.Ok)
            {
                if (response.IsNullOrDestroy())
                    return;
                MessageControl.FailReason(response.FailReason);
                return;
            }

            SiriusQuizControl.SetQuizIndex(response.QuizDatas);
            EventControl.UpdateEvents(response.EventDataDiffs);
            SiriusQuizControl.SiriusQuizLobby.SiriusQuizResult.InitResult(response);
            SiriusQuizControl.SiriusQuizLobby.SiriusQuizResult.gameObject.SetActive(true);
        }

        public void SubmitEraseChoice()
        {
            CopyCatUIManager.instance.ShowConfirm(StringsHelper.GetMessage(2032700), StringsHelper.GetMessage(12076), StringsHelper.GetMessage(10011), StringsHelper.GetMessage(10010), null, () =>
            {
                if (SiriusQuizControl.EraseCount == 0)
                    CopyCatUIManager.instance.ShowAlert(StringsHelper.GetMessage(2032700), StringsHelper.GetMessage(12077));
                else if (checkEraseChoice(SiriusQuizControl.NowQuizIndex))
                    CopyCatUIManager.instance.ShowAlert(StringsHelper.GetMessage(2032700), StringsHelper.GetMessage(12079));
                else if (SiriusQuizControl.WrongQuizDataList[SiriusQuizControl.NowQuizIndex].Status == SystemEnums.QuizOfSiriusQuizStatus.Correct)
                    CopyCatUIManager.instance.ShowAlert(StringsHelper.GetMessage(2032700), StringsHelper.GetMessage(12075));
                else
                    UpdataCorrectQuestion();
            }
            );
        }
        
        private void RefreshSubmittedData(List<QuizOfSiriusSubmittedAnswerData> Data)
        {
            SubmittedAnswerData = Data;
        }

        private bool checkEraseChoice(int Index)
        {
            if (SiriusQuizControl.MyEraseChoiceAnswer[Index] == -1 || CheckNotSelected())
                return true;
            else
                return false;
        }
        private void CompleteQuiz()
        {
            CopyCatUIManager.instance.ShowAlert(StringsHelper.GetMessage(2032700), StringsHelper.GetMessage(12092), () => { OnClickGotoMainBtn(); });
        }
        public async void UpdataCorrectQuestion()
        {
            if (SiriusQuizControl.MyEraseChoiceAnswer[SiriusQuizControl.NowQuizIndex] == -1)
                return;
            RequestEraseAndSubmitAnswerForQuizOfSirius request = new RequestEraseAndSubmitAnswerForQuizOfSirius()
            {
                EventId = SiriusQuizControl.EventData.eventId,
                QuizAnswer = new AnswerForQuizOfSirius()
                {
                    QuizId = SiriusQuizControl.WrongQuizDataList[SiriusQuizControl.NowQuizIndex].QuizId,
                    Answer = SiriusQuizControl.MyEraseChoiceAnswer[SiriusQuizControl.NowQuizIndex] + 1
                },
            };
            var response = await WebServerClientModule.Instance.CommonRequest<RequestEraseAndSubmitAnswerForQuizOfSirius, RequestEraseAndSubmitAnswerForQuizOfSiriusResponse>(request);
            if (response.IsNullOrDestroy() || !response.Ok)
            {
                if (response.IsNullOrDestroy())
                    return;
                MessageControl.FailReason(response.FailReason);
                return;
            }
            RefreshSubmittedData(response.SubmittedAnswers);
            SiriusQuizControl.SetEraseCount(response.ItemDataDiff.Amount);
            SiriusEraserCountText.text = StringsHelper.GetMessage(10258, SiriusQuizControl.EraseCount, 3);
            if (response.QuizData.Status == SystemEnums.QuizOfSiriusQuizStatus.Wrong)
            {
                QuizItemUIList[SiriusQuizControl.MyEraseChoiceAnswer[SiriusQuizControl.NowQuizIndex]].SetDisable(true);
                QuizItemUIList[SiriusQuizControl.MyEraseChoiceAnswer[SiriusQuizControl.NowQuizIndex]].ToggleOff();
                SiriusQuizControl.MyEraseChoiceAnswer[SiriusQuizControl.NowQuizIndex] = -1;
                OXAnimator.SetTrigger("Wrong");
            }
            else if (response.QuizData.Status == SystemEnums.QuizOfSiriusQuizStatus.Correct)
            {
                NonClick.gameObject.SetActive(true);
                OXAnimator.SetTrigger("Correct");
                EraserBtnDim.SetActive(true);
            }
            EventControl.UpdateEvents(response.EventDataDiffs);

            SiriusQuizControl.EventRewardData = EventControl.EventInfoList[EventConditionType.QuizOfSirius].rewardList;
            SiriusQuizControl.SetWrongQuizData(response.QuizData);
            MoveButtonUpdate();
        }


        private bool CheckAllSolve()
        {
            bool check = true;
            foreach (int data in SiriusQuizControl.MyChoiceAnswer)
            {
                if (data == -1)
                    check = false;
            }
            return check;
        }
        private bool CheckAllCorrect()
        {
            bool check = true;
            for(int i = 0; i<SiriusQuizControl.WrongQuizDataList.Count;i++)
            {
                if (SiriusQuizControl.WrongQuizDataList[i].Status != SystemEnums.QuizOfSiriusQuizStatus.Correct)
                    check = false;
            }
            return check;
        }
        private bool CheckNotSelected()
        {
            bool CheckNotSelected = true;
            for (int i = 0; i < QuizItemUIList.Count; i++)
            {
                if (QuizItemUIList[i].Toggle.isOn == true)
                {
                    CheckNotSelected = false;
                }
            }
            return CheckNotSelected;
        }

        public void Dispose()
        {
            if (IsInit == true && !IsErase)
            {
                if (CheckNotSelected())
                {
                    SiriusQuizControl.MyChoiceAnswer[SiriusQuizControl.NowQuizIndex] = -1;
                }
            }
            for (int i = 0; i < QuizItemUIList.Count; i++)
            {
                QuizItemUIList[i].ToggleOff();
            }
            gameObject.SetActive(false);
            foreach (var data in QuizPrograss)
                data.isOn = false;
            IsInit = false;

        }
        public void Final()
        {
            if (CheckNotSelected() && !IsErase)
            {
                SiriusQuizControl.MyChoiceAnswer[SiriusQuizControl.NowQuizIndex] = -1;
            }
            for (int i = 0; i < QuizItemUIList.Count; i++)
            {
                QuizItemUIList[i].ToggleOff();
            }
            gameObject.SetActive(false);
            foreach (var data in QuizPrograss)
                data.isOn = false;
        }
    }
}

