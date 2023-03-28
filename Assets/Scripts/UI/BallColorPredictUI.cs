using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//cung cấp các màu của bóng
public sealed class BallColorPredictUI : MonoBehaviour
{
    public Image ballColor1;
    public Image ballColor2;
    public Image ballColor3;

    private void OnEnable()
    {
        EventDispatcher.Register(this, GameEvent.GE_ADD_DOT_COLOR, "OnAddDotColor");
    }

    private void OnDisable()
    {
        EventDispatcher.UnRegister(this, GameEvent.GE_ADD_DOT_COLOR);
    }

    public void OnAddDotColor(int color1, int color2, int color3)
    {
        ballColor1.color = Utilities.ToColor(color1);
        ballColor2.color = Utilities.ToColor(color2);
        ballColor3.color = Utilities.ToColor(color3);
    }
}
