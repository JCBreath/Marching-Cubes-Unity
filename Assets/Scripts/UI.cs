using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Slider ThresholdSlider;
    private float ThresholdSliderValue;
    public Text TrianglesCount;
    public Text VerticesCount;

    public float getThresholdSliderValue()
    {
        return ThresholdSliderValue;
    }

    // Start is called before the first frame update
    void Start()
    {
        ThresholdSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ValueChangeCheck()
    {
        ThresholdSliderValue = ThresholdSlider.value;
    }
}
