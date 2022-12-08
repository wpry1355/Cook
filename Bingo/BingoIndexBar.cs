namespace Ekkorr.Cook
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;


    public class BingoIndexBar : MonoBehaviour
    {
        [SerializeField] GameObject On;
        [SerializeField] public GameObject Noti;

        public enum IsBingoStateOfIndexBar
        {
            None = 0,
            On = 1,
            Off = 2,
            Noti = 3

        }
        public void init()
        {
            SetState(IsBingoStateOfIndexBar.None);
        }

        public void SetState(IsBingoStateOfIndexBar BingoMapState)
        {
            switch (BingoMapState)
            {
                case IsBingoStateOfIndexBar.None:
                    {
                        On.SetActive(false);
                        Noti.SetActive(false);
                        return;
                    }
                case IsBingoStateOfIndexBar.On:
                    {
                        On.SetActive(true);
                        return;
                    }
                case IsBingoStateOfIndexBar.Off:
                    {
                        On.SetActive(false);
                        Noti.SetActive(false);
                        return;
                    }
                case IsBingoStateOfIndexBar.Noti:
                    {
                        Noti.SetActive(true);
                        return;
                    }
            }
        }

    }
}
