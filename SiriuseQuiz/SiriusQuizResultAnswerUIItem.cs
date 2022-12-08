using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Protocol.GameWebAndClient;

public class SiriusQuizResultAnswerUIItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Number;
    [SerializeField] GameObject RightUI;
    [SerializeField] GameObject WrongUI;

    SystemEnums.QuizOfSiriusQuizStatus QuizStatus;

    public void Init(int Index, SystemEnums.QuizOfSiriusQuizStatus QuizStatus)
    {
        RightUI.SetActive(false);
        WrongUI.SetActive(false);
        Number.text = Ekkorr.Cook.UI.StringsHelper.GetMessage(12089, Index);
        if (QuizStatus == SystemEnums.QuizOfSiriusQuizStatus.Correct)
            RightUI.SetActive(true);
        else if (QuizStatus == SystemEnums.QuizOfSiriusQuizStatus.Wrong)
            WrongUI.SetActive(true);

    }
    public void Dispose()
    {
        RightUI.SetActive(false);
        WrongUI.SetActive(false);
    }

}
