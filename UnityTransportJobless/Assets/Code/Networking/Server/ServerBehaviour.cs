using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
using System.IO;
using Assets.Code;
using UnityEngine.Events;
using Unity.Jobs;
using UnityEditor;
using System;
using UnityEngine.Timers;

public class ServerBehaviour : MonoBehaviour
{
    private NetworkDriver networkDriver;
    private NativeList<NetworkConnection> connections;

    private JobHandle networkJobHandle;

    private Queue<MessageHeader> serverMessagesQueue;
    public Queue<MessageHeader> ServerMessageQueue
    {
        get
        {
            return serverMessagesQueue;
        }
    }

    public MessageEvent[] ServerCallbacks = new MessageEvent[(int)MessageHeader.MessageType.Count - 1];

    private void Start()
    {
        networkDriver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = 9000;
        if (networkDriver.Bind(endpoint) != 0)
        {
            Debug.Log("Failed to bind port");
        }
        else
        {
            networkDriver.Listen();
        }

        connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        serverMessagesQueue = new Queue<MessageHeader>();

        for (int i = 0; i < ServerCallbacks.Length; i++)
        {
            ServerCallbacks[i] = new MessageEvent();
        }

        ServerCallbacks[(int)MessageHeader.MessageType.SetName].AddListener(PlayerManager.Instance.HandleSetName);
        ServerCallbacks[(int)MessageHeader.MessageType.PlayerLeft].AddListener(GameManager.Instance.LeavePlayer);
        ServerCallbacks[(int)MessageHeader.MessageType.PlayerTurn].AddListener(PlayerManager.Instance.SetTurn);
        //ServerCallbacks[(int)MessageHeader.MessageType.ClaimTreasureRequest].AddListener(PlayerManager.Instance.BroadCastMessageToPlayers);


    }



    public void StartGame()
    {
        networkJobHandle.Complete();
        Debug.Log("Starting Game");
        GameManager.Instance.Grid.InitializeGrid();
        var startGameMessage = new StartGameMessage()
        {
            StartHP = 50
        };

        var playerTurn = new PlayerTurnMessage()
        {
            playerID = 0
        };


        for (int i = 0; i < connections.Length; i++)
        {
            NetworkingManager.SendMessage(networkDriver, startGameMessage, connections[i]);

            RoomInfoMessage roomInfoMessage = MakeRoomInfoMessage(i);
            NetworkingManager.SendMessage(networkDriver, roomInfoMessage, connections[i]);

            NetworkingManager.SendMessage(networkDriver, playerTurn, connections[i]);
        }
    }


