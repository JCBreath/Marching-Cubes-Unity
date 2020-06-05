using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class DataLoader : MonoBehaviour
{
    public string file_name;
    byte[] raw_data;
    public Vector3Int data_dim;
    public Dictionary<Vector3, float> voxels;
    public int[][][] data;
    public int[] data_arr;
    public Texture3D tex;


    // public Dictionary<Vector3, float> getData()
    public int[][][] getData()
    {
        data = new int[data_dim.x][][];
        voxels = new Dictionary<Vector3, float>();
        Vector3 curr = Vector3.zero;
        Vector3 center = Vector3.one * (data_dim.x - 1) / 2f;

        float start = Time.realtimeSinceStartup;
        Parallel.For(0, data_dim.x, i =>
        {
            data[i] = new int[data_dim.y][];
            Parallel.For(0, data_dim.y, j =>
            {
                data[i][j] = new int[data_dim.z];
                Parallel.For(0, data_dim.z, k =>
                {
                    if (i == 0 || j == 0 || k == 0 || i == data_dim.x - 1 || j == data_dim.y - 1 || k == data_dim.z - 1)
                    {
                        data[i][j][k] = 0;
                    }
                    else
                    {
                        data[i][j][k] = raw_data[(i - 1) + (data_dim.y - 2) * (j - 1) + (data_dim.y - 2) * (data_dim.z - 2) * (k - 1)];
                    }
                });
            });
        });
        print(string.Format("Data Loading Time: {0}", Time.realtimeSinceStartup - start));
        return data;
    }

    public Texture3D getTex3D()
    {
        tex = new Texture3D(data_dim.x, data_dim.y, data_dim.z, TextureFormat.RGBA32, true);
        for (int i = 0; i < data_dim.x; i++)
        {
            for (int j = 0; j < data_dim.y; j++)
            {
                for (int k = 0; k < data_dim.z; k++)
                {
                    // int val = raw_data[i + data_dim.y * j + data_dim.y * data_dim.z * k];
                    int val = data[i][j][k];
                    tex.SetPixel(i, j, k, new Color(val / 255f, 0, 0, 1));
                }
            }
        }
        tex.Apply();
        return tex;
    }

    public Vector3Int getDim()
    {
        return data_dim - Vector3Int.one;
    }

    public void loadData()
    {
        raw_data = File.ReadAllBytes("Assets/Resources/Datasets/" + file_name + ".raw");


    }

    public void extendRange()
    {
        // Extend Data Range
        data_dim.x += 2;
        data_dim.y += 2;
        data_dim.z += 2;
    }
}