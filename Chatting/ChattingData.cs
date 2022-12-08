using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ekkorr.Cook
{
    public abstract class ChattingData
    {
        public enum MessageType
        {
            MyMsg,
            SomeoneMsg,
            Date,
            System,
            MyDailyPresent,
            SomeoneDailyPresent
        }

        public long id;
        public int elapsedSeconds;
        public string message;
        public MessageType messageType;

        public ChattingData(long id, int elapsedSeconds, string message, MessageType messageType)
        {
            this.id = id;
            this.elapsedSeconds = elapsedSeconds;
            this.message = message;
            this.messageType = messageType;
        }

        public ChattingData(string message, MessageType messageType)
        {
            id = -1;
            elapsedSeconds = -1;
            this.message = message;
            this.messageType = messageType;
        }

        public virtual int GetSenderPortraitIconTempId()
        {
            return -1;
        }

        public virtual string GetSenderNickname()
        {
            return string.Empty;
        }
    } 
}
