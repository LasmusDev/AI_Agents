using Radishmouse;
using System.Collections;
using System.Linq;
using UnityEngine;

public class GraphDrawer : MonoBehaviour
{
    [Header("Setup in Editor before use")]
    public UILineRenderer angleGraphRenderer;
    public int noOfDisplayedSamples;
    [Tooltip("If there was no sample added since last frame, add a sample with value 0.")]
    public bool addSampleEveryFrame;
    public ZeroAxis axisPosition;
    public float maxSampleValue;

    bool sampleSet;
    float[] yValues;
    float[] xValues;
    int currentSample;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Setup array
        yValues = new float[noOfDisplayedSamples];
        //Setup indices - simple array. Scale those with renderer size to get X positions
        int[] indices = Enumerable.Range(0, noOfDisplayedSamples).ToArray();
        xValues = indices.Select(x => x * angleGraphRenderer.GetComponent<RectTransform>().sizeDelta.x / (noOfDisplayedSamples -1)).ToArray();
    }

    // Update is called once per frame
    void Update()
    {
        if (addSampleEveryFrame && !sampleSet)
        {
            AddSample(0);           
        }
        sampleSet = false;      
        //add points to the custom renderer
        angleGraphRenderer.points = xValues.Zip(yValues, (x, y) => new Vector2((float)x, y)).ToArray();
        angleGraphRenderer.SetAllDirty();
    }


    public void AddSample(float ySampleValue)
    {
        yValues[currentSample] = ySampleValue / maxSampleValue;
        //Add half the renderers height, so the 0 line is in the middle.
        float graphRenderHeight = angleGraphRenderer.GetComponent<RectTransform>().sizeDelta.y;

        switch (axisPosition)
        {
            case ZeroAxis.BOTTOM: break;
            case ZeroAxis.TOP: yValues[currentSample] *= -1;
                yValues[currentSample] += graphRenderHeight;
                break;
            case ZeroAxis.MID:
                yValues[currentSample] *= 0.5f;
                yValues[currentSample] += (graphRenderHeight / 2); break;
        }
        
        currentSample = (currentSample + 1) % (noOfDisplayedSamples);
        sampleSet = true;
    }

    public enum ZeroAxis
    {
        BOTTOM, MID, TOP
    }
}
