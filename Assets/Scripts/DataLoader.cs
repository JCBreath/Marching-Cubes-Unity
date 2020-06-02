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
        //int size = 32;
        //TextureFormat format = TextureFormat.RGBA32;
        //TextureWrapMode wrapMode = TextureWrapMode.Clamp;

        //// Create the texture and apply the configuration
        //Texture3D texture = new Texture3D(size, size, size, format, false);
        //texture.wrapMode = wrapMode;

        //// Create a 3-dimensional array to store color data
        //Color[] colors = new Color[size * size * size];

        //// Populate the array so that the x, y, and z values of the texture will map to red, blue, and green colors
        //float inverseResolution = 1.0f / (size - 1.0f);
        //for (int z = 0; z < size; z++)
        //{
        //    int zOffset = z * size * size;
        //    for (int y = 0; y < size; y++)
        //    {
        //        int yOffset = y * size;
        //        for (int x = 0; x < size; x++)
        //        {
        //            colors[x + yOffset + zOffset] = new Color(x * inverseResolution,
        //                y * inverseResolution, z * inverseResolution, 1.0f);
        //        }
        //    }
        //}

        //// Copy the color values to the texture
        //texture.SetPixels(colors);

        //// Apply the changes to the texture and upload the updated texture to the GPU
        //texture.Apply();
        //return texture;

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