using System;
using UnityEngine;
using UnityEngine.UI;

namespace ca.stellarforgeinteractive.silverstream.Core
{
    internal class SliderHelper:MonoBehaviour
    {
        public Action<float> Updated;
        
        public Slider Slider { get; private set; }
        

        void Start()
        {
            Slider = GetComponent<Slider>();
            Slider.onValueChanged.AddListener(_clicked);

            void _clicked(float value)
            {
                Updated(value);
            }
        }
    }
}