using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class GlobalStorage : MonoBehaviour
{
    public Vector2Int Default_MapSize = new Vector2Int(30, 30);
    public int Default_SnakeLength = 5;

    public Map CurrentMap = null;
    public TileBase BorderMapTile = null;
    public TileBase BaseMapTile = null;
    public TileBase FoodTile = null;

    public string LeaderboardFile = "leaderboard.xml";

    public static float CELL_SIZE = 1f;

    public static GlobalStorage Instance { get; private set; } = null;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

}
