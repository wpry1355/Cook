using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Ekkorr.UICompoenent.List;

namespace Ekkorr.Cook
{
    public class ChattingEKFlexibleListEvent : EKListBaseEvent<ChattingData> { }

    public class ChattingEKFlexibleList : EKFlexibleListBase<ChattingData, ChattingListItem, ChattingEKFlexibleListEvent>
    {

    }

}