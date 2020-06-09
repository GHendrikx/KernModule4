
using Unity.Networking.Transport;

public class NewPlayerMessage : MessageHeader
{
    public override MessageType Type => MessageType.NewPlayer;
    public int PlayerID;
    public uint PlayerColor;
    public string PlayerName;

    public override void SerializeObject(ref DataStreamWriter writer)
    {
        base.SerializeObject(ref writer);

        writer.WriteInt(PlayerID);
        writer.WriteUInt(PlayerColor);
        writer.WriteString(PlayerName);
    }

    public override void DeserializeObject(ref DataStreamReader reader)
    {
        base.DeserializeObject(ref reader);

        PlayerID = reader.ReadInt();
        PlayerColor = reader.ReadUInt();
        PlayerName = reader.ReadString().ToString();
    }
}

