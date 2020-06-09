using System.Collections.Generic;
using Unity.Networking.Transport;

public class RoomInfoMessage : MessageHeader
{
	public override MessageType Type => MessageType.RoomInfo;
    public byte directions;
    public ushort TreasureInRoom;
    public byte ContainsMonster;
    public byte ContainsExit;
    public byte NumberOfOtherPlayers;
    public List<int> OtherPlayerIDs = new List<int>();

	public override void SerializeObject(ref DataStreamWriter writer)
	{
		base.SerializeObject(ref writer);

        writer.WriteByte(directions);
        writer.WriteUShort(TreasureInRoom);
        writer.WriteByte(ContainsMonster);
        writer.WriteByte(ContainsExit);
        writer.WriteByte(NumberOfOtherPlayers);

        for (int i = 0; i < NumberOfOtherPlayers; i++)
            writer.WriteInt(OtherPlayerIDs[i]);
    }
	public override void DeserializeObject(ref DataStreamReader reader)
	{
		base.DeserializeObject(ref reader);

        directions = reader.ReadByte();
        TreasureInRoom = reader.ReadUShort();
        ContainsMonster = reader.ReadByte();
        ContainsExit = reader.ReadByte();
        NumberOfOtherPlayers = reader.ReadByte();
        OtherPlayerIDs.Clear();

        for (int i = 0; i < NumberOfOtherPlayers; i++)
            OtherPlayerIDs.Add(reader.ReadInt());

    }
}
