using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using System.IO;
using Assets.Code;
using UnityEngine.Events;
using Unity.Jobs;
using UnityEditor;


public static class NetworkingManager
{
    public static void SendMessage(NetworkDriver networkDriver, MessageHeader message, NetworkConnection id)
    {
        var writer = networkDriver.BeginSend(id);
        message.SerializeObject(ref writer);
        networkDriver.EndSend(writer);
    }

    public static MessageHeader ReadMessage<T>(DataStreamReader reader, Queue<MessageHeader> messageQueue) where T : MessageHeader, new()
    {
        var msg = new T();
        msg.DeserializeObject(ref reader);
        messageQueue.Enqueue(msg);
        return msg;
    }
}

