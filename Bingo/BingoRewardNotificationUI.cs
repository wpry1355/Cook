using CopyCat;
using Ekkorr.Cook;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BingoRewardNotificationUI : UIItem
{
    [SerializeField] TextMeshProUGUI Desc;
    string DescText;
    CopyCatCoroutine _coroutine;
    WaitForSeconds delay = new WaitForSeconds(0.7f);
    public void init(string desctext)
    {
        DescText = desctext;
        SetTransltion();
        if(_coroutine != null)
        {
            _coroutine.Dispose();
            _coroutine = null;
        }
        _coroutine = CopyCatCoroutiner.StartCoroutine(DelayClose());
    }
    private void SetTransltion()
    {
        Desc.text = DescText;
    }
    IEnumerator DelayClose()
    {
        yield return delay;
        Close();
    }
}
