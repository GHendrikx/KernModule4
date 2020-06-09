using Unity.Networking.Transport;

public class ObtainTreasureMessage : MessageHeader
{
	public override MessageType Type => MessageType.ObtainTreasure;
    public ushort Amount;

	public override void SerializeObject(ref DataStreamWriter writer)
	{
		base.SerializeObject(ref writer);
        writer.WriteUShort(Amount);
	}
	public override void DeserializeObject(ref DataStreamReader reader)
	{
		base.DeserializeObject(ref reader);
        Amount = reader.ReadUShort();
	}
}
