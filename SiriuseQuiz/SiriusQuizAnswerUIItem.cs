namespace Ekkorr.Cook
{
    using System.Collections;
    using System.Collections.Generic;
    using Protocol.GameWebAndClient;
    using Ekkorr.UICompoenent;

    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public class SiriusQuizAnswerUIItem : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI AnswerContents;
        [SerializeField] GameObject SelectedPanel;
        [SerializeField] public GameObject DisablePnael;
        [SerializeField] public Toggle Toggle;
        [SerializeField] public ToggleGroup ToggleGroup;

        private int[] SetChoiceList;
        private int Index;

        public void Init(int Index, int ContentsIndex, Dictionary<int, List<string>> AnswerContents)
        {
            this.Index = Index;
            SetTranslation(Index, AnswerContents[ContentsIndex]);

        }
        public void Init(int Index ,string QuizDataAnswerContents, int[] setChoiceList)
        {
            this.Index = Index;
            AnswerContents.text = QuizDataAnswerContents;
            SetChoiceList = setChoiceList;
            SetDisable(false);
            Toggle.isOn = false;
            Toggle.interactable = true;
        }

        public void ToggleOff()
        {
            Toggle.isOn = false;
        }
        public void Dispose()
        {

            DisablePnael.SetActive(false);
            GetComponent<Toggle>().interactable = true;
        }
        private void SetTranslation(int Index,List< string> AnswerContensts)
        {
            // 문제 스트링 적용
            AnswerContents.text = AnswerContensts[Index];
            
        }
        public void SetDisable(bool isBool)
        {
            DisablePnael.SetActive(isBool);
            GetComponent<Toggle>().interactable = false;
        }


        public void OnClickEvent()
        {
            if (Toggle.isOn == true)
            {
                    SiriusQuizControl.NowChoiceNumber = Index;
                    SetChoiceList[SiriusQuizControl.NowQuizIndex] = Index;
                if(SiriusQuizControl.IsErase && SiriusQuizControl.WrongQuizDataList[SiriusQuizControl.NowQuizIndex].Status == SystemEnums.QuizOfSiriusQuizStatus.Wrong)
                {
                    SiriusQuizControl.SiriusQuizLobby.SiriusQuiz.SetMoveBtnActive(true);
                }
                
            }
            if (SiriusQuizControl.IsErase)
            {
                if (ToggleGroup.AnyTogglesOn() == false)
                {
                    SiriusQuizControl.SiriusQuizLobby.SiriusQuiz.MoveButtonUpdate();
                }

            }
        }
    }
}
