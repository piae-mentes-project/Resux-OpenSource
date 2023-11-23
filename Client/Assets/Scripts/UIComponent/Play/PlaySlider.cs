using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Resux.UI
{
    public class PlaySlider : MonoBehaviour
    {
        #region properties

        [SerializeField] private Material material;
        private Image image;

        private float value;
        public float Value
        {
            get => value;
            set
            {
                this.value = value;
                image.material.SetFloat("_Value", value);
            }
        }

        #endregion

        void Start()
        {
            image = GetComponent<Image>();
            image.material = Material.Instantiate(material);
        }

        #region Public Method



        #endregion
    }
}