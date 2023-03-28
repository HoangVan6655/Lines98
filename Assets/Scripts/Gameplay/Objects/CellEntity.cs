using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellEntity
{
    public enum State { None, Block, Ball }
    public int rowOffset;
    public int colOffset;
    public Ball ball;
    public State state;

    public bool selected { get; protected set; }
    public Action onSelectedChange;

    public CellEntity()
    {

    }

    public void Reset()
    {
        SetSelection(false);
    }

    public void SetSelection(bool select)
    {
        if (selected == select) return;
        selected = select;
        ball?.SetSelection(selected);
        onSelectedChange?.Invoke();
    }


    public bool empty => ball == null;// && state < State.Block;

    public bool selectable => ball != null && ball.size == Ball.Size.Ball;

    public bool available => ball == null || ball.size <= Ball.Size.Dot;

    public void AttachBall(Ball ball)
    {
        if (!ball) return;

        if (this.ball != null) DettachBall().Destroy();

        this.ball = ball;
    }

    public Ball DettachBall()
    {
        Ball ball = this.ball;
        this.ball = null;
        return ball;
    }
}
