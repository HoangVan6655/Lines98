using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallEntity
{
    public enum Color { None = -1, Red, Green, Blue, Cyan, Magenta, Yellow, Brown, Ghost }
    public enum Size { Dot, Ball };
    public enum State { Idle, Selected }

    public Color color = Color.None;
    public Size size = Size.Dot;
    public State state = State.Idle;

    public bool selected { get; protected set; }
    public Action onSelectedChange;

    public BallEntity()
    {

    }

    public void Reset()
    {
        color = Color.None;
        size = Size.Dot;
        state = State.Idle;
    }

    public void SetSelection(bool select)
    {
        if (selected == select) return;
        selected = select;
        onSelectedChange?.Invoke();
    }
}
