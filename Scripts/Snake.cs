using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class Snake_NodePool: DataPool
{
    protected int fId = 0;

    public override IDataPool_Element ElementConstructor()
    {
        SnakeNode node = new SnakeNode();
        node.Initializer();
        node.Name = "element" + fId;
        fId++;
        return node;
    }

    public override void ElementDestructor(IDataPool_Element element)
    {
        SnakeNode node = element as SnakeNode;

        node.Finalizer();
    }
}

public class Snake : CachedMonoBehaviour
{
    public TileBase[] NodeVisualStateTiles = null;

    public NotifyEvent<Snake> OnStep = new NotifyEvent<Snake>();
    public NotifyEvent<Snake> OnMapOutOfBounds = new NotifyEvent<Snake>();
    public NotifyEvent<Snake> OnCollideWithSelf = new NotifyEvent<Snake>();
    public NotifyEvent_2P<Snake, object> OnCollision = new NotifyEvent_2P<Snake, object>();
    
    [SerializeField]
    protected Vector2Int fHeadPosition = Vector2Int.zero;
    [SerializeField]
    protected int fStepsPerSecond = 1;
    [SerializeField]
    protected float fSPSCache_Tick = 0f;
    [SerializeField]
    protected float fSPS_Time = 0f;
    [SerializeField]
    protected bool fChangeSpeedVector = false;
    [SerializeField]
    protected Vector2Int fChangingSpeedVector = Vector2Int.zero;
    [SerializeField]
    protected Vector2Int fSpeedVector = Vector2Int.zero;
    [SerializeField]
    protected bool fMoving = false;
    [SerializeField]
    protected SnakeNode fFirstNode = null;
    [SerializeField]
    protected SnakeNode fLastNode = null;
    [SerializeField]
    protected int fLength = 0;
    [SerializeField]
    protected Snake_NodePool fNodePool = new Snake_NodePool();

    protected Transform fBody = null;

    public Vector2Int M_HeadPosition
    {
        get
        {
            return fHeadPosition;
        }

        set
        {
            fHeadPosition = value;
            FullMoveTo(fHeadPosition);
        }
    }

    public void StartMove()
    {
        fMoving = true;
    }

    public void StopMove()
    {
        fMoving = false;
    }

    public int StepsPerSecond
    {
        get
        {
            return fStepsPerSecond;
        }

        set
        {
            fStepsPerSecond = value;

            fSPSCache_Tick = 1f / fStepsPerSecond;
        }
    }

    public int Length
    {
        get
        {
            return fLength;
        }

        set
        {
            int delta = value - fLength;
            
            while (fLength != value)
            {
                if (delta < 0)
                    RemoveNode(fLastNode);
                else
                    AddNode();
            }
        }
    }

    public Vector2Int SpeedVector
    {
        get
        {
            return fSpeedVector;
        }

        set
        {
            fChangeSpeedVector = fMoving;

            if (fMoving)
            {
                fChangingSpeedVector = value;
            }
            else if (IsValidSpeedVector(value))
            {
                fSpeedVector = value;
            }
        }
    }

    public void ResetToDefault()
    {
        StopMove();
        fSpeedVector = Vector2Int.zero;
    }

    public void AddNodeToEnd()
    {
        AddNode();
    }

    public Snake_NodePool NodePool
    {
        get
        {
            return fNodePool;
        }
    }

    public void InvalidateGraphic()
    {

    }

    protected void AddNode()
    {
        SnakeNode new_node = fNodePool.TakeElement() as SnakeNode;
        new_node.Owner = this;
        AttachNodeBefore(null, new_node);
    }

    protected void RemoveNode(SnakeNode node)
    {
        DettachNode(node);
        fNodePool.ReturnElement(node);
    }

    protected void AttachNodeAfter(SnakeNode after, SnakeNode node)
    {
        node.PrevSibling = after;
        node.NextSibling = null;

        if (fFirstNode == null)
        {
            fFirstNode = node;
            fLastNode = node;
        }
        else if ((after == fLastNode) ||
                 (after == null))
        {
            fLastNode.NextSibling = node;
            node.PrevSibling = fLastNode;
            node.NextSibling = null;
            fLastNode = node;
        }
        else
        {
            node.PrevSibling = after;
            node.NextSibling = after.NextSibling;
            after.NextSibling.PrevSibling = node;
            after.NextSibling = node;
        }

        fLength++;
    }

    protected void AttachNodeBefore(SnakeNode before, SnakeNode node)
    {
        node.PrevSibling = null;
        node.NextSibling = before;

        if (fFirstNode == null)
        {
            fFirstNode = node;
            fLastNode = node;
        }
        else if ((before == fFirstNode) ||
                 (before == null))
        {
            fFirstNode.PrevSibling = node;
            node.PrevSibling = null;
            node.NextSibling = fFirstNode;
            fFirstNode = node;
        }
        else
        {
            node.PrevSibling = before.PrevSibling;
            node.NextSibling = before;
            before.PrevSibling.NextSibling = node;
            before.PrevSibling = node;
        }

        fLength++;
    }

