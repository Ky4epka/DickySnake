using UnityEngine;
using System.Collections;

public struct DataPool_ElementData
{
    internal IDataPool fOwner;
    internal int fPoolIndex;
}

public interface IDataPool_Element
{
    void DataPool_Element_SetData(DataPool_ElementData data);
    DataPool_ElementData DataPool_Element_GetData();
}


public interface IDataPool
{
    void SetCapacity(int value);
    int GetCapacity();

    int GetUsedCount();
    int GetUnusedCount();

    void SetUseAutogrow(bool value);
    bool GetUseAutogrow();

    void SetGrowQuota(int value);
    int GetGrowQuota();

    IDataPool_Element ElementConstructor();
    void ElementDestructor(IDataPool_Element element);

    IDataPool_Element ElementAt(int index);

    IDataPool_Element TakeElement();
    bool ReturnElement(IDataPool_Element element);
}

// Based on fast native-array
// O(1) selection
// O(1) take/return object operations
// NOTE: The active elements list may not correspond to adding order. Keep it in mind at selection from the list.
// No thread-safe
[System.Serializable]
public class DataPool : IDataPool
{
    [SerializeField]
    protected IDataPool_Element[] fObjectPool = null;
    [SerializeField]
    protected IDataPool_Element[] fPoolCache = null;
    [SerializeField]
    protected int fPoolUsedCount = 0;
    [SerializeField]
    protected int fPoolCapacity = 0;

    [SerializeField]
    protected bool fUseAutoGrow = false;
    [SerializeField]
    protected int fGrowQuota = 1;

    public void SetCapacity(int value)
    {
        if (fPoolCapacity == value)
            return;

        // Если новый размер меньше старого, то удаляем лишние элементы
        for (int i = value; i < fPoolCapacity; i++)
        {
            ElementDestructor(fObjectPool[i]);
            fObjectPool[i] = null;
        }

        if (value > 0)
        {
            fPoolCache = new IDataPool_Element[value];

            // Копирование данных старого массива
            for (int i = Mathf.Min(value, fPoolCapacity) - 1; i >= 0; i--)
            {
                fPoolCache[i] = fObjectPool[i];
                fObjectPool[i] = null;
            }

            fObjectPool = fPoolCache;
            fPoolCache = null;

            // Если новый размер больше старого, то добавляем недостающие элементы
            for (int i = fPoolCapacity; i < value; i++)
            {
                fObjectPool[i] = ElementConstructor();
            }
        }
        else
            value = 0;

        fPoolCapacity = value;

        if (fPoolUsedCount > fPoolCapacity)
            fPoolUsedCount = fPoolCapacity;
    }

    public int GetCapacity()
    {
        return fPoolCapacity;
    }

    public int GetUsedCount()
    {
        return fPoolUsedCount;
    }

    public int GetUnusedCount()
    {
        return fPoolCapacity - fPoolUsedCount;
    }

    public void SetUseAutogrow(bool value)
    {
        fUseAutoGrow = value;
    }

    public bool GetUseAutogrow()
    {
        return fUseAutoGrow;
    }

    public void SetGrowQuota(int value)
    {
        fGrowQuota = value;
    }

    public int GetGrowQuota()
    {
        return fGrowQuota;
    }

    public virtual IDataPool_Element ElementConstructor()
    {
        return null;
    }

    public virtual void ElementDestructor(IDataPool_Element element)
    {

    }

    public IDataPool_Element ElementAt(int index)
    {
        if ((index >= 0) &&
            (index < fPoolUsedCount))
            return fObjectPool[index];
        else
            Debug.LogError(string.Concat("The index ('", index, "') out of range (0, ", fPoolUsedCount, ")"));

        return null;
    }

    public IDataPool_Element TakeElement()
    {
        IDataPool_Element element;
        
        if (fPoolUsedCount >= fPoolCapacity)
        {
            if (fUseAutoGrow)
            {
                PoolCapacity += fGrowQuota;
            }
            else
            {
                Debug.LogError("Can't take a object from object-pool. Reason: The pool has no free elements");
                return null;
            }
        }

        DataPool_ElementData data;
        data.fOwner = this;
        data.fPoolIndex = fPoolUsedCount;

        element = fObjectPool[fPoolUsedCount];
        element.DataPool_Element_SetData(data);
        fPoolUsedCount++;

        return element;
    }

    public bool ReturnElement(IDataPool_Element element)
    {
        DataPool_ElementData data = element.DataPool_Element_GetData();

        if (element == null)
        {
            Debug.LogError("Can't return object. Reason: object is null");
            return false;
        }
        else if (data.fOwner != this)
        {
            Debug.LogError(string.Concat("Can't return object '", element, "' to this pool ('", this, "'). Reason: The pool has no ownership this object. "));
            return false;
        }

        fPoolUsedCount--;
        fObjectPool[data.fPoolIndex] = fObjectPool[fPoolUsedCount];
        fObjectPool[data.fPoolIndex].DataPool_Element_SetData(data);
        fObjectPool[fPoolUsedCount] = element;
        data.fPoolIndex = -1;
        element.DataPool_Element_SetData(data);
        return true;
    }

    public DataPool()
    {

    }

    public int PoolCapacity
    {
        get
        {
            return GetCapacity();
        }

        set
        {
            SetCapacity(value);
        }
    }

    public int UsedCount
    {
        get
        {
            return GetUsedCount();
        }
    }

    public bool UseAutoGrow
    {
        get
        {
            return GetUseAutogrow();
        }

        set
        {
            SetUseAutogrow(value);
        }
    }

    public int GrowQuota
    {
        get
        {
            return GetGrowQuota();
        }

        set
        {
            SetGrowQuota(value);
        }
    }

    public IDataPool_Element this[int index]
    {
        get
        {
            return ElementAt(index);
        }
    }
    
}
