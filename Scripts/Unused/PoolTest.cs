using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoolTest : MonoBehaviour
{
    public GameObjectPool Pool = null;
    public InputField ChangeCapacityField = null;

    public void Add()
    {
        Pool.TakeElement();
    }

    public void RemoveFromBegin()
    {
        Pool.ReturnElement(Pool[0]);
    }

    public void RemoveFromEnd()
    {
        Pool.ReturnElement(Pool[Pool.UsedCount - 1]);
    }

    public void RemoveFromMiddle()
    {
        Pool.ReturnElement(Pool[Pool.UsedCount / 2]);
    }

    public void ChangeCapacity()
    {
        int ivalue = int.Parse(ChangeCapacityField.text);

        Pool.PoolCapacity = ivalue;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
