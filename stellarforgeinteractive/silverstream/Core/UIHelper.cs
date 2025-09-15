using System;
using UnityEngine;
using UnityEngine.UI;

namespace ca.stellarforgeinteractive.silverstream.Core
{
    public class UIHelper:MonoBehaviour
    {
        
        public Action Clicked;
        private Button b;

        void Start()
        {
            b = GetComponent<Button>();
            b.onClick.AddListener(_clicked);

            void _clicked()
            {
                Clicked();
            }
        }
    }
}