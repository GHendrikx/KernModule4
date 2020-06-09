using Unity.Networking.Transport;

public class HitMonsterMessage : MessageHeader
{
	public override MessageType Type => MessageType.HitMonster;
	public int PlayerID;
	public ushort DamageDeal;
	public override void SerializeObject(ref DataStreamWriter writer)
	{
		base.SerializeObject(ref writer);
		writer.WriteInt(PlayerID);
		writer.WriteUShort(DamageDeal);
	}
	public override void DeserializeObject(ref DataStreamReader reader)
	{
		base.DeserializeObject(ref reader);
		PlayerID = reader.ReadInt();
		DamageDeal = reader.ReadUShort();	
	}
}
