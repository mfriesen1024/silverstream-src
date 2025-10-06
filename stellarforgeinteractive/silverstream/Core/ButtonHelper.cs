using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ca.stellarforgeinteractive.silverstream.Core
{
    public class ButtonHelper:MonoBehaviour
    {
        public Action Clicked;
        public Action Init = EventSystem.DoNothing;
        public Action TextInit = EventSystem.DoNothing;
        
        public Button Button { get; private set; }
        public TextMeshProUGUI Text { get; private set; }
        

        void Start()
        {
            Button = GetComponent<Button>();
            Button.onClick.AddListener(_clicked);

            // If there's a text child component, fetch it.
            try
            {
                if (transform.GetChild(0).TryGetComponent(out TextMeshProUGUI text))
                {
                    // Debug.Log($"{name} found a text component.");
                    Text = text;
                    TextInit();
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            void _clicked()
            {
                Clicked();
            }
        }
    }
}