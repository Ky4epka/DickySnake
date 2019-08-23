using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LinkedList2<T> : LinkedList<T>
{

    public void MoveBefore(LinkedListNode<T> before, LinkedListNode<T> node)
    {
        if (node == before)
            return;

        T value = node.Value;
        Remove(node);
        AddBefore(before, value);
    }

    public void MoveBefore(LinkedListNode<T> before, T value)
    {
        MoveBefore(before, Find(value));
    }

    public void MoveAfter(LinkedListNode<T> after, LinkedListNode<T> node)
    {
        if (after == node)
            return;

        T value = node.Value;
        Remove(node);
        AddAfter(after, value);
    }

    public void MoveAfter(LinkedListNode<T> after, T value)
    {
        MoveAfter(after, Find(value));
    }

    public void BringToFront(LinkedListNode<T> node)
    {
        MoveBefore(First, node);
    }

    public void BringToFront(T value)
    {
        MoveBefore(First, value);
    }

    public void BringToBack(LinkedListNode<T> node)
    {
        MoveAfter(Last, node);
    }

    public void BringToBack(T value)
    {
        MoveAfter(Last, value);
    }
}
