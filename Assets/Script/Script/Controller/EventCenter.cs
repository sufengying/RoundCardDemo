using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventCenter : Singletion<EventCenter>
{
    private Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();
#region 添加事件
    public void AddListener(string eventName, Action action)
    {
        if (eventTable.TryGetValue(eventName, out var existing))
        {
            eventTable[eventName] = Delegate.Combine(existing, action);
        }
        else
        {
            eventTable[eventName] = action;
        }

    }
    public void AddListener<T>(string eventName, Action<T> action)
    {
        if (eventTable.TryGetValue(eventName, out var existing))
        {
            eventTable[eventName] = Delegate.Combine(existing, action);
        }
        else
        {
            eventTable[eventName] = action;
        }
    }
    public void AddListener<T1,T2>(string eventName, Action<T1,T2> action)
    {
        if (eventTable.TryGetValue(eventName, out var existing))
        {
            eventTable[eventName] = Delegate.Combine(existing, action);
        }
        else
        {
            eventTable[eventName] = action;
        }
    }
    
    public void AddListener<T1>(string eventName, Func<T1> action)
    {
        if (eventTable.TryGetValue(eventName, out var existing))
        {
            eventTable[eventName] = Delegate.Combine(existing, action);
        }
        else
        {
            eventTable[eventName] = action;
        }
    }
    public void AddListener<T1,T2>(string eventName, Func<T1,T2> action)
    {
        if (!eventTable.ContainsKey(eventName))
        {
            eventTable[eventName] = action;
        }

    }
    #endregion
#region 移除事件
    public void RemoveListener(string eventName,Action action)
    {
        if (eventTable.TryGetValue(eventName, out var existing))
        {
            existing = Delegate.Remove(existing, action);
            if (existing == null)
            {
                eventTable.Remove(eventName);
            }
            else
            {
                eventTable[eventName] = existing;
            }
        }
    }
    public void RemoveListener(string eventName)
    {
        if (eventTable.TryGetValue(eventName, out var existing))
        {
            eventTable.Remove(eventName);
        }
    }
    #endregion
#region 触发事件
    public void TriggerAction(string eventName)
    {
        if (eventTable.TryGetValue(eventName, out var action))
        {
            (action as Action)?.Invoke();
        }
    }
    public void TriggerAction<T>(string eventName,T data)
    {
        if (eventTable.TryGetValue(eventName, out var action))
        {
            (action as Action<T>)?.Invoke(data);
        }
    }
    public void TriggerAction<T1,T2>(string eventName,T1 data1,T2 data2)
    {
        if (eventTable.TryGetValue(eventName, out var action))
        {
            (action as Action<T1,T2>)?.Invoke(data1,data2);
        }
    }
    public T1 TriggerAction<T1>(string eventName)
    {
        if (eventTable.TryGetValue(eventName, out var action))
        {
            var func = action as Func<T1>;
            if (func != null)
            {
                return func.Invoke();
            }
        }
        return default;
    }
    public T2 TriggerAction<T1,T2>(string eventName,T1 data1)
    {
        if (eventTable.TryGetValue(eventName, out var action))
        {
            var func = action as Func<T1,T2>;
            if (func != null)
            {
                return func.Invoke(data1);
            }
        }
        return default;
    }

    #endregion
    public void OnDestroy()
    {
        eventTable.Clear();

    }
}
