using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    Vector2 gridSize = new Vector2(10, 10);
    public Tile[,] tilesArray = new Tile[10, 10];
    public List<Tile> tiles = new List<Tile>();

    public void InitializeGrid()
    {

        Debug.Log("INITIALIZING");
        for (int x = 0; x < gridSize.x; x++)
            for (int y = 0; y < gridSize.y; y++)
            {
                Tile temp = new Tile(x, y); 
                tilesArray[x, y] = temp;
                tiles.Add(temp);
            }
        tilesArray[0, 0].SetBeginOrExitTile(TileContent.Begin);
        tilesArray[9, 9].SetBeginOrExitTile(TileContent.Exit);

        for (int x = 0; x < tilesArray.GetLength(0); x++)
        {
            for (int y = 0; y < tilesArray.GetLength(1); y++)
            {
                if (tilesArray[x, y].Content == TileContent.Begin || tilesArray[x, y].Content == TileContent.Exit)
                    continue;

                if (tilesArray[x, y].Content == TileContent.Treasure || tilesArray[x, y].Content == TileContent.Both)
                    tilesArray[x, y].RandomTreasureAmount = (ushort)Random.Range(10, 101);
                if (tilesArray[x, y].Content == TileContent.Monster || tilesArray[x, y].Content == TileContent.Both)
                    tilesArray[x, y].MonsterHealth = Random.Range(1, 6);
            }
        }
        Debug.Log(tilesArray[0,1].Content);
    }

    /// <summary>
    /// Check the neighbors and returns the byte. 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public byte CheckNeighbors(int i)
    {
        Players currentPlayer = PlayerManager.Instance.players[i];

        byte north = 0;
        byte east = 0;
        byte south = 0;
        byte west = 0;

        Vector2 currentPosition = currentPlayer.TilePosition;
        float x = currentPosition.x;
        float y = currentPosition.y;

        Tile northTile = null;
        Tile southTile = null;
        Tile eastTile = null;
        Tile westTile = null;

        if (InGrid((int)currentPosition.x,(int)currentPosition.y + 1))
            northTile = tilesArray[(int)currentPosition.x, (int)currentPosition.y + 1];
        if (InGrid((int)currentPosition.x, (int)currentPosition.y - 1))
            southTile = tilesArray[(int)currentPosition.x, (int)currentPosition.y - 1];
        if (InGrid((int)currentPosition.x + 1, (int)currentPosition.y))
            eastTile = tilesArray[(int)currentPosition.x + 1, (int)currentPosition.y];
        if (InGrid((int)currentPosition.x - 1, (int)currentPosition.y))
            westTile = tilesArray[(int)currentPosition.x - 1, (int)currentPosition.y];

        if (northTile != null)
            north |= (byte)Direction.North;
        if (southTile != null)
            south = (byte)Direction.South;
        if (westTile != null)
            west = (byte)Direction.West;
        if (eastTile != null)
            east = (byte)Direction.East;

        byte answer = (byte)(north | east | south | west);
        return answer;
    }

    /// <summary>
    /// Inside the grid
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool InGrid(int x, int y)
    {
        if (x < tilesArray.GetLength(0) && x >= 0 && y < tilesArray.GetLength(1) && y >= 0)
            return true;
        return false;
    }

    /// <summary>
    /// Returns the content of the tile.
    /// </summary>
    /// <param name="currentPosition"></param>
    /// <returns></returns>
    public TileContent TileContain(Vector2 currentPosition)
    {
        return tilesArray[(int)currentPosition.x, (int)currentPosition.y].Content;
    }

    public void HitMonster(Vector2 position ,int hitpoints)
    {
        Tile currentTile = tilesArray[(int)position.x, (int)position.y];

        if (currentTile.Content == TileContent.Both || currentTile.Content == TileContent.Monster)
            currentTile.MonsterHealth -= hitpoints;
        else
            return;

        if(currentTile.MonsterHealth <= 0)
        {
            if (currentTile.Content == TileContent.Both)
                currentTile.Content = TileContent.Treasure;
            else
                currentTile.Content = TileContent.None;
        }
    }
}
[System.Serializable]
public class Tile
{
    public int X;
    public int Y;
    public TileContent Content;
    public ushort RandomTreasureAmount;
    public int MonsterHealth;
    public bool ExitTile;
    public bool BeginTile;

    public Tile(int x, int y)
    {
        this.X = x;
        this.Y = y;
        RandomTileContent();
    }

    public void SetBeginOrExitTile(TileContent content)
    {
        if (content == TileContent.Exit)
        {
            ExitTile = true;
            this.Content = TileContent.Exit;
        }

        else if (content == TileContent.Begin)
        {
            BeginTile = true;
            this.Content = TileContent.Begin;
        }
    }

    public void RandomTileContent()
    {
        int i = Random.Range(2, 4 + 1);
        Content = (TileContent)i;
    }
}

public enum TileContent
{
    Begin,
    Exit,
    Monster,
    Treasure,
    Both,
    None
}

