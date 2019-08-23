using UnityEngine;
using System.Collections;

public class CachedMonoBehaviour : MonoBehaviour
{
    protected GameObject fCached_GameObject = null;
    protected Transform fCached_Transform = null;


    public new GameObject gameObject
    {
        get
        {
            if (fCached_GameObject == null)
                fCached_GameObject = base.gameObject;

            return fCached_GameObject;
        }
    }

    public new Transform transform
    {
        get
        {
            if (fCached_Transform == null)
                fCached_Transform = base.transform;

            return fCached_Transform;
        }
    }

}
