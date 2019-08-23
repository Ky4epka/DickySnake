using UnityEngine;
using System.Collections;

public class MapObject 
{
    protected Vector2Int fM_Position = Vector2Int.zero;

    public Vector2Int M_Position
    {
        get
        {
            return fM_Position;
        }

        set
        {
            Map map = GlobalStorage.Instance.CurrentMap;

            map.ReleaseCell(fM_Position);
            ClearGraphic();

            fM_Position = value;

            if (map.OccupyCell(fM_Position, this)) 
                RefreshGraphic();
        }
    }

    public virtual void ClearGraphic()
    {

    }

    public virtual void RefreshGraphic()
    {

    }
}
