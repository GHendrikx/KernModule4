using Unity.Networking.Transport;

public class PlayerTurnMessage : MessageHeader
{
    public override MessageType Type => MessageType.PlayerTurn;
    public int playerID;

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);
        writer.WriteInt(playerID);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);
        playerID = reader.ReadInt();
    }
}

