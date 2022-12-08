using Protocol.GameWebAndClient.SharedDataModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClientDataContainer;
using Protocol.GameWebAndClient;

public class SiriusQuizData 
{
    public int QuizId;
    public int TemplateId;
    public int Index;
    public string Question;
    public List<string> AnswerItem;
    public SystemEnums.QuizOfSiriusQuizStatus Status;
    public void init(int quizId, int templateId,int index, string question,List<string> answerItem, SystemEnums.QuizOfSiriusQuizStatus status = SystemEnums.QuizOfSiriusQuizStatus.None)
    {
        QuizId = quizId;
        TemplateId = templateId;
        Index = index;
        Question = question;
        AnswerItem = answerItem;
        Status = status;
    }

    public void init(QuizOfSiriusEventQuizData quizData, int index = -1)
    {
        QuizId = quizData.QuizId;
        TemplateId = quizData.TemplateId;
        Index = index;
        Status = quizData.Status;
        Question = TemplateContainer<QuizOfSiriusEventSampleTemplate>.Find(TemplateId).QuestionStringIdRef.Get();
        AnswerItem = MakeAnswerItem();
    }
    private List<string> MakeAnswerItem()
    {
        List<string> ListData = new List<string>();
        ListData.Add(TemplateContainer<QuizOfSiriusEventSampleTemplate>.Find(TemplateId).Choice.Choice1StringIdRef.Get());
        ListData.Add(TemplateContainer<QuizOfSiriusEventSampleTemplate>.Find(TemplateId).Choice.Choice2StringIdRef.Get());
        ListData.Add(TemplateContainer<QuizOfSiriusEventSampleTemplate>.Find(TemplateId).Choice.Choice3StringIdRef.Get());
        ListData.Add(TemplateContainer<QuizOfSiriusEventSampleTemplate>.Find(TemplateId).Choice.Choice4StringIdRef.Get());
        return ListData;
    }
}
