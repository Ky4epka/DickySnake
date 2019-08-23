using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class FoodBase: MapObject
{
    public TileBase Tile = null;

    public override void ClearGraphic()
    {
        base.ClearGraphic();
        Map map = GlobalStorage.Instance.CurrentMap;
        map.SetTileTo(M_Position, null);
    }

    public override void RefreshGraphic()
    {
        base.RefreshGraphic();
        Map map = GlobalStorage.Instance.CurrentMap;
        map.SetTileTo(M_Position, Tile);
    }

    public virtual void ReplaceOnMap()
    {
        M_Position = GlobalStorage.Instance.CurrentMap.GetRandomFreeMapCoord();
    }   

}
