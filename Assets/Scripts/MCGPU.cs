using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class MCGPU : MonoBehaviour
{
    private int kernelMC;
    public ComputeShader MarchingCubesCS;
    
    void Awake()
    {
        kernelMC = MarchingCubesCS.FindKernel("MarchingCubes");
    }

    public ComputeBuffer ComputeSection(int idx, int step_size, Vector3Int resolution, Texture3D tex3D, float threshold)
    {
        // Size of struct Triangle is (3 v_pos's + 3 norm_vec's) * 3 v's = 18 floats
        float t_0 = Time.realtimeSinceStartup;
        ComputeBuffer appendVertexBuffer = new ComputeBuffer(resolution.x * resolution.y * step_size * 6, sizeof(float) * 18, ComputeBufferType.Append);
        MarchingCubesCS.SetInt("_resolution", resolution.x); // THIS IS TEMPORARY
        MarchingCubesCS.SetFloat("_threshold", threshold);
        MarchingCubesCS.SetInts("_idOffset", new int[3] { 0, 0, idx });

        MarchingCubesCS.SetBuffer(kernelMC, "triangleRW", appendVertexBuffer);
        MarchingCubesCS.SetInt("_triangleCount", 0);
        MarchingCubesCS.SetTexture(kernelMC, "_tex3D", tex3D);
        appendVertexBuffer.SetCounterValue(0);
        
        MarchingCubesCS.Dispatch(kernelMC, resolution.x, resolution.y, step_size);
        gameObject.GetComponent<MarchingCubes>().actual_proc_time += Time.realtimeSinceStartup - t_0;



        return appendVertexBuffer;
    }
}
