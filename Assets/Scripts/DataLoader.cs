using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataLoader : MonoBehaviour
{
    Dictionary<Vector3, float> voxels;

    public Dictionary<Vector3, float> getData()
    {
        voxels = new Dictionary<Vector3, float>();
        Vector3 curr = Vector3.zero;
        Vector3 center = Vector3.one * 4.5f;

        for (int i = 0; i < 10; i++)
        {
            curr.x = i;
            for (int j = 0; j < 10; j++)
            {
                curr.y = j;
                for (int k = 0; k < 10; k++)
                {
                    curr.z = k;
                    Vector3.Distance(curr, center);
                    voxels[new Vector3(i, j, k)] = Vector3.Distance(curr, center) < 3f ? 1f : 0f;
                }
            }
        }

        return voxels;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
