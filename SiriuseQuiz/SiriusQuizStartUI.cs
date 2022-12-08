namespace Ekkorr.Cook
{
    using System.Collections;
    using System.Collections.Generic;
    using TMPro;
    using UnityEngine;
    using Cook.UI;

    public class SiriusQuizStartUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI EventTitleText;
        [SerializeField] TextMeshProUGUI EventSubText;
        [SerializeField] SiriusEraser SiriusEraserUI;
        
        public void init()
        {
            SiriusEraserUI.init();
            SetTranslation();
        }

        private void SetTranslation()
        {
            EventTitleText.text = StringsHelper.GetMessage(2032700);
            EventSubText.text = StringsHelper.GetMessage(12065);
        }


        public void OnStartBtnClick()
        {
            SiriusQuizControl.IsErase = false;
            SiriusQuizControl.StartQuiz();
        }
        public void OnClickHelpBtn()
        {   
            FacilityInformationControl.ShowFacilityInformationUI(E_FacilityInformationType.QuizOfSiriusEvent);
        }
        public void Dispose()
        {

        }
        public void Final()
        {
        }

    }

}