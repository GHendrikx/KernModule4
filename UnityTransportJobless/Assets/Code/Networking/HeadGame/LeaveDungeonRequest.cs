using Unity.Networking.Transport;

public class LeaveDungeonRequest : MessageHeader
{
	public override MessageType Type => MessageType.LeaveDungeonRequest;

	public override void SerializeObject(ref DataStreamWriter writer)
	{
		base.SerializeObject(ref writer);
	}
	public override void DeserializeObject(ref DataStreamReader reader)
	{
		base.DeserializeObject(ref reader);
	}
}
