using Unity.Networking.Transport;

public class DefendRequestMessage : MessageHeader
{
	public override MessageType Type => MessageType.DefendRequest;
	public override void SerializeObject(ref DataStreamWriter writer)
	{
		base.SerializeObject(ref writer);
	}
	public override void DeserializeObject(ref DataStreamReader reader)
	{
		base.DeserializeObject(ref reader);
	}
}
