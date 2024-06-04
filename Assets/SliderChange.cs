using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace OpenSkiJumping
{
    public class SliderChange : MonoBehaviour
    {
        public Slider slider;
        public TMP_Text sliderText;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            sliderText.text = ((int)slider.value).ToString();
        }
    }
}
