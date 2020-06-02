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
    public Button ToggleError;

    public float getThresholdSliderValue()
    {
        return ThresholdSliderValue;
    }

    // Start is called before the first frame update
    void Start()
    {
        ThresholdSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        ToggleError.onClick.AddListener(ToggleErrorVis);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ValueChangeCheck()
    {
        ThresholdSliderValue = ThresholdSlider.value;
    }

    public void ToggleErrorVis()
    {
        Text text = ToggleError.transform.GetChild(0).gameObject.GetComponent<Text>();
        if (text.text == "Show Error")
        {
            text.text = "Hide Error";
            GameObject.Find("DataVis").GetComponent<MarchingCubes>().SetMatToError();
        }
        else
        {
            text.text = "Show Error";
            GameObject.Find("DataVis").GetComponent<MarchingCubes>().SetMatToStandard();
        }
            
    }
}
