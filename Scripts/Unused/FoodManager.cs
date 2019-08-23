using UnityEngine;
using System.Collections;

public class FoodManager : CachedMonoBehaviour
{
    public NotifyEvent_2P<FoodManager, FoodBase> OnFoodCollision = new NotifyEvent_2P<FoodManager, FoodBase>();

    public FoodBase FoodPrototype = null;
    protected GameObjectPool fFoodPool = new GameObjectPool();

    public GameObjectPool FoodPool
    {
        get
        {
            return fFoodPool;
        }
    }



}
