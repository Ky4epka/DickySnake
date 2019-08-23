using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum SnakeNode_VisualState
{
    None = 0,
    Horz = 1,
    Vert = 2,
    CornerTopLeft = 3,
    CornerTopRight = 4,
    CornerBottomLeft = 5,
    CornerBottomRight = 6,
    HeadL,
    HeadR,
    HeadT,
    HeadB,
    TailL,
    TailR,
    TailT,
    TailB,
}

public enum SnakeNode_NodeType
{
    None,
    Head,
    Tail,
    Body
}

public class SnakeNode: MapObject, IDataPool_Element
{
    [SerializeField]
    public SnakeNode PrevSibling = null;
    [SerializeField]
    public SnakeNode NextSibling = null;

    [SerializeField]
    protected string fName = "";
    [SerializeField]
    protected Snake fOwner = null;
    [SerializeField]
    protected SnakeNode_VisualState fVisualState = SnakeNode_VisualState.None;
    [SerializeField]
    protected DataPool_ElementData fDataPool_ElementData;
    [SerializeField]
    protected Vector2Int fDirection = Vector2Int.zero;
    [SerializeField]
    protected SnakeNode_NodeType fNodeType = SnakeNode_NodeType.None;
    
    public void Initializer()
    {

    }

    public void Finalizer()
    {
        ClearGraphic();
    }

    public void DataPool_Element_SetData(DataPool_ElementData data)
    {
        fDataPool_ElementData = data;
    }

    public DataPool_ElementData DataPool_Element_GetData()
    {
        return fDataPool_ElementData;
    }

    public Vector2Int Direction
    {
        get
        {
            return fDirection;
        }

        set
        {
            fDirection = value;
        }
    }

    public SnakeNode_NodeType NodeType
    {
        get
        {
            return fNodeType;
        }

        set
        {
            fNodeType = value;
        }
    }

    public string Name
    {
        get
        {
            return fName;
        }

        set
        {
            fName = value;
        }
    }

    public Snake Owner
    {
        get
        {
            return fOwner;
        }

        set
        {
            fOwner = value;
        }
    }
            
    public override void ClearGraphic()
    {
        Map m = GlobalStorage.Instance.CurrentMap;
        m.SetTileTo(M_Position, GlobalStorage.Instance.BaseMapTile);
    }

    public override void RefreshGraphic()
    {
        SnakeNode_VisualState vstate = SnakeNode_VisualState.None;

        switch (fNodeType)
        {
            case SnakeNode_NodeType.None:
                break;
            case SnakeNode_NodeType.Head:
                Vector2Int direction = fDirection;

                if (fDirection == Vector2Int.zero)
                {
                    if (PrevSibling == null)
                        fDirection = Vector2Int.right;
                    else
                    {
                        direction = (fM_Position - PrevSibling.M_Position);
                    }
                }

                if ((direction.x == 1) &&
                    (direction.y == 0))
                {
                    vstate = SnakeNode_VisualState.HeadL;
                }
                else if ((direction.x == -1) &&
                         (direction.y == 0))
                {
                    vstate = SnakeNode_VisualState.HeadR;
                }
                else if ((direction.x == 0) &&
                         (direction.y == 1))
                {
                    vstate = SnakeNode_VisualState.HeadT;
                }
                else if ((direction.x == 0) &&
                         (direction.y == -1))
                {
                    vstate = SnakeNode_VisualState.HeadB;
                }

                break;
            case SnakeNode_NodeType.Tail:
                Vector2Int dir = Vector2Int.zero;

                if (NextSibling != null)
                    dir = NextSibling.M_Position - M_Position;

                if ((dir.x == 1) &&
                    (dir.y == 0))
                {
                    vstate = SnakeNode_VisualState.TailL;
                }
                else if ((dir.x == -1) &&
                         (dir.y == 0))
                {
                    vstate = SnakeNode_VisualState.TailR;
                }
                else if ((dir.x == 0) &&
                         (dir.y == 1))
                {
                    vstate = SnakeNode_VisualState.TailT;
                }
                else if ((dir.x == 0) &&
                         (dir.y == -1))
                {
                    vstate = SnakeNode_VisualState.TailB;
                }

                break;
            case SnakeNode_NodeType.Body:
                Vector2Int next_delta = Vector2Int.zero;

                if (NextSibling != null)
                    next_delta = M_Position - NextSibling.M_Position;

                Vector2Int prev_delta = Vector2Int.zero;

                if (PrevSibling != null)
                    prev_delta = M_Position - PrevSibling.M_Position;

                Vector2Int vec = prev_delta + next_delta;

                if ((vec.x == -1) &&
                         (vec.y == 1))
                {
                    vstate = SnakeNode_VisualState.CornerTopLeft;
                }
                else if ((vec.x == 1) &&
                         (vec.y == 1))
                {
                    vstate = SnakeNode_VisualState.CornerTopRight;
                }
                else if ((vec.x == 1) &&
                         (vec.y == -1))
                {
                    vstate = SnakeNode_VisualState.CornerBottomRight;
                }
                else if ((vec.x == -1) &&
                         (vec.y == -1))
                {
                    vstate = SnakeNode_VisualState.CornerBottomLeft;
                }
                else if ((next_delta.x != 0) &&
                         (next_delta.y == 0))
                {
                    vstate = SnakeNode_VisualState.Horz;
                }
                else if ((next_delta.x == 0) &&
                         (next_delta.y != 0))
                {
                    vstate = SnakeNode_VisualState.Vert;
                }
                else
                {
                    vstate = SnakeNode_VisualState.None;
                }

                break;
        }
        
        Map m = GlobalStorage.Instance.CurrentMap;
        m.SetTileTo(M_Position, fOwner.NodeVisualStateTiles[(int)vstate]);
    }
}
