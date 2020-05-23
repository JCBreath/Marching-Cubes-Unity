using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotGrid : MonoBehaviour
{
    public Dictionary<Vector3, float> voxels;

    // Start is called before the first frame update
    void Start()
    {
        /*
        GetComponent<DataLoader>().loadData();
        voxels = GetComponent<DataLoader>().getData();

        foreach (KeyValuePair<Vector3, float> voxel in voxels)
        {
            Debug.Log(voxel.Key);
            GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dot.transform.parent = transform;
            dot.transform.position = voxel.Key;
            dot.transform.localScale = Vector3.one * .1f;
            dot.GetComponent<MeshRenderer>().material.color = Color.white * voxel.Value;

        }
        */
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
