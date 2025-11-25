using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ca.stellarforgeinteractive.silverstream.Core
{
    public class ButtonHelper:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        public Action Clicked;
        public Action Hovered = EventSystem.DoNothing;
        public Action Init = EventSystem.DoNothing;
        public Action TextInit = EventSystem.DoNothing;
        
        public Button Button { get; private set; }
        public TextMeshProUGUI Text { get; private set; }
        public Image Image { get; private set; }

        Sprite spriteNormal;
        [SerializeField] Sprite spriteHover;

        void Start()
        {
            Button = GetComponent<Button>();
            Button.onClick.AddListener(_clicked);

            if (TryGetComponent(out Image image))
            {
                Image = image;
                spriteNormal=image.sprite;
            }
            else throw new NullReferenceException();
            
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            Hovered();
            if(spriteHover) Image.sprite = spriteHover;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Image.sprite = spriteNormal;
        }
    }
}