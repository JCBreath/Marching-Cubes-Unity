using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class MCGPU : MonoBehaviour
{
    private DataLoader dataLoader;
    private Vector3Int data_dim;
    public Texture3D tex;
    
    private int kernelMC;
    public ComputeShader MarchingCubesCS;
    // private ComputeBuffer appendVertexBuffer;
    // private ComputeBuffer argBuffer;
    public int Resolution;
    public Texture3D DensityTexture;
    
    void Awake()
    {
        kernelMC = MarchingCubesCS.FindKernel("MarchingCubes");
    }

    // public float[] ComputeSection(int idx, int n_div, Vector3Int resolution, Texture3D tex3D, float threshold)
    public ComputeBuffer ComputeSection(int idx, int step_size, Vector3Int resolution, Texture3D tex3D, float threshold)
    {
        // Size of struct Triangle is (3 v_pos's + 3 norm_vec's) * 3 v's = 18 floats
        //ComputeBuffer appendVertexBuffer = new ComputeBuffer((resolution.x) * (resolution.y) * (resolution.z) * 5 / n_div, sizeof(float) * 18, ComputeBufferType.Append);
        ComputeBuffer appendVertexBuffer = new ComputeBuffer(resolution.x * resolution.y * step_size * 5, sizeof(float) * 18, ComputeBufferType.Append);
        ComputeBuffer argBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
        MarchingCubesCS.SetInt("_gridSize", resolution.x); // THIS IS TEMPORARY
        MarchingCubesCS.SetFloat("_isoLevel", threshold);
        // MarchingCubesCS.SetInts("_idOffset", new int[3] { 0, 0, idx * Mathf.FloorToInt(resolution.z / n_div) }); // nth Section
        MarchingCubesCS.SetInts("_idOffset", new int[3] { 0, 0, idx });

        MarchingCubesCS.SetBuffer(kernelMC, "triangleRW", appendVertexBuffer);
        MarchingCubesCS.SetTexture(kernelMC, "_densityTexture", tex3D);
        appendVertexBuffer.SetCounterValue(0);
        float t_0 = Time.realtimeSinceStartup;
        // MarchingCubesCS.Dispatch(kernelMC, resolution.x, resolution.y, Mathf.FloorToInt(resolution.z / n_div));
        MarchingCubesCS.Dispatch(kernelMC, resolution.x, resolution.y, step_size);
        gameObject.GetComponent<MarchingCubes>().actual_proc_time += Time.realtimeSinceStartup - t_0;

        int[] args = new int[] { 0, 1, 0, 0 };
        argBuffer.SetData(args);

        ComputeBuffer.CopyCount(appendVertexBuffer, argBuffer, 0);

        argBuffer.GetData(args);
        args[0] *= 3;

        //float[] data = new float[args[0] * 6];
        //appendVertexBuffer.GetData(data, 0, 0, args[0] * 6);

        // appendVertexBuffer.Release();
        argBuffer.Release();

        return appendVertexBuffer;
    }

    // Start is called before the first frame update
    void Start()
    {
        //dataLoader = GetComponent<DataLoader>();
        //dataLoader.loadData();
        // resolution = dataLoader.getOriginalDim();
        //dataLoader.extendRange();

        //data_dim = dataLoader.getDim();
        // resolution = (data_dim - Vector3Int.one) / 4;
        // ChangeResolution(0.25f);
        //dataLoader.getData();
        //DensityTexture = dataLoader.getTex3D();

        //appendVertexBuffer = new ComputeBuffer((Resolution) * (Resolution) * (Resolution) * 5 / 4, sizeof(float) * 18, ComputeBufferType.Append);
        //argBuffer = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);

        //MarchingCubesCS.SetInt("_gridSize", Resolution);
        //MarchingCubesCS.SetFloat("_isoLevel", 0.5f);
        //MarchingCubesCS.SetInts("_idOffset", new int[3] { 0, 0, 0 });

        //MarchingCubesCS.SetBuffer(kernelMC, "triangleRW", appendVertexBuffer);
        //MarchingCubesCS.SetTexture(kernelMC, "_densityTexture", DensityTexture);
        //appendVertexBuffer.SetCounterValue(0);

        float start = Time.realtimeSinceStartup;
        //MarchingCubesCS.Dispatch(kernelMC, Resolution, Resolution, Resolution / 2);
        //// MarchingCubesCS.Dispatch(kernelMC, Resolution, Resolution, Resolution);
        //print(string.Format("GPU Computing Time: {0}", Time.realtimeSinceStartup - start));

        //int[] args = new int[] { 0, 1, 0, 0 };
        //argBuffer.SetData(args);


        //ComputeBuffer.CopyCount(appendVertexBuffer, argBuffer, 0);

        //argBuffer.GetData(args);
        //args[0] *= 3;
        //argBuffer.SetData(args);
        //Debug.Log("Vertex count:" + args[0]);

        //print("SIZE: " + appendVertexBuffer.count * 18);
        //// float[] data = new float[appendVertexBuffer.count * 18];
        //float[] data = new float[args[0] * 6];
        //appendVertexBuffer.GetData(data, 0, 0, args[0]*6);

        //float[] data = ComputeSection(0, 2, Vector3Int.one*64, DensityTexture);

        //int n = data.Length/6;
        //n = (n / 3) * 3;
        //List<Vector3> vertices = new List<Vector3>();
        //List<Vector3> normals = new List<Vector3>();
        //List<int> triangles = new List<int>();

        //// Parallel.For(0, n, i =>
        //for (int i = 0; i < n; i++)
        //{
        //    vertices.Add(new Vector3(data[i * 6], data[i * 6 + 1], data[i * 6 + 2]));
        //    normals.Add(new Vector3(data[i * 6 + 3], data[i * 6 + 4], data[i * 6 + 5]));
        //    triangles.Add(triangles.Count);
        //}

        //Mesh mesh = new Mesh();
        //mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        //mesh.vertices = vertices.ToArray();
        //mesh.triangles = triangles.ToArray();
        //mesh.normals = normals.ToArray();
        //// mesh.RecalculateNormals();
        //gameObject.AddComponent<MeshFilter>();
        //GetComponent<MeshFilter>().mesh = mesh;
        //gameObject.AddComponent<MeshRenderer>();
        //GetComponent<MeshRenderer>().material = Resources.Load("Materials/Standard", typeof(Material)) as Material;
        //print(string.Format("Total Reconstruction Time: {0}", Time.realtimeSinceStartup - start));
    }



    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {
        //appendVertexBuffer.Release();
        //argBuffer.Release();
    }
}
