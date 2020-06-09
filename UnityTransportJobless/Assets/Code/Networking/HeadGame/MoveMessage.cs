using UnityEngine;
using System;
using Unity.Networking.Transport;

public class MoveMessage : MessageHeader
{
    public Direction direction;

    public override MessageType Type => MessageType.MoveRequest;

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);
        writer.WriteInt((int)direction);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);
        direction = (Direction)reader.ReadInt();
    }
}

[Flags]
public enum Direction
{
    North = 1,
    East = 2,
    South = 4,
    West = 8
}

