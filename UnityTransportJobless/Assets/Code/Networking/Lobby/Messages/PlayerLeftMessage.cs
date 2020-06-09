using Unity.Networking.Transport;

public class PlayerLeftMessage : MessageHeader
{
    public override MessageType Type => MessageType.PlayerLeft;

    public uint playerLeftID;

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);

        writer.WriteUInt(playerLeftID);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);

        playerLeftID = reader.ReadUInt();
    }
}
