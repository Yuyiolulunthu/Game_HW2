using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultBtn : MonoBehaviour
{
    public void OnClick_Menu()
    {
        GameSession.I.ToMenu();
    }

    public void OnClick_Replay()
    {
        GameSession.I.Replay();
    }
}
