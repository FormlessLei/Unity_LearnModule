using System;
using System.Collections.Generic;
using UnityEngine;

public class Message
{
    public string type;
    public Message() { type = this.GetType().Name; }
}

public delegate bool MessageHandlerDelegate(Message message);

public class MessageSystem : SingletonComponent<MessageSystem>
{
    private Dictionary<string, List<MessageHandlerDelegate>> _listenerDict = new Dictionary<string, List<MessageHandlerDelegate>>();
    private Queue<Message> _messageQueue = new Queue<Message>();
    private const int _maxQueueProcessingTime = 16667;
    private System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();

    public bool AttachListener(System.Type type, MessageHandlerDelegate handler)
    {
        if (type == null)
        {
            Debug.LogWarning($"[Message] AttachListener failed due to having no message type");
            return false;
        }

        string msgType = type.Name;
        if (!_listenerDict.ContainsKey(msgType))
        {
            _listenerDict.Add(msgType, new List<MessageHandlerDelegate>());
        }

        List<MessageHandlerDelegate> list = _listenerDict[msgType];
        if (/*list == null || */list.Contains(handler))
        {
            return false; // 已存在，不必加
        }

        list.Add(handler);

        return true;
    }

    public bool DetachListener(System.Type type, MessageHandlerDelegate handler)
    {
        if (type == null)
        {
            Debug.LogWarning($"[Message] DetachListener failed due to having no message type");
            return false;
        }

        string msgType=type.Name;
        if (!_listenerDict.ContainsKey(type.Name))
        {
            return false;
        }

        List<MessageHandlerDelegate> list = _listenerDict[msgType];
        if (!list.Contains(handler))
        {
            return false;
        }

        list.Remove(handler);
        return true;
    }

    public bool QueueMessage(Message msg)
    {
        if (!_listenerDict.ContainsKey(msg.type)) return false;

        _messageQueue.Enqueue(msg);
        return true;
    }


    private void Update()
    {
        timer.Start();
        while (_messageQueue.Count > 0)
        {
            if (_maxQueueProcessingTime > 0.0f && timer.Elapsed.Milliseconds > _maxQueueProcessingTime)
            {
                timer.Stop();
                return;
            }

            Message msg = _messageQueue.Dequeue();
            if (!TriggerMessage(msg))
            {
                Debug.LogWarning("Error when processing message:" + msg.type);
            }
        }
    }

    private bool TriggerMessage(Message msg)
    {
        string msgType = msg.type;
        if (!_listenerDict.ContainsKey(msgType))
        {
            Debug.LogError($"message listener has no {msgType}");
            return false;
        }

        var listenerList = _listenerDict[msgType];
        for (int i = 0; i < listenerList.Count; i++)
        {
            if (listenerList[i](msg))
            {
                return true;
            }
        }
        return true;
    }
}