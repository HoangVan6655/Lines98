using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ObjectPool : MonoBehaviour
{
    [HideInInspector]
    public string poolName;

    public virtual void Destroy()
    {
        Recycle();
        ResetObject();
    }

    public virtual void ResetObject()
    {

    }

    public virtual void Recycle()
    {
        gameObject.SetActive(false);
        if (IsInPool())
            Pooler.instance.AttachObject(poolName, this);
        else
            Destroy(gameObject);

    }

    public bool IsInPool() => !string.IsNullOrEmpty(poolName);
}