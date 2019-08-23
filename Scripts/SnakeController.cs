using UnityEngine;
using System.Collections;

public class SnakeController : MonoBehaviour, KeyboardEvents.IKeyHandler
{
    public Snake Snake = null;
    
    public bool OnKeyDown(KeyCode key)
    {
        return ProcessKey(key);
    }

    public bool OnKeyUp(KeyCode key)
    {
        return true;
    }

    public bool OnKeyRepeat(KeyCode key)
    {
        return true;
    }

    public void OnSnakeStep(Snake sender)
    {
    }
    
    protected bool ProcessKey(KeyCode key)
    {
        Vector2Int direction = Vector2Int.zero;
        bool moving = false;

        if (key == KeyCode.UpArrow)
        {
            Snake.SpeedVector = Vector2Int.up;
            Snake.StartMove();
        }

        if (key == KeyCode.DownArrow)
        {
            Snake.SpeedVector = Vector2Int.down;
            Snake.StartMove();
        }

        if (key == KeyCode.LeftArrow)
        {
            Snake.SpeedVector = Vector2Int.left;
            Snake.StartMove();
        }

        if (key == KeyCode.RightArrow)
        {
            Snake.SpeedVector = Vector2Int.right;
            Snake.StartMove();
        }
        
        return !moving;
    }

    private void Start()
    {
        Snake.OnStep.AddListener(OnSnakeStep);
    }


    private void OnDestroy()
    {
        Snake.OnStep.RemoveListener(OnSnakeStep);
    }

}
