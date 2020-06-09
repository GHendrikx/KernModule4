using UnityEngine;
using System.Collections;
using Unity.Networking.Transport;
using System.IO;
using Assets.Code;
using Unity.Jobs;
using System.Collections.Generic;
using UnityEngine.Timers;

public class ClientBehaviour : MonoBehaviour
{
    private NetworkDriver networkDriver;
    private NetworkConnection connection;

    private JobHandle networkJobHandle;

    public string ClientName;

    private Queue<MessageHeader> ClientMessagesQueue;

    public MessageEvent[] ClientCallbacks = new MessageEvent[(int)MessageHeader.MessageType.Count - 1];

    Players currentPlayer;

    // Use this for initialization
    void Start()
    {
        networkDriver = NetworkDriver.Create();
        connection = default;

        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 9000;
        connection = networkDriver.Connect(endpoint);
        TimerManager.Instance.AddTimer(StayAlive, 10);
        ClientMessagesQueue = new Queue<MessageHeader>();

        for (int i = 0; i < ClientCallbacks.Length; i++)
        {
            ClientCallbacks[i] = new MessageEvent();
        }

        ClientCallbacks[(int)MessageHeader.MessageType.StartGame].AddListener(GameManager.Instance.StartGameMessage);
        ClientCallbacks[(int)MessageHeader.MessageType.PlayerLeft].AddListener(GameManager.Instance.LeavePlayer);
        ClientCallbacks[(int)MessageHeader.MessageType.RoomInfo].AddListener(GameManager.Instance.GetRoomInfo);



    }

    public void StayAlive()
    {
        networkJobHandle.Complete();
        NetworkingManager.SendMessage(networkDriver, new NoneMessage(), connection);
    }

    // Update is called once per frame
    void Update()
    {
        networkJobHandle.Complete();

        if (!connection.IsCreated)
        {
            return;
        }

        DataStreamReader reader;
        NetworkEvent.Type cmd;
        while ((cmd = connection.PopEvent(networkDriver, out reader)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                Debug.Log("Connected to server");
            }

            else if (cmd == NetworkEvent.Type.Data)
            {
                var messageType = (MessageHeader.MessageType)reader.ReadUShort();
                switch (messageType)
                {
                    case MessageHeader.MessageType.None:
                        var message = NetworkingManager.ReadMessage<NoneMessage>(reader, ClientMessagesQueue) as NoneMessage;
                        TimerManager.Instance.AddTimer(StayAlive, 10);
                        break;

                    case MessageHeader.MessageType.Welcome:
                        Debug.Log("Welcome");
                        WelcomeMessage welcomeMessage = NetworkingManager.ReadMessage<WelcomeMessage>(reader, ClientMessagesQueue) as WelcomeMessage;

                        var setNameMessage = new SetNameMessage
                        {
                            Name = ClientName
                        };

                        currentPlayer = new Players(welcomeMessage.PlayerID, setNameMessage.Name, welcomeMessage.Colour);
                        PlayerManager.Instance.AddPlayer(currentPlayer);
                        NetworkingManager.SendMessage(networkDriver, setNameMessage, connection);
                        break;

                    case MessageHeader.MessageType.NewPlayer:
                        var newPlayerMessage = NetworkingManager.ReadMessage<NewPlayerMessage>(reader, ClientMessagesQueue);
                        PlayerManager.Instance.AddPlayer(new Players((newPlayerMessage as NewPlayerMessage).PlayerID, (newPlayerMessage as NewPlayerMessage).PlayerName, (newPlayerMessage as NewPlayerMessage).PlayerColor));
                        break;

                    case MessageHeader.MessageType.RequestDenied:
                        MessageHeader requestDeniedMessage = NetworkingManager.ReadMessage<RequestDeniedMessage>(reader,ClientMessagesQueue);
                        break;

                    case MessageHeader.MessageType.PlayerLeft:
                        MessageHeader playerLeftMessage = NetworkingManager.ReadMessage<PlayerLeftMessage>(reader, ClientMessagesQueue);
                        break;

                    case MessageHeader.MessageType.StartGame:
                        MessageHeader startGameMessage = NetworkingManager.ReadMessage<StartGameMessage>(reader, ClientMessagesQueue);
                        break;

                    case MessageHeader.MessageType.RoomInfo:
                        MessageHeader roomInfoMessage = NetworkingManager.ReadMessage<RoomInfoMessage>(reader, ClientMessagesQueue) as RoomInfoMessage;
                        break;

                    case MessageHeader.MessageType.PlayerEnterRoom:
                        MessageHeader playerEnterRoom = NetworkingManager.ReadMessage<PlayerEnterRoomMessage>(reader, ClientMessagesQueue) as PlayerEnterRoomMessage;
                        break;

                    case MessageHeader.MessageType.PlayerLeaveRoom:
                        MessageHeader playerLeaveRoom = NetworkingManager.ReadMessage<PlayerLeaveRoomMessage>(reader, ClientMessagesQueue) as PlayerEnterRoomMessage;
                        break;

                    case MessageHeader.MessageType.ObtainTreasure:
                        MessageHeader playerObtainTreasure = NetworkingManager.ReadMessage<ObtainTreasureMessage>(reader, ClientMessagesQueue) as ObtainTreasureMessage;
                        break;

                    case MessageHeader.MessageType.PlayerTurn:
                        MessageHeader playerTurn = NetworkingManager.ReadMessage<PlayerTurnMessage>(reader, ClientMessagesQueue) as PlayerTurnMessage;
                    break;
                    case MessageHeader.MessageType.HitMonster:
                        MessageHeader hitMonster = NetworkingManager.ReadMessage<HitMonsterMessage>(reader, ClientMessagesQueue) as HitMonsterMessage;
                        break;

                    case MessageHeader.MessageType.HitByMonsters:
                        MessageHeader hitByMonster = NetworkingManager.ReadMessage<HitByMonsterMessage>(reader, ClientMessagesQueue) as HitByMonsterMessage;
                        break;

                    case MessageHeader.MessageType.PlayerDefends:
                        MessageHeader playerDefend = NetworkingManager.ReadMessage<PlayerDefendsMessage>(reader, ClientMessagesQueue);

                        break;

                    case MessageHeader.MessageType.PlayerLeftDungeon:
                        MessageHeader playerLeft = NetworkingManager.ReadMessage<PlayerLeftDungeonMessage>(reader, ClientMessagesQueue);
                        break;

                    case MessageHeader.MessageType.PlayerDies:
                        MessageHeader playerDies = NetworkingManager.ReadMessage<PlayerDiesMessage>(reader, ClientMessagesQueue);
                        break;

                    case MessageHeader.MessageType.EndGame:
                        MessageHeader endGame = NetworkingManager.ReadMessage<EndGameMessage>(reader, ClientMessagesQueue);
                        break;
                }

                //Set GUI on or off for the player turn.
                PlayerManager.Instance.ToggleTurn(currentPlayer.Turn);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Disconnected from server");
                connection = default;
            }
        }

        networkJobHandle = networkDriver.ScheduleUpdate();

        ProcessMessagesQueue();
    }

    private void ProcessMessagesQueue()
    {
        while (ClientMessagesQueue.Count > 0)
        {
            var message = ClientMessagesQueue.Dequeue();
            ClientCallbacks[(int)message.Type].Invoke(message);
        }
    }

    public void SendMessage(MessageHeader MessageRequest)
    {
        networkJobHandle.Complete();
        NetworkingManager.SendMessage(networkDriver, MessageRequest, connection);
    }

    private void OnDestroy()
    {
        networkDriver.Dispose();
    }
}