    void Update()
    {
        networkJobHandle.Complete();

        for (int i = 0; i < connections.Length; ++i)
        {
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        NetworkConnection c;
        if (connections.Length < 4)
        {
            while ((c = networkDriver.Accept()) != default)
            {
                connections.Add(c);
                Debug.Log("Accepted connection");

                //adding a new connection and give it a welcome message
                var colour = (Color32)UnityEngine.Random.ColorHSV();

                var message = new WelcomeMessage
                {
                    PlayerID = c.InternalId,
                    Colour = ((uint)colour.r << 24) | ((uint)colour.g << 16) | ((uint)colour.b << 8) | colour.a
                };

                PlayerManager.Instance.AddPlayer(new Players(message.PlayerID, "", message.Colour));
                NetworkingManager.SendMessage(networkDriver, message, c);
            }
        }

        DataStreamReader reader;
        for (int i = 0; i < connections.Length; ++i)
        {
            if (!connections[i].IsCreated) continue;

            NetworkEvent.Type cmd;
            while ((cmd = networkDriver.PopEventForConnection(connections[i], out reader)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    var messageType = (MessageHeader.MessageType)reader.ReadUShort();

                    switch (messageType)
                    {
                        #region Stay Alive
                        case MessageHeader.MessageType.None:
                            var noneMessage = NetworkingManager.ReadMessage<NoneMessage>(reader, ServerMessageQueue);
                            NetworkingManager.SendMessage(networkDriver, noneMessage, connections[i]);
                            break;
                        #endregion

                        #region setName
                        case MessageHeader.MessageType.SetName:

                            SetNameMessage setNameMessage = NetworkingManager.ReadMessage<SetNameMessage>(reader, ServerMessageQueue) as SetNameMessage;
                            PlayerManager.Instance.SetName(setNameMessage.Name, i);

                            var newPlayerMessage = new NewPlayerMessage()
                            {
                                PlayerID = PlayerManager.Instance.players[i].playerID,
                                PlayerColor = PlayerManager.Instance.players[i].clientColor,
                                PlayerName = setNameMessage.Name
                            };


                            for (int j = 0; j < connections.Length; j++)
                                if (connections[j].InternalId != newPlayerMessage.PlayerID)
                                {
                                    NetworkingManager.SendMessage(networkDriver, newPlayerMessage, connections[j]);

                                    var currentPlayerMessage = new NewPlayerMessage()
                                    {
                                        PlayerID = PlayerManager.Instance.players[j].playerID,
                                        PlayerColor = PlayerManager.Instance.players[j].clientColor,
                                        PlayerName = PlayerManager.Instance.players[j].clientName
                                    };

                                    NetworkingManager.SendMessage(networkDriver, currentPlayerMessage, connections[i]);
                                }
                            break;
                        #endregion

                        #region NewPlayer
                        case MessageHeader.MessageType.NewPlayer:
                            var newPlayer = NetworkingManager.ReadMessage<NewPlayerMessage>(reader, ServerMessageQueue);
                            Players player = new Players((newPlayer as NewPlayerMessage).PlayerID, (newPlayer as NewPlayerMessage).PlayerName, (newPlayer as NewPlayerMessage).PlayerColor);
                            PlayerManager.Instance.AddPlayer(player);
                            break;
                        #endregion

                        case MessageHeader.MessageType.RequestDenied:
                            break;

                        case MessageHeader.MessageType.PlayerLeft:
                            var playerLeft = NetworkingManager.ReadMessage<PlayerLeftMessage>(reader, ServerMessageQueue);
                            break;

                        case MessageHeader.MessageType.PlayerTurn:
                            MessageHeader messageHeader = NetworkingManager.ReadMessage<PlayerTurnMessage>(reader, serverMessagesQueue);
                            break;


                        #region RoomInfoMessage
                        case MessageHeader.MessageType.RoomInfo:
                            RoomInfoMessage roomInfoMessage = MakeRoomInfoMessage(i);
                            NetworkingManager.SendMessage(networkDriver, roomInfoMessage, connections[i]);
                            break;
                        #endregion

                        case MessageHeader.MessageType.HitByMonsters:

                            break;
                        case MessageHeader.MessageType.PlayerDefends:
                            break;
                        case MessageHeader.MessageType.PlayerLeftDungeon:
                            break;
                        case MessageHeader.MessageType.PlayerDies:
                            break;
                        case MessageHeader.MessageType.EndGame:
                            break;

                        #region moveRequest
                        case MessageHeader.MessageType.MoveRequest:

                            var moveRequest = NetworkingManager.ReadMessage<MoveMessage>(reader, serverMessagesQueue) as MoveMessage;
                            byte neighbors = GameManager.Instance.Grid.CheckNeighbors(i);
                            UnityEngine.Vector2 currentGridPosition = PlayerManager.Instance.players[i].TilePosition;

                            if (GameManager.Instance.Grid.tilesArray[(int)currentGridPosition.x, (int)currentGridPosition.y].Content == TileContent.Monster || GameManager.Instance.Grid.tilesArray[(int)currentGridPosition.x, (int)currentGridPosition.y].Content == TileContent.Both)
                                Debug.Log("Cant escape");

                            for (int j = 0; j < PlayerManager.Instance.players.Count; j++)
                            {
                                if (PlayerManager.Instance.players[i].TilePosition != PlayerManager.Instance.players[j].TilePosition)
                                    continue;

                                PlayerLeaveRoomMessage leaveRoom = new PlayerLeaveRoomMessage()
                                {
                                    PlayerID = i
                                };

                                NetworkingManager.SendMessage(networkDriver, leaveRoom, connections[j]);
                            }


                            if (((byte)moveRequest.direction & neighbors) == (byte)moveRequest.direction)
                            {
                                switch (moveRequest.direction)
                                {
                                    case Direction.North:
                                        PlayerManager.Instance.MovePlayer(PlayerManager.Instance.players[i], new Vector2(0, 1));
                                        break;
                                    case Direction.East:
                                        PlayerManager.Instance.MovePlayer(PlayerManager.Instance.players[i], new Vector2(1, 0));
                                        break;
                                    case Direction.South:
                                        PlayerManager.Instance.MovePlayer(PlayerManager.Instance.players[i], new Vector2(0, -1));
                                        break;
                                    case Direction.West:
                                        PlayerManager.Instance.MovePlayer(PlayerManager.Instance.players[i], new Vector2(-1, 0));
                                        break;
                                }
                            }


                            for (int j = 0; j < PlayerManager.Instance.players.Count; j++)
                            {
                                if (PlayerManager.Instance.players[i].TilePosition != PlayerManager.Instance.players[j].TilePosition)
                                    continue;
                                if (PlayerManager.Instance.players[i] == PlayerManager.Instance.players[j])
                                    continue;

                                PlayerEnterRoomMessage enterRoom = new PlayerEnterRoomMessage()
                                {
                                    playerID = i
                                };

                                NetworkingManager.SendMessage(networkDriver, enterRoom, connections[j]);
                            }
                            //Make a new Room Message info for all the connections
                            for (int j = 0; j < connections.Length; j++)
                            {
                                RoomInfoMessage newRoomInfoMessage = MakeRoomInfoMessage(j);
                                NetworkingManager.SendMessage(networkDriver, newRoomInfoMessage, connections[j]);
                            }
                            break;
                        #endregion

                        case MessageHeader.MessageType.AttackRequest:
                            var attackRequest = NetworkingManager.ReadMessage<AttackRequestMessage>(reader, serverMessagesQueue);
                            AttackMonster(i);
                            break;

                        case MessageHeader.MessageType.DefendRequest:
                            var defendRequest = NetworkingManager.ReadMessage<DefendRequestMessage>(reader, serverMessagesQueue);
                            PlayerManager.Instance.DefendRequest(i);
                            break;

                        #region claimTreasure
                        case MessageHeader.MessageType.ClaimTreasureRequest:

                            var claimTreasure = NetworkingManager.ReadMessage<ClaimTreasureRequestMessage>(reader, serverMessagesQueue);
                            List<Players> claimTreasurePlayers = new List<Players>();
                            int counter = 1;

                            for (int j = 0; j < PlayerManager.Instance.players.Count; j++)
                            {
                                if (PlayerManager.Instance.players[i].TilePosition != PlayerManager.Instance.players[j].TilePosition)
                                    continue;
                                if (PlayerManager.Instance.players[i] == PlayerManager.Instance.players[j])
                                    continue;

                                counter++;
                                claimTreasurePlayers.Add(PlayerManager.Instance.players[j]);
                            }
                            Vector2 position = PlayerManager.Instance.players[i].TilePosition;
                            if (GameManager.Instance.Grid.tilesArray[(int)position.x, (int)position.y].Content != TileContent.Both)
                                Debug.Log("Monster still present");
                            if (GameManager.Instance.Grid.tilesArray[(int)position.x, (int)position.y].Content == TileContent.Treasure)
                            {
                                int score = GameManager.Instance.Grid.tilesArray[(int)position.x, (int)position.y].RandomTreasureAmount;
                                score /= claimTreasurePlayers.Count;
                                for (int j = 0; j < claimTreasurePlayers.Count; j++)
                                {
                                    var obtainTreasure = new ObtainTreasureMessage()
                                    {
                                        Amount = (ushort)score
                                    };
                                }
                            }
                            break;
                        #endregion

                        case MessageHeader.MessageType.LeaveDungeonRequest:
                            var leaveRequest = NetworkingManager.ReadMessage<LeaveDungeonRequest>(reader, serverMessagesQueue);
                            break;

                    }

                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Client disconnected");


                    var message = new PlayerLeftMessage()
                    {
                        playerLeftID = (uint)i
                    };

                    PlayerManager.Instance.RemovePlayer(PlayerManager.Instance.players[(int)message.playerLeftID]);

                    for (int j = 0; j < connections.Length; j++)
                    {
                        if (j != i)
                        {
                            NetworkingManager.SendMessage(networkDriver, message, connections[i]);
                        }
                    }

                    connections[i] = default;
                }
            }
        }

        networkJobHandle = networkDriver.ScheduleUpdate();

        ProcessMessagesQueue();
    }

    private void AttackMonster(int i)
    {
        Players currentPlayer = PlayerManager.Instance.players[i];
        if (GameManager.Instance.Grid.TileContain(currentPlayer.TilePosition) == TileContent.Monster || GameManager.Instance.Grid.TileContain(currentPlayer.TilePosition) == TileContent.Both)
        {
            HitMonsterMessage hitMonster = new HitMonsterMessage()
            {
                PlayerID = i,
                DamageDeal = (ushort)UnityEngine.Random.Range(1, 3)
            };

            NetworkingManager.SendMessage(networkDriver, hitMonster, connections[i]);
            GameManager.Instance.Grid.HitMonster(PlayerManager.Instance.players[i].TilePosition, hitMonster.DamageDeal);
        }
        Debug.Log(GameManager.Instance.Grid.tilesArray[(int)currentPlayer.TilePosition.x, (int)currentPlayer.TilePosition.y].MonsterHealth.ToString());
    }

    /// <summary>
    /// Packing info message for the player in this room.
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    private RoomInfoMessage MakeRoomInfoMessage(int i)
    {
        networkJobHandle.Complete();
        byte neighbors = GameManager.Instance.Grid.CheckNeighbors(i);

        TileContent tileContent = GameManager.Instance.Grid.TileContain(PlayerManager.Instance.players[i].TilePosition);

        ushort treasure = 0;
        byte monster = 0;
        byte exit = 0;

        if (tileContent == TileContent.Treasure || tileContent == TileContent.Both)
            treasure = 1;
        if (tileContent == TileContent.Monster || tileContent == TileContent.Both)
            monster = 1;
        if (tileContent == TileContent.Exit)
            exit = 1;

        List<int> playersID = new List<int>();

        for (int j = 0; j < PlayerManager.Instance.players.Count; j++)
            if (PlayerManager.Instance.players[j].TilePosition == PlayerManager.Instance.players[i].TilePosition)
            {
                if (PlayerManager.Instance.players[j].playerID == PlayerManager.Instance.players[i].playerID)
                    continue;
                else
                    playersID.Add(PlayerManager.Instance.players[i].playerID);
            }

        var roomInfo = new RoomInfoMessage()
        {
            directions = neighbors,
            TreasureInRoom = treasure,
            ContainsMonster = monster,
            ContainsExit = exit,
            NumberOfOtherPlayers = (byte)playersID.Count,
            OtherPlayerIDs = playersID
        };

        return roomInfo;
    }

    private void NextTurn(int i)
    {
        List<Players> players = PlayerManager.Instance.players;
        for (int j = 0; j < players.Count; j++)
        {

        }
    }

    private void ProcessMessagesQueue()
    {
        while (serverMessagesQueue.Count > 0)
        {
            var message = serverMessagesQueue.Dequeue();
            ServerCallbacks[(int)message.Type].Invoke(message);
        }
    }

    public void PlayerLeft(MessageHeader message)
    {
        PlayerLeftMessage playerLeft = (message as PlayerLeftMessage);
        PlayerManager.Instance.RemovePlayer(PlayerManager.Instance.players[(int)playerLeft.playerLeftID]);

        for (int j = 0; j < connections.Length; j++)
        {
            if (j != playerLeft.playerLeftID)
            {
                NetworkingManager.SendMessage(networkDriver, message, connections[j]);
            }
        }
    }

    private void OnDestroy()
    {
        networkDriver.Dispose();
        connections.Dispose();
    }
}
