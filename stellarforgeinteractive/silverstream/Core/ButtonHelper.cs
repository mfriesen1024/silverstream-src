using System;
using UnityEngine;
using UnityEngine.UI;

namespace ca.stellarforgeinteractive.silverstream.Core
{
    public class ButtonHelper:MonoBehaviour
    {
        public Action Clicked;
        public Button Button { get; private set; }

        void Start()
        {
            Button = GetComponent<Button>();
            Button.onClick.AddListener(_clicked);

            void _clicked()
            {
                Clicked();
            }
        }
    }
}