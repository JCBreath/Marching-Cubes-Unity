using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataLoader : MonoBehaviour
{
    public byte[] raw_data;
    public Vector3Int data_dim;
    public Dictionary<Vector3, float> voxels;
    public int[][][] data;

    // public Dictionary<Vector3, float> getData()
    public int[][][] getData()
    {
        data = new int[data_dim.x][][];
        voxels = new Dictionary<Vector3, float>();
        Vector3 curr = Vector3.zero;
        Vector3 center = Vector3.one * (data_dim.x - 1)/2f;

        for (int i = 0; i < data_dim.x; i++)
        {
            curr.x = i;
            data[i] = new int[data_dim.y][];
            for (int j = 0; j < data_dim.y; j++)
            {
                curr.y = j;
                data[i][j] = new int[data_dim.z];
                for (int k = 0; k < data_dim.z; k++)
                {
                    curr.z = k;
                    // Vector3.Distance(curr, center);
                    // voxels[new Vector3(i, j, k)] = Vector3.Distance(curr, center) < (data_dim.x - 1) / 3f ? 1f : 0f;
                    // data[i][j][k] = Vector3.Distance(curr, center) < (data_dim.x - 1) / 3f ? 1 : 0;
                    data[i][j][k] = raw_data[i + data_dim.y * j + data_dim.y * data_dim.z * k];
                    // voxels[new Vector3(i, j, k)] = raw_data[i + data_dim.y * j + data_dim.y * data_dim.z * k] / 256f;
                    // Debug.Log(voxels[new Vector3(i, j, k)]);
                }
            }
        }

        return data;
    }

    public Vector3Int getDim()
    {
        return data_dim - Vector3Int.one;
    }

    public void loadData()
    {
        raw_data = File.ReadAllBytes("Assets/Resources/Datasets/neghip.raw");
        /*
        for(int i=0;i<raw_data.Length;i++)
        {
            if(raw_data[i] > 0)
            {
                Debug.Log(raw_data[i]);
            }
        }
        */
        // raw_data = ta.bytes;
        
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
