namespace Ekkorr.Cook{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;
    using UnityEngine.UI;
    using ClientDataContainer.Interfaces;
    using Ekkorr.Cook.UI;

    public class BingoNotAvailable : MonoBehaviour
    {

        [SerializeField] TextMeshProUGUI DescText;
        [SerializeField] TextMeshProUGUI RacipeBtnText;
        [SerializeField] Button GoToRacipeStorageBtn;


        private void OnEnable()
        {
            SetTranslaion();
            AddOnClickBtnEvent();
        }

        private void OnDisable()
        {
            Dispose();
        }
        private void Dispose()
        {
            RemoveOnClickBtnEvent();
        }

        private void SetTranslaion()
        {
            DescText.text = StringsHelper.GetMessage(12102);
            RacipeBtnText.text = StringsHelper.GetMessage(10744);
        }

        private void AddOnClickBtnEvent()
        {
            GoToRacipeStorageBtn.onClick.AddListener(GoToRacipeStorage);
        }
        private void RemoveOnClickBtnEvent()
        {
            GoToRacipeStorageBtn.onClick.RemoveAllListeners();
        }
        public void GoToRacipeStorage()
        {
            BingoControl.GotoRacipeStorage();
        }


    }
}