    void MoveNodeAfter(SnakeNode after, SnakeNode node)
    {
        if (fFirstNode == fLastNode)
            return;

        DettachNode(node);
        AttachNodeAfter(after, node);
    }

    void MoveNodeBefore(SnakeNode before, SnakeNode node)
    {
        if (fFirstNode == fLastNode) 
            return;

        DettachNode(node);
        AttachNodeBefore(before, node);
    }

    protected void DettachNode(SnakeNode node)
    {
        if (fFirstNode == null)
        {
            return;
        }
        else if ((fFirstNode == fLastNode) && 
                 (fFirstNode == node))
        {
            fFirstNode = null;
            fLastNode = null;
        }
        else if (fFirstNode == node)
        {
            fFirstNode = fFirstNode.NextSibling;
            fFirstNode.PrevSibling = null;
        }
        else if (fLastNode == node)
        {
            fLastNode = fLastNode.PrevSibling;
            fLastNode.NextSibling = null;
        }
        else
        {
            node.PrevSibling.NextSibling = node.NextSibling;
            node.NextSibling.PrevSibling = node.PrevSibling;
        }

        node.PrevSibling = null;
        node.NextSibling = null;
        fLength--;
    }
       
    protected void Clear()
    {
        SnakeNode node = fFirstNode;
        SnakeNode cache = null;

        while (node != null)
        {
            cache = node;
            node = node.NextSibling;
            RemoveNode(cache);
        }

        fLength = 0;
    }

    protected void FullMoveTo(Vector2Int m_position)
    {
        SnakeNode node = fLastNode;
        
        while (node != null)
        {
            node.NodeType = SnakeNode_NodeType.Body;
            node.Direction = Vector2Int.zero;
            node.M_Position = m_position;
            m_position += Vector2Int.down;
            node = node.PrevSibling;
        }

        if (fFirstNode != null)
        {
            fFirstNode.NodeType = SnakeNode_NodeType.Tail;
            fLastNode.NodeType = SnakeNode_NodeType.Head;

            if (fLastNode.PrevSibling != null)
            {
                fSpeedVector = fLastNode.M_Position - fLastNode.PrevSibling.M_Position;
            }
            else
                fSpeedVector = Vector2Int.zero;
        }

        node = fLastNode;

        while (node != null)
        {
            node.RefreshGraphic();
            node = node.PrevSibling;
        }
    }

    protected bool CheckCollision(Vector2Int m_pos)
    {
        Map map = GlobalStorage.Instance.CurrentMap;
        bool map_out_of_bounds = !map.IsValidCoords(m_pos);

        if (map_out_of_bounds)
        {
            OnMapOutOfBounds.Invoke(this);
            return false;
        }
        else
        {
            Map_Cell cell = map.GetCell(m_pos);
            object col_object = cell.Object;

            if (col_object != null)
            {
                if ((col_object is SnakeNode) &&
                    (col_object as SnakeNode).Owner == this &&
                    (M_HeadPosition != m_pos))
                {
                    OnCollideWithSelf.Invoke(this);
                    return false;
                }
                else
                    OnCollision.Invoke(this, col_object);
            }
        }

        return true;
    }

    protected void DoStep(Vector2Int direction)
    {
        Vector2Int head_pos = fHeadPosition + direction;

        if (CheckCollision(head_pos))
        {
            SnakeNode cur_node = fFirstNode;
            SnakeNode prev_node = fLastNode;
            MoveNodeAfter(fLastNode, fFirstNode);
            fHeadPosition = head_pos;
            cur_node.NodeType = SnakeNode_NodeType.Head;
            cur_node.Direction = direction;
            cur_node.M_Position = fHeadPosition;
            prev_node.NodeType = SnakeNode_NodeType.Body;
            prev_node.RefreshGraphic();
            fFirstNode.NodeType = SnakeNode_NodeType.Tail;
            fFirstNode.RefreshGraphic();
            OnStep.Invoke(this);
        }
    }
    
    protected bool IsValidSpeedVector(Vector2Int direction)
    {
        return (direction != fSpeedVector * -1);
    }

    private void Awake()
    {
        fBody = transform;
        StepsPerSecond = 1;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    // Update is called once per frame
    void Update()
    {
        if (!fMoving)
            return;

        fSPS_Time += Time.deltaTime;

        if (fSPS_Time >= fSPSCache_Tick)
        {
            fSPS_Time = 0f;

            if (fChangeSpeedVector &&
                IsValidSpeedVector(fChangingSpeedVector))
            {
                fChangeSpeedVector = false;
                fSpeedVector = fChangingSpeedVector;
            }

            DoStep(fSpeedVector);
        }
    }
}
