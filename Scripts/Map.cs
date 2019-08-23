using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class Map_Cell
{
    public Vector2Int M_Position = Vector2Int.zero;
    protected object fObject = null;
    internal int fPoolIndex = -1;
    internal bool fIsBusy = false;
    

    public Map_Cell()
    {

    }

    public object Object
    {
        get
        {
            return fObject;
        }

        set
        {
            fObject = value;
        }
    }

    public void Reset()
    {
        fObject = null;
        fIsBusy = false;
    }
}

public class Map : MonoBehaviour
{
    public NotifyEvent_2P<Map, Vector2Int> OnResize = new NotifyEvent_2P<Map, Vector2Int>();

    [SerializeField]
    protected Tilemap fAssociatedTileMap = null;
    [SerializeField]
    protected Transform fBody = null;
    [SerializeField]
    protected Vector2Int fSize = Vector2Int.zero;
    [SerializeField]
    protected Map_Cell[,] fCells = null;
    [SerializeField]
    protected Map_Cell[,] fCellsCache = null;

    [SerializeField]
    protected Map_Cell[] fCellsPool = null;
    [SerializeField]
    protected int fCellsPoolCapacity = 0;
    [SerializeField]
    protected int fOccupyCellsCount = 0;
    [SerializeField]
    protected int fFreeCellsCount = 0;
    
    public Tilemap AssociatedTilemap
    {
        get
        {
            return fAssociatedTileMap;
        }

        set
        {
            if (fAssociatedTileMap == value)
                return;

            if (fAssociatedTileMap != null)
            {
                TileBase tile;

                for (int i = 0; i < fSize.y; i++)
                {
                    for (int j = 0; j < fSize.x; j++)
                    {
                        tile = fAssociatedTileMap.GetTile(new Vector3Int(j, i, 0));
                        ScriptableObject.Destroy(tile);
                    }
                }
            }

            fAssociatedTileMap = value;

            if (fAssociatedTileMap != null)
            {
                for (int i = 0; i < fSize.y; i++)
                {
                    for (int j = 0; j < fSize.x; j++)
                    {
                        fAssociatedTileMap.SetTile(new Vector3Int(j, i, 0), GlobalStorage.Instance.BaseMapTile);
                    }
                }
            }
        }
    }

    public void Clk()
    {
    
        for (int i = 0; i < fSize.y; i++)
        {
            for (int j = 0; j < fSize.x; j++)
            {
                fAssociatedTileMap.SetTile(new Vector3Int(j, i, 0), GlobalStorage.Instance.BaseMapTile);
            }
        }
    }

    public Vector2Int Size
    {
        get
        {
            return fSize;
        }

        set
        {
            Resize(value);
        }
    }

    public int OccupyCellsCount
    {
        get
        {
            return fOccupyCellsCount;
        }
    }

    public int FreeCellsCount
    {
        get
        {
            return fFreeCellsCount;
        }
    }

    public bool OccupyCell(int x, int y, object occupy_object)
    {
        Map_Cell cell = GetCell(x, y);

        if ((cell != null) && 
            (fFreeCellsCount > 0) &&
            (!cell.fIsBusy))
        {
            cell.Object = occupy_object;
            cell.fIsBusy = true;
            int temp_index = fCellsPool[fOccupyCellsCount].fPoolIndex;
            fCellsPool[fOccupyCellsCount].fPoolIndex = cell.fPoolIndex;
            fCellsPool[cell.fPoolIndex] = fCellsPool[fOccupyCellsCount];
            cell.fPoolIndex = temp_index;
            fCellsPool[fOccupyCellsCount] = cell;
            fOccupyCellsCount++;
            fFreeCellsCount--;
            return true;
        }

        return false;
    }

    public bool OccupyCell(Vector2Int m_pos, object occupy_object)
    {
        return OccupyCell(m_pos.x, m_pos.y, occupy_object);
    }

    public bool ReleaseCell(int x, int y)
    {
        Map_Cell cell = GetCell(x, y);

        if ((cell != null) &&
            (fOccupyCellsCount > 0) &&
            (cell.fIsBusy))
        {
            fOccupyCellsCount--;
            fCellsPool[cell.fPoolIndex] = fCellsPool[fOccupyCellsCount];
            fCellsPool[cell.fPoolIndex].fPoolIndex = cell.fPoolIndex;
            cell.fPoolIndex = fOccupyCellsCount;
            cell.Object = null;
            cell.fIsBusy = false;
            fFreeCellsCount++;
            return true;
        }

        return false;
    }

    public bool ReleaseCell(Vector2Int m_pos)
    {
        return ReleaseCell(m_pos.x, m_pos.y);
    }
    
    public void ClearMapCells()
    {
        for (int i=0; i<fSize.y; i++)
        {
            for (int j=0; j<fSize.x; j++)
            {
                fCells[j, i].Reset();
                SetTileTo(j, i, GlobalStorage.Instance.BaseMapTile);
            }
        }

        fOccupyCellsCount = 0;
        fFreeCellsCount = fCellsPoolCapacity;
    }

    public Vector2Int WorldToMapCoord(Vector3 world)
    {
        world -= fBody.position;
        return new Vector2Int((int)(world.x / GlobalStorage.CELL_SIZE), 
                              (int)(world.y / GlobalStorage.CELL_SIZE));
    }

    public Vector3 MapCoordToWorld(int x, int y)
    {
        return fBody.position + new Vector3(x * GlobalStorage.CELL_SIZE, 
                                            y * GlobalStorage.CELL_SIZE);
    }

