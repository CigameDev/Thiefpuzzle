using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventDispatcher : MonoBehaviour
{
    #region Singleton
    private static EventDispatcher s_intance;
    public static EventDispatcher Instance => s_intance;
    
    public  static bool HasIntance()
    {
        return s_intance != null;
    }
    void Awake()
    {
        if(s_intance)
        {
            Destroy(gameObject);
        }  
        else
        {
            s_intance = this as EventDispatcher;
        }    
    }
    void OnDestroy()
    {
        if(s_intance == this)
        {
            s_intance = null;
        }
    }
    #endregion
    #region Fields
    Dictionary<EventId,Action<object>> _listeners = new Dictionary<EventId,Action<object>>();

    #endregion
    #region Add listeners, Post event,remove listeners
    public void RegisterListener (EventId eventId,Action<object>callback)
    {
        if(_listeners.ContainsKey(eventId))
        {
            //neu da co key nay trong dictionary
            _listeners[eventId]+=callback;
        }   
        else
        {
            //neu chua co phai them eventId do vao da
            _listeners.Add(eventId, null);
            _listeners[eventId]+=callback;
            //_listeners.Add(eventId, callback);//test xem cau nay co tuong duong voi 2 cau tren khong
        }    
    }    
    public void PostEvent(EventId eventId, object param = null)
    {
        if (!_listeners.ContainsKey(eventId)) return;//neu khong co key nay thi lam an gi nua
        var callbacks = _listeners[eventId];
        if(callbacks != null)
        {
            callbacks(param);
        }   
        else
        {
            _listeners.Remove(eventId);
        }    
    }    
    public void RemoveListener(EventId eventId,Action<object>callback)
    {
        if (_listeners.ContainsKey(eventId))
        {
            _listeners[eventId]-=callback;
        }    
    }    
    public void ClearListener()
    {
        _listeners.Clear();
    }
    #endregion

   
}
#region Extention Class
//class extention mo rong de su dung EventDispatcher 1 cach thuan tien hon ,khong phai instance nhieu
public static class EventDispatcherExtention
{
    public static void RegisterListener(this MonoBehaviour listener, EventId eventId, Action<object> callback)
    {
        EventDispatcher.Instance.RegisterListener(eventId, callback);
    }
    //dang su kien co thong so (param)
    public static void PostEvent(this MonoBehaviour listener ,EventId eventId,object param)
    {
        EventDispatcher.Instance.PostEvent(eventId, param);
    }
    //dang su kien khong co thong so (no param)
    public static void PostEvent(this MonoBehaviour listener,EventId eventId)
    {
        EventDispatcher.Instance.PostEvent(eventId, null);
    }
    
    public static void RemoveListener(this MonoBehaviour listener ,EventId eventId,Action<object>callback)
    {
        EventDispatcher.Instance.RemoveListener(eventId,callback);
    }
}
#endregion
