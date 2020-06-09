using Unity.Networking.Transport;

public class HitByMonsterMessage : MessageHeader
{
	public override MessageType Type => MessageType.HitByMonsters;
	public int PlayerID;
	public ushort NewHP;
	public override void SerializeObject(ref DataStreamWriter writer)
	{
		base.SerializeObject(ref writer);
		writer.WriteInt(PlayerID);
		writer.WriteUShort(NewHP);
	}
	public override void DeserializeObject(ref DataStreamReader reader)
	{
		base.DeserializeObject(ref reader);
		PlayerID = reader.ReadInt();
		NewHP = reader.ReadUShort();
	}
}