    public Vector3 MapCoordToWorld(Vector2Int map)
    {
        return MapCoordToWorld(map.x, map.y);
    }

    public Map_Cell GetCell(int x, int y)
    {
        if (!IsValidCoords(x, y))
        {
            Debug.LogError(string.Concat("Cant get map cell at (", x, ",", y,"). Reason: Invalid map coords"));
            return null;
        }
        
        return fCells[x, y];
    }

    public Map_Cell GetCell(Vector2Int m_coord)
    {
        return GetCell(m_coord.x, m_coord.y);
    }

    public Map_Cell GetCell(Vector3 w_coord)
    {
        return GetCell(WorldToMapCoord(w_coord));
    }

    public TileBase GetTileAt(Vector2Int m_coord)
    {
        return fAssociatedTileMap.GetTile(new Vector3Int(m_coord.x, m_coord.y, 0));
    }

    public void SetTileTo(int x, int y, TileBase tile)
    {
        if (tile == null)
            tile = GlobalStorage.Instance.BaseMapTile;

        fAssociatedTileMap.SetTile(new Vector3Int(x, y, 0), tile);
    }

    public void SetTileTo(Vector2Int m_coord, TileBase tile)
    {
        SetTileTo(m_coord.x, m_coord.y, tile);
    }

    public bool IsValidCoords(int x, int y)
    {
        return (x >= 0) && (x < fSize.x) &&
               (y >= 0) && (y < fSize.y);
    }

    public bool IsValidCoords(Vector2Int m_coord)
    {
        return IsValidCoords(m_coord.x, m_coord.y);
    }

    public bool IsValidCoords(Vector3 w_coord)
    {
        return IsValidCoords(WorldToMapCoord(w_coord));
    }

    public Vector2Int GetRandomMapCoord()
    {
        Vector2Int new_mpos = Vector2Int.zero;
        new_mpos.x = Random.Range(0, GlobalStorage.Instance.CurrentMap.Size.x - 1);
        new_mpos.y = Random.Range(0, GlobalStorage.Instance.CurrentMap.Size.y - 1);
        return new_mpos;
    }

    public Vector2Int GetRandomFreeMapCoord()
    {
        int index = Random.Range(fOccupyCellsCount, fCellsPoolCapacity - 1);
        return fCellsPool[index].M_Position;
    }

    public void Resize(Vector2Int new_size)
    {
        if (new_size == fSize)
            return;

        for (int i = 0; i < fSize.y; i++)
        {
            SetTileTo(-1, i, null);
            SetTileTo(fSize.x, i, null);
        }

        for (int j = -1; j < fSize.x + 1; j++)
        {
            SetTileTo(j, -1, null);
            SetTileTo(j, fSize.y, null);
        }

        for (int i=new_size.y; i<fSize.y; i++)
        {
            for (int j=new_size.x; i<fSize.x; j++)
            {
                fCells[j, i] = null;

                if (fAssociatedTileMap != null)
                {
                    fAssociatedTileMap.SetTile(new Vector3Int(j, i, 0), null);
                }
            }
        }

        fCellsCache = new Map_Cell[new_size.x, new_size.y];

        for (int i = new_size.y; i < fSize.y; i++)
        {
            for (int j = new_size.x; i < fSize.x; j++)
            {
                fCellsCache[j, i] = fCells[j, i];
            }
        }

        fCells = null;
        fCells = fCellsCache;
        fCellsCache = null;

        fCellsPoolCapacity = new_size.x * new_size.y;
        fCellsPool = new Map_Cell[fCellsPoolCapacity];
        fFreeCellsCount = 0;
        fOccupyCellsCount = 0;

        for (int i = fSize.y; i < new_size.y; i++)
        {
            for (int j = fSize.x; j < new_size.x; j++)
            {
                fCells[j, i] = new Map_Cell();
                fCells[j, i].M_Position = new Vector2Int(j, i);
                
                if (fAssociatedTileMap != null)
                {
                    fAssociatedTileMap.SetTile(new Vector3Int(j, i, 0), GlobalStorage.Instance.BaseMapTile);
                }
            }
        }

        fSize = new_size;

        for (int i = 0; i < fSize.y; i++)
        {
            for (int j = 0; j < fSize.x; j++)
            {
                fAssociatedTileMap.SetTile(new Vector3Int(j, i, 0), GlobalStorage.Instance.BaseMapTile);
                fCells[j, i].Object = null;
                fCells[j, i].fIsBusy = false;
                fCellsPool[fFreeCellsCount] = fCells[j, i];
                fCellsPool[fFreeCellsCount].fPoolIndex = fFreeCellsCount;
                fFreeCellsCount++;
            }
        }
        
        for (int i = 0; i < fSize.y; i++)
        {
            SetTileTo(-1, i, GlobalStorage.Instance.BorderMapTile);
            SetTileTo(fSize.x, i, GlobalStorage.Instance.BorderMapTile);
        }

        for (int j = -1; j < fSize.x + 1; j++)
        {
            SetTileTo(j, -1, GlobalStorage.Instance.BorderMapTile);
            SetTileTo(j, fSize.y, GlobalStorage.Instance.BorderMapTile);
        }

        OnResize.Invoke(this, fSize);
    }

    public void Resize(int new_width, int new_height)
    {
        Resize(new Vector2Int(new_width, new_height));
    }

    private void Awake()
    {
        fBody = transform;
    }

}
