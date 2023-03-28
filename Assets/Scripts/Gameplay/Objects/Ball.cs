using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : ObjectPool
{
    public enum Color { None = -1, Red, Green, Blue, Cyan, Magenta, Yellow, Brown, Ghost }
    public enum Size { Dot, Ball };
    public enum State { Idle, Selected }

    public Color color { get; protected set; }
    public Size size { get; protected set; }
    public State state { get; protected set; }

    public bool selected { get; protected set; }
    public Action onSelectedChange;

    public ParticleSystem explosiveVfx;
    //
    protected int[] m_ColorTable = { 0xffffff, 0xcd0027, 0x509d28, 0x1d57a9, 0x54b9c1, 0xdd6ba7, 0xffe001, 0x4f3401, 0xe47a0a };
    protected Animator m_Animator;

    void Awake() => Initialize();

    protected virtual void Initialize()
    {
        m_Animator = GetComponent<Animator>();
        Reset();
    }

    public void Reset()
    {
        SetColor(Color.None);
        SetSize(Size.Dot);
        state = State.Idle;
    }

    public override void ResetObject()
    {
        Reset();
    }

    public void SetColor(Color color)
    {
        this.color = color;
        GetComponent<MeshRenderer>().material.color = Utilities.ToColor(m_ColorTable[(int)color + 1]);
    }

    public int GetColor32Code()
    {
        return m_ColorTable[(int)(this.color) + 1];
    }

    public void SetSelection(bool select)
    {
        if (selected == select) return;
        selected = select;
        SetTrigger(selected ? "selected" : "idle");
        onSelectedChange?.Invoke();
    }

    public void SetSize(Size size)
    {
        if (this.size != size)
        {
            if (size == Size.Ball)
                SetTrigger("zoomOut");
            this.size = size;
        }
    }

    protected void SetTrigger(string trigger)
    {
        if (m_Animator && !string.IsNullOrEmpty(trigger))
        {
            m_Animator.SetTrigger(trigger);
        }
    }

    public void Explosive()
    {
        var effect = Instantiate(explosiveVfx, transform.position, Quaternion.identity);
        effect.GetComponent<Renderer>().material.color = Utilities.ToColor(m_ColorTable[(int)color + 1]);
        Destroy(effect, 0.8f);
    }
}
