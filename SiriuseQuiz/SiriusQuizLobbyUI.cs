namespace Ekkorr.Cook
{
    using System.Collections.Generic;
    
    using UnityEngine;
    using UnityEngine.UI;
    public class SiriusQuizLobbyUI : EventAbstractContentUI
    {
        [SerializeField] public SiriusQuizStartUI SiriusQuizStart;
        [SerializeField] public SiriusQuizResultUI SiriusQuizResult;
        [SerializeField] public SiriusQuizUI SiriusQuiz;

        private string eventId;


        public override void Init(int tabIndex)
        {
            base.Init(tabIndex);
            var data = EventControl.EventInfoList[eventType];
            eventId = data.eventId;
            index = tabIndex;
            SiriusQuizControl.Init(data,data.rewardList);
            SiriusQuizControl.SiriusQuizLobby = this;
            
            UIInit();         
        }

        public void UIInit()
        {
            SiriusQuizStart.gameObject.SetActive(false);
            SiriusQuizResult.gameObject.SetActive(false);
            SiriusQuiz.gameObject.SetActive(false);
        }
        public override void Show()
        {
            base.Show();
            SiriusQuizControl.SetEventData();
        }
        public override void Hide()
        {
            base.Hide();
            SiriusQuiz?.Dispose();
            SiriusQuizResult?.Dispose();
            SiriusQuizStart?.Dispose();
            UIInit();
        }
        public override void Final()
        {
            base.Final();
            SiriusQuiz?.Final();
            SiriusQuizResult?.Final();
            SiriusQuizStart?.Final();
            UIInit();
        }
    }
}
