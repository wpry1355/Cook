
namespace Ekkorr.Cook {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using ClientDataContainer;
    using ClientDataContainer.Interfaces;
    using Protocol.GameWebAndClient;

    public class EventDailyActionRecordUIItem : MonoBehaviour
    {
        [SerializeField] GameObject CompleteOn;
        [SerializeField] TextMeshProUGUI Description;
        [SerializeField] Image NavigationDisable;

        DailyActionRecordTemplate TemplateData;
        public void init(int Index, SystemEnums.EventMissionStatus NowStatus)
        {
            bool IsComplete = false;
            TemplateData = TemplateContainer<DailyActionRecordTemplate>.Find(Index);
            IsComplete = NowStatus.Equals(SystemEnums.EventMissionStatus.GiveReward);
        
            SetTranslation();
            CompleteOn.SetActive(IsComplete);
            NavigationDisable.gameObject.SetActive(IsComplete);
        }

        void SetTranslation()
        {
            Description.text = TemplateData.DescriptionRef.Get();
        }



        public void OnClickNavigationBtn()
        {
            RedirectionControl.Redirection(TemplateData.QuickGo, () => 
            {
                EventControl.eventUI.Close();
                QuestControl.ShowQuickUI(TemplateData.QuickGo, TemplateData.DescriptionRef.Get()); 
                
            });
        }


    }
}