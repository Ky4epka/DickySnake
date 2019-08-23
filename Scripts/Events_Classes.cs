using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[System.Serializable]
public class NotifyEvent_NP: UnityEvent
{

}

[System.Serializable]
public class NotifyEvent<P1>: UnityEvent<P1>
{

}


[System.Serializable]
public class NotifyEvent_2P<P1, P2> : UnityEvent<P1, P2>
{

}

[System.Serializable]
public class NotifyEvent_3P<P1, P2, P3> : UnityEvent<P1, P2, P3>
{

}

public class Events_Classes : MonoBehaviour {

}
