using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ekkorr.UICompoenent.List;

using TMPro;

namespace Ekkorr.Cook
{
    public class ChattingListItem : EKListItemBase<ChattingData>
    {
#pragma warning disable CS0649
        [Header("My Message")]
        [SerializeField] private GameObject myMessageContainer;
        [SerializeField] private TextMeshProUGUI txtMyMessage;
        [SerializeField] private TextMeshProUGUI txtMySentDate;

        [Header("My Daily Present")]
        [SerializeField] private GameObject myDailyPresentContainer;
        [SerializeField] private TextMeshProUGUI txtMyDailyPresent;
        [SerializeField] private TextMeshProUGUI txtMyDailyPresentSentDate;

        [Header("Someone Message")]
        [SerializeField] private GameObject someoneMessageContainer;
        [SerializeField] private Image someonePortraitIcon;
        [SerializeField] private TextMeshProUGUI txtSomeoneMessage;
        [SerializeField] private TextMeshProUGUI txtSomeoneNickname;
        [SerializeField] private TextMeshProUGUI txtSomeoneSentDate;

        [Header("Someone Daily Present")]
        [SerializeField] private GameObject someoneDailyPresentContainer;
        [SerializeField] private Image someoneDailyPresentPortraitIcon;
        [SerializeField] private TextMeshProUGUI txtSomeoneDailyPresent;
        [SerializeField] private TextMeshProUGUI txtSomeoneDailyPresentNickname;
        [SerializeField] private TextMeshProUGUI txtSomeoneDailyPresentSentDate;

        [Header("Date Line")]
        [SerializeField] private GameObject dateLineContainer;
        [SerializeField] private TextMeshProUGUI txtDateLine;

        [Header("System Message")]
        [SerializeField] private GameObject systemContainer;
        [SerializeField] private TextMeshProUGUI txtSystemLabel;
        [SerializeField] private TextMeshProUGUI txtSystemMessage;
        [SerializeField] private TextMeshProUGUI txtSystemSentDate;

#pragma warning restore CS0649

        private RectTransform sizeBindingTransform;
        private const float offsetY = 5f;
        public override void DrawItem(bool selected = false)
        {
            SetMessage();
        }

        public override void ClearItem()
        {
        }

        public override Vector2 GetItemSize()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(sizeBindingTransform);

            return new Vector2(sizeBindingTransform.sizeDelta.x, sizeBindingTransform.sizeDelta.y + offsetY);
        }

        private void SetMessage()
        {
            myMessageContainer.SetActive(_data.messageType == ChattingData.MessageType.MyMsg);
            someoneMessageContainer.SetActive(_data.messageType == ChattingData.MessageType.SomeoneMsg);
            dateLineContainer.SetActive(_data.messageType == ChattingData.MessageType.Date);
            if (GuildControl.ShowSystemMsg == true)
                systemContainer.SetActive(_data.messageType == ChattingData.MessageType.System);
            else
                systemContainer.SetActive(false);
            myDailyPresentContainer.SetActive(_data.messageType == ChattingData.MessageType.MyDailyPresent);
            someoneDailyPresentContainer.SetActive(_data.messageType == ChattingData.MessageType.SomeoneDailyPresent);

            switch (_data.messageType)
            {
                case ChattingData.MessageType.MyMsg:
                    txtMyMessage.text = _data.message;
                    txtMySentDate.text = ConvertToSentDate();
                    sizeBindingTransform = myMessageContainer.transform as RectTransform;
                    break;

                case ChattingData.MessageType.SomeoneMsg:
                    txtSomeoneMessage.text = _data.message;
                    txtSomeoneNickname.text = _data.GetSenderNickname();
                    txtSomeoneSentDate.text = ConvertToSentDate();
                    someonePortraitIcon.sprite = AtlasControl.GetProfile(_data.GetSenderPortraitIconTempId());
                    sizeBindingTransform = someoneMessageContainer.transform as RectTransform;
                    break;
                    
                case ChattingData.MessageType.Date:
                    txtDateLine.text = _data.message;
                    sizeBindingTransform = dateLineContainer.transform as RectTransform;
                    break;

                case ChattingData.MessageType.System:
                    if (GuildControl.ShowSystemMsg == true)
                    {
                        txtSystemMessage.text = _data.message;
                        txtSystemLabel.text = _data.GetSenderNickname();
                        txtSystemSentDate.text = ConvertToSentDate();
                    }
                    else
                    {
                        txtSystemMessage.text = "";
                        txtSystemLabel.text = "";
                        txtSystemSentDate.text = "";
                        RectTransform rectTransform = systemContainer.transform as RectTransform;
                        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, 0);
                    }
                    sizeBindingTransform = systemContainer.transform as RectTransform;
                    break;

                case ChattingData.MessageType.MyDailyPresent:
                    txtMyDailyPresent.text = _data.message;
                    txtMyDailyPresentSentDate.text = ConvertToSentDate();
                    sizeBindingTransform = myDailyPresentContainer.transform as RectTransform;
                    break;

                case ChattingData.MessageType.SomeoneDailyPresent:
                    txtSomeoneDailyPresent.text = _data.message;
                    txtSomeoneDailyPresentNickname.text = _data.GetSenderNickname();
                    txtSomeoneDailyPresentSentDate.text = ConvertToSentDate();
                    someoneDailyPresentPortraitIcon.sprite = AtlasControl.GetProfile(_data.GetSenderPortraitIconTempId());
                    sizeBindingTransform = someoneDailyPresentContainer.transform as RectTransform;
                    break;
            }
        }

        private string ConvertToSentDate()
        {
            var date = DateTime.UtcNow.ToLocalTime() - TimeSpan.FromSeconds(_data.elapsedSeconds);

            return string.Format("{0:00}:{1:00}", date.Hour, date.Minute);
        }
    }

}