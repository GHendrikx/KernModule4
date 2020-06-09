using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    public UIManager UIManager;
    public List<Players> players = new List<Players>();
    public int playerTurn;
    [SerializeField]
    public GameObject TurnGameObject;

    public void AddPlayer(Players newPlayer)
    {
        players.Add(newPlayer);
        UpdateUI();
    }

    public void UpdateUI()
    {
        UIManager.UpdateUI();
    }

    public void RemovePlayer(Players player)
    {
        players.Remove(player);

        UpdateUI();
    }

    public void HandleSetName(MessageHeader message)
    {
        Debug.Log($"Got a name: {(message as SetNameMessage).Name} ");
        UpdateUI();
    }

    public void SetName(string name, int i)
    {
        players[i].clientName = name;
    }

    public void MovePlayer(Players player, Vector2 dir)
    {
        player.TilePosition += dir;
    }

    public void SetTurn(MessageHeader player)
    {
        for (int i = 0; i < players.Count; i++)
        {
            players[i].Turn = false;
        }

        players[(player as PlayerTurnMessage).playerID].Turn = true;
    }

    public void ToggleTurn(bool active)
    {
        TurnGameObject.SetActive(active);
    }

    public void DefendRequest(int i)
    {
        Players Player = players[i];
        Player.DefendOneTurn = true;
    }

}
[System.Serializable]
public class Players
{
    public int playerID;
    public string clientName;
    public uint clientColor;
    public Vector2 TilePosition;
    public int treasureAmount;
    public bool Turn;
    public bool DefendOneTurn;

    public Players(int playerID, string clientName, uint clientColor)
    {
        this.playerID = playerID;
        this.clientName = clientName;
        this.clientColor = clientColor;
        this.TilePosition = new Vector2(0, 0);
    }
}
