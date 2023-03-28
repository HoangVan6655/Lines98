using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

internal class Listenner
{
    public object caller = null;
    private System.Reflection.MethodInfo callback = null;

    public Listenner(object target, System.Reflection.MethodInfo callback)
    {
        this.caller = target;
        this.callback = callback;
    }

    public void Invoke(params object[] args)
    {
        if (caller != null && callback != null)
        {
            callback.Invoke(caller, args);
        }
    }
}

public class EventDispatcher : ClassSingleton<EventDispatcher>
{
    private static readonly Dictionary<GameEvent, List<Listenner>> MSubscribers = new Dictionary<GameEvent, List<Listenner>>();

    ////
    ///methodName: must be public
    ///

    public static void Register(object caller, GameEvent eventID, string methodName)
    {
        var type = caller.GetType();
        System.Reflection.MethodInfo invoker = type.GetMethod(methodName);

        if (!MSubscribers.ContainsKey(eventID))
            MSubscribers[eventID] = new List<Listenner>();

        if (!IsSubscriberExists(caller, eventID))
            MSubscribers[eventID].Add(new Listenner(caller, invoker));
    }

    public static void Dispatch(GameEvent eventID, params object[] args)
    {
        if (!MSubscribers.TryGetValue(eventID, out var subscribers))
            return;

        foreach (var listenner in subscribers)
        {
            listenner.Invoke(args);
        }
    }

    public static void UnRegister(object caller, GameEvent eventID)
    {
        if (!MSubscribers.TryGetValue(eventID, out var subscribers))
            return;

        MSubscribers[eventID].RemoveAll(l => l.caller == caller);

        if (MSubscribers[eventID].Count == 0) MSubscribers.Remove(eventID);
    }

    private static bool IsSubscriberExists(object caller, GameEvent eventID)
    {
        if (!MSubscribers.TryGetValue(eventID, out var subscribers)) return false;

        bool exists = false;

        for (int i = 0; i < subscribers.Count; i++)
        {
            if (subscribers[i] == caller)
            {
                exists = true;
                break;
            }
        }

        return exists;
    }

    public static void UnRegisterAllInTarget(object caller)
    {
        foreach (var subscribers in MSubscribers.Values)
        {
            subscribers.RemoveAll(l => l.caller == caller);
        }
    }
}

