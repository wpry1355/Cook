using Ekkorr.Cook;
using Ekkorr.Cook.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BingoSubmitUI :MonoBehaviour
{
    [SerializeField] TextMeshProUGUI SubmitBoxDishName;
    [SerializeField] TextMeshProUGUI SubmitBoxDishAcount;
    [SerializeField] TextMeshProUGUI SubmitBtnText;
    [SerializeField] GameObject Panel;
    public void SetSubmitBoxBoxTranslation(string Name, string Account)
    {
        SubmitBoxDishName.text = Name;
        SubmitBoxDishAcount.text = Account;
        SubmitBtnText.text = StringsHelper.GetMessage(10085);
    }
    public void ShowSubmitBox(Vector3 transform)
    {
        var Ract = GetComponent<RectTransform>();
        Ract.anchoredPosition = new Vector3(transform.x, transform.y, 0f);
        Panel.SetActive(true);
        gameObject.SetActive(true);
    }
    public void DisposeSubmitBox()
    {
        var rect = GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        Panel.SetActive(false);
        gameObject.SetActive(false);
    }
    public void OnClickSubmitBtn()
    {
        BingoControl.DishSubmitBtnClick(BingoControl.NowBingoItemSelectedIndex);
    }
}
