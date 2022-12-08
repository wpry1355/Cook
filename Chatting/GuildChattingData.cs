using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Protocol.GameWebAndClient;

namespace Ekkorr.Cook
{
    public class GuildChattingData : ChattingData
    {
        public int senderPortraitTempId;
        public string senderNickname;

        public GuildChattingData(long id, int elapsedSeconds, string message, int senderPortraitTempId, string senderNickname, MessageType messageType)
            :base(id, elapsedSeconds, message, messageType)
        {
            this.senderPortraitTempId = senderPortraitTempId;
            this.senderNickname = senderNickname;
        }

        public GuildChattingData(string message, MessageType messageType)
            : base(message, messageType)
        {
            senderPortraitTempId = -1;
            senderNickname = string.Empty;
        }

        public override int GetSenderPortraitIconTempId()
        {
            return senderPortraitTempId;
        }

        public override string GetSenderNickname()
        {
            return senderNickname;
        }
    }

}