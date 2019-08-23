using UnityEngine;
using System.Collections;

public class GameObjectPool_Element: CachedMonoBehaviour, IDataPool_Element
{
    protected DataPool_ElementData fDataPool_ElementData;

    public void DataPool_Element_SetData(DataPool_ElementData data)
    {
        fDataPool_ElementData = data;
    }

    public DataPool_ElementData DataPool_Element_GetData()
    {
        return fDataPool_ElementData;
    }
}

// Based on fast native-array
// O(1) selection
// O(1) take/return object operations
// NOTE: The active elements list may not correspond to adding order. Keep it in mind at selection from the list.
// No thread-safe
[System.Serializable]
public class GameObjectPool: DataPool
{
    [SerializeField]
    protected GameObjectPool_Element fObjectPrototype = null;
    [SerializeField]
    protected Transform fPrototypeParent = null;
    [SerializeField]
    protected bool fObjectActivationState = false;


    public GameObjectPool()
    {

    }
    

    public GameObjectPool_Element ObjectPrototype
    {
        get
        {
            return fObjectPrototype;
        }

        set
        {
            fObjectPrototype = value;
        }
    }

    public bool ObjectActivationState
    {
        get
        {
            return fObjectActivationState;
        }

        set
        {
            fObjectActivationState = value;
        }
    }
    
    public Transform ObjectParent
    {
        get
        {
            return fPrototypeParent;
        }

        set
        {
            fPrototypeParent = value;

            for (int i=0; i<fPoolCapacity; i++)
            {
                GameObjectPool_Element o = fObjectPool[i] as GameObjectPool_Element;
                o.transform.parent = fPrototypeParent;
            }
        }
    }

    public override IDataPool_Element ElementConstructor()
    {
        base.ElementConstructor();

        GameObjectPool_Element sample = GameObject.Instantiate(fObjectPrototype, fPrototypeParent);
        return sample;
    }

    public override void ElementDestructor(IDataPool_Element element)
    {
        GameObjectPool_Element o = element as GameObjectPool_Element;
        GameObject.Destroy(o.gameObject);
        base.ElementDestructor(element);
    }
}


/*
using UnityEngine;
using System.Collections;

public class GameObjectPool_Element: CachedMonoBehaviour
{
    [SerializeField]
    protected internal GameObjectPool fOwner = null;
    [SerializeField]
    protected internal int fPoolIndex = -1;
}

// Based on fast native-array
// O(1) selection
// O(1) take/return object operations
// NOTE: The active elements list may not correspond to adding order. Keep it in mind at selection from the list.
// No thread-safe
[System.Serializable]
public class GameObjectPool
{
    [SerializeField]
    protected GameObjectPool_Element[] fObjectPool = null;
    protected GameObjectPool_Element[] fPoolCache = null;
    [SerializeField]
    protected int fPoolUsedCount = 0;
    [SerializeField]
    protected int fPoolCapacity = 0;
    [SerializeField]
    protected GameObjectPool_Element fObjectPrototype = null;
    [SerializeField]
    protected Transform fPrototypeParent = null;
    [SerializeField]
    protected bool fObjectActivationState = false;

    [SerializeField]
    protected bool fUseAutoGrow = false;
    [SerializeField]
    protected int fGrowQuota = 1;

    public GameObjectPool()
    {

    }
    
    public int PoolCapacity
    {
        get
        {
            return fPoolCapacity;
        }

        set
        {
            if (fPoolCapacity == value)
                return;

            // Если новый размер меньше старого, то удаляем лишние элементы
            for (int i = value; i < fPoolCapacity; i++)
            {
                DestroyObjectSample(fObjectPool[i]);
                fObjectPool[i] = null;
            }

            if (value > 0)
            {
                fPoolCache = new GameObjectPool_Element[value];

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
                    fObjectPool[i] = CreateObjectSample();
                }
            }
            else
                value = 0;

            fPoolCapacity = value;

            if (fPoolUsedCount > fPoolCapacity)
                fPoolUsedCount = fPoolCapacity;
        }
    }

    public int UsedCount
    {
        get
        {
            return fPoolUsedCount;
        }
    }

    public bool UseAutoGrow
    {
        get
        {
            return fUseAutoGrow;
        }

        set
        {
            fUseAutoGrow = value;
        }
    }

    public int GrowQuota
    {
        get
        {
            return fGrowQuota;
        }

        set
        {
            fGrowQuota = value;
        }
    }

    public GameObjectPool_Element ObjectPrototype
    {
        get
        {
            return fObjectPrototype;
        }

        set
        {
            fObjectPrototype = value;
        }
    }

    public bool ObjectActivationState
    {
        get
        {
            return fObjectActivationState;
        }

        set
        {
            fObjectActivationState = value;
        }
    }
    
    public Transform ObjectParent
    {
        get
        {
            return fPrototypeParent;
        }

        set
        {
            fPrototypeParent = value;

            for (int i=0; i<fPoolCapacity; i++)
            {
                fObjectPool[i].transform.parent = fPrototypeParent;
            }
        }
    }
    
    public GameObjectPool_Element ObjectAt(int index)
    {
        if ((index >= 0) &&
            (index < fPoolUsedCount))
            return fObjectPool[index];
        else
            Debug.LogError(string.Concat("The index ('", index,"') out of range (0, ", fPoolUsedCount, ")"));

        return null;
    }

    public GameObjectPool_Element this [int index]
    {
        get
        {
            return ObjectAt(index);
        }
    }

    public GameObjectPool_Element TakeObject()
    {
        GameObjectPool_Element element;

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

        element = fObjectPool[fPoolUsedCount];
        element.fPoolIndex = fPoolUsedCount;
        element.gameObject.SetActive(fObjectActivationState);
        fPoolUsedCount++;

        return element;
    }

    public bool ReturnObject(GameObjectPool_Element element)
    {
        if (element == null)
        {
            Debug.LogError("Can't return object. Reason: object is null");
            return false;
        }
        else if (element.fOwner != this)
        {
            Debug.LogError(string.Concat("Can't return object '", element, "' to this pool ('", this, "'). Reason: The pool has no ownership this object. "));
            return false;
        }

        fPoolUsedCount--;
        fObjectPool[element.fPoolIndex] = fObjectPool[fPoolUsedCount];
        fObjectPool[element.fPoolIndex].fPoolIndex = element.fPoolIndex;
        fObjectPool[fPoolUsedCount] = element;
        element.gameObject.SetActive(false);
        element.fPoolIndex = -1;
        return true;
    }

    public GameObjectPool_Element CreateObjectSample()
    {
        GameObjectPool_Element sample = GameObject.Instantiate(fObjectPrototype, fPrototypeParent);
        sample.fPoolIndex = -1;
        sample.fOwner = this;
        return sample;
    }

    public void DestroyObjectSample(GameObjectPool_Element _object)
    {
        GameObject.Destroy(_object.gameObject);
    }
}

 */
