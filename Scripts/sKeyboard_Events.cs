using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace KeyboardEvents
{

    [System.Serializable]
    public class sKeyboard_Event : UnityEvent<KeyCode>
    {

    }

    public interface IKeyHandler
    {
        bool OnKeyDown(KeyCode key);
        bool OnKeyUp(KeyCode key);
        bool OnKeyRepeat(KeyCode key);
    }
    
    public class sKeyboard_Events : MonoBehaviour
    {
        public float KeyRepeatDelay = 1f;
        public float KeyRepeatInterval = 0.5f;

        public LinkedList2<IKeyHandler> Handlers = new LinkedList2<IKeyHandler>();
        public IKeyHandler GlobalHandler = null;

        public List<KeyCode> KeyCodeCollection = new List<KeyCode>();

        private static sKeyboard_Events fCurrent = null;
        private Dictionary<KeyCode, bool> sKeyState = new Dictionary<KeyCode, bool>();
        private Dictionary<KeyCode, bool> sKeyRepeats = new Dictionary<KeyCode, bool>();
        private Dictionary<KeyCode, float> sKeyTime = new Dictionary<KeyCode, float>();
        private bool fInit = true;


        public void KeyCodesUpdated()
        {
            sKeyState.Clear();
            sKeyRepeats.Clear();
            sKeyTime.Clear();

            for (int i = 0; i < KeyCodeCollection.Count; i++)
            {
                sKeyState.Add(KeyCodeCollection[i], false);
                sKeyRepeats.Add(KeyCodeCollection[i], false);
                sKeyTime.Add(KeyCodeCollection[i], 0f);
            }
        }

        public void ProcessKeyDown(KeyCode key)
        {
            if ((GlobalHandler != null) &&
                !GlobalHandler.OnKeyDown(key))
                return;

            LinkedListNode<IKeyHandler> node = Handlers.First;

            while (node != null)
            {
                if (!node.Value.OnKeyDown(key))
                    break;

                node = node.Next;
            }
        }

        public void ProcessKeyUp(KeyCode key)
        {
            if ((GlobalHandler != null) &&
                !GlobalHandler.OnKeyUp(key))
                return;

            LinkedListNode<IKeyHandler> node = Handlers.First;

            while (node != null)
            {
                if (!node.Value.OnKeyUp(key))
                    break;

                node = node.Next;
            }
        }

        public void ProcessKeyRepeat(KeyCode key)
        {
            if ((GlobalHandler != null) &&
                !GlobalHandler.OnKeyRepeat(key))
                return;

            LinkedListNode<IKeyHandler> node = Handlers.First;

            while (node != null)
            {
                if (!node.Value.OnKeyRepeat(key))
                    break;

                node = node.Next;
            }
        }

        public static sKeyboard_Events Current
        {
            get
            {
                return fCurrent;
            }
        }

        private void Awake()
        {
            if (fCurrent == null)
                fCurrent = this;
        }

        // Update is called once per frame
        void Update()
        {
            if (fInit)
            {
                fInit = false;
                KeyCodesUpdated();
            }

            KeyCode code;

            for (int i = 0; i < KeyCodeCollection.Count; i++)
            {
                code = KeyCodeCollection[i];

                if (Input.GetKeyDown(code))
                {
                    sKeyState[code] = true;
                    sKeyRepeats[code] = false;
                    sKeyTime[code] = Time.time;
                    ProcessKeyDown(code);
                }

                if (Input.GetKeyUp(code))
                {
                    sKeyState[code] = false;
                    sKeyRepeats[code] = false;
                    ProcessKeyUp(code);
                }

                if (sKeyState[code])
                {
                    if (((sKeyRepeats[code]) && (Time.time - sKeyTime[code] >= KeyRepeatInterval)) ||
                         (Time.time - sKeyTime[code] >= KeyRepeatDelay))
                    {
                        if (!sKeyRepeats[code]) sKeyRepeats[code] = true;

                        sKeyTime[code] = Time.time;
                        ProcessKeyRepeat(code);
                    }
                }
            }
        }
    }
}