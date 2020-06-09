using Unity.Networking.Transport;

public class PlayerLeaveRoomMessage : MessageHeader
{
	public override MessageType Type => MessageType.PlayerLeaveRoom;
    public int PlayerID;
	public override void SerializeObject(ref DataStreamWriter writer)
	{
		base.SerializeObject(ref writer);
        writer.WriteInt(PlayerID);
	}
	public override void DeserializeObject(ref DataStreamReader reader)
	{
		base.DeserializeObject(ref reader);
        PlayerID = reader.ReadInt();
	}
}
