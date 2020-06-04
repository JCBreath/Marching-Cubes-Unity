using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;
using Autodesk.Fbx;
using System.Text;

public class UI : MonoBehaviour
{
    public Slider ThresholdSlider;
    private float ThresholdSliderValue;
    public Text TrianglesCount;
    public Text VerticesCount;
    public Button ToggleError;
    public Dropdown ResDropdown;
    public Dropdown CompMethDropdown;
    public Dropdown DatasetDropdown;
    public Text ProcTime;
    public Text RecoTime;
    public Button ExportBtn;
    public Text ThreText;

    public float getThresholdSliderValue()
    {
        return ThresholdSliderValue;
    }

    // Start is called before the first frame update
    void Start()
    {
        ThresholdSlider.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        ToggleError.onClick.AddListener(ToggleErrorVis);
        ResDropdown.onValueChanged.AddListener(ChangeResolution);
        CompMethDropdown.onValueChanged.AddListener(ChangeComputeMethod);
        DatasetDropdown.onValueChanged.AddListener(ChangeDataset);
        ExportBtn.onClick.AddListener(ExportFBX);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ValueChangeCheck()
    {
        ThresholdSliderValue = ThresholdSlider.value;
        ThreText.text = string.Format("Threshold: {0}", ThresholdSliderValue.ToString("F3"));
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

    public void ChangeResolution(int idx)
    {
        GameObject.Find("DataVis").GetComponent<MarchingCubes>().ChangeResolution(Mathf.Pow(2, idx-2));
    }

    public void ChangeComputeMethod(int idx)
    {
        GameObject.Find("DataVis").GetComponent<MarchingCubes>().ChangeComputeMethod(idx);
    }

    public void ChangeDataset(int idx)
    {
        GameObject.Find("DataVis").GetComponent<MarchingCubes>().ChangeDataset(idx);
    }

    public static string MeshToString(MeshFilter mf)
    {
        Mesh m = mf.mesh;
        //Material[] mats = mf.renderer.sharedMaterials;

        StringBuilder sb = new StringBuilder();

        sb.Append("g ").Append(mf.name).Append("\n");
        foreach (Vector3 v in m.vertices)
        {
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
        }
        sb.Append("\n");
        foreach (Vector3 v in m.normals)
        {
            sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
        }
        sb.Append("\n");
        for(int i=0; i<m.triangles.Length; i+=3)
        {
            sb.Append(string.Format("f {0} {1} {2}\n", m.triangles[i]+1, m.triangles[i+1]+1, m.triangles[i+2]+1));
        }
        //for (int material = 0; material < m.subMeshCount; material++)
        //{
        //    sb.Append("\n");
        //    sb.Append("usemtl ").Append(mats[material].name).Append("\n");
        //    sb.Append("usemap ").Append(mats[material].name).Append("\n");

        //    int[] triangles = m.GetTriangles(material);
        //    for (int i = 0; i < triangles.Length; i += 3)
        //    {
        //        sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
        //            triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
        //    }
        //}
        return sb.ToString();
    }

    public static void MeshToFile(MeshFilter mf, string filename)
    {
        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(MeshToString(mf));
        }
    }

    public void ExportFBX()
    {
        // string filePath = Path.Combine(Application.dataPath, "MyGame.obj");
        MeshToFile(GameObject.Find("DataVis").GetComponent<MeshFilter>(), "model.obj");
        //void ExportScene(string fileName)
        //{
        //    using (FbxManager fbxManager = FbxManager.Create())
        //    {
        //        // configure IO settings.
        //        fbxManager.SetIOSettings(FbxIOSettings.Create(fbxManager, Globals.IOSROOT));

        //        // Export the scene
        //        using (FbxExporter exporter = FbxExporter.Create(fbxManager, "myExporter"))
        //        {

        //            // Initialize the exporter.
        //            bool status = exporter.Initialize(fileName, fbxManager.GetIOPluginRegistry().FindWriterIDByDescription("FBX ascii (*.fbx)"), fbxManager.GetIOSettings());

        //            // Create a new scene to export
        //            FbxScene scene = FbxScene.Create(fbxManager, "myScene");

        //            FbxDocumentInfo fbxSceneInfo = FbxDocumentInfo.Create(fbxManager, "SceneInfo");
        //            fbxSceneInfo.mTitle = "";
        //            fbxSceneInfo.mSubject = "";
        //            fbxSceneInfo.mAuthor = "Unity Technologies";
        //            fbxSceneInfo.mRevision = "1.0";
        //            fbxSceneInfo.mKeywords = "";
        //            fbxSceneInfo.mComment = "";

        //            scene.SetSceneInfo(fbxSceneInfo);

        //            FbxNode fbxNode = FbxNode.Create(scene, "Node");

        //            UnityEngine.Vector3 unityTranslate = GameObject.Find("DataVis").transform.localPosition;
        //            UnityEngine.Vector3 unityRotate = GameObject.Find("DataVis").transform.localRotation.eulerAngles;
        //            UnityEngine.Vector3 unityScale = GameObject.Find("DataVis").transform.localScale;

        //            // transfer transform data from Unity to Fbx
        //            // Negating the x value of the translation, and the y and z values of the rotation
        //            // to convert from Unity to Maya coordinates (left to righthanded)
        //            var fbxTranslate = new FbxDouble3(-unityTranslate.x, unityTranslate.y, unityTranslate.z);
        //            var fbxRotate = new FbxDouble3(unityRotate.x, -unityRotate.y, -unityRotate.z);
        //            var fbxScale = new FbxDouble3(unityScale.x, unityScale.y, unityScale.z);

        //            // set the local position of fbxNode
        //            fbxNode.LclTranslation.Set(fbxTranslate);
        //            fbxNode.LclRotation.Set(fbxRotate);
        //            fbxNode.LclScaling.Set(fbxScale);

        //            // create the mesh structure.
        //            FbxMesh fbxMesh = FbxMesh.Create(scene, "Mesh");

        //            Mesh mesh = GameObject.Find("DataVis").GetComponent<MeshFilter>().mesh;

        //            // Create control points.
        //            int NumControlPoints = mesh.vertices.Length;
        //            print(mesh.vertices.Length);
        //            fbxMesh.InitControlPoints(NumControlPoints);

        //            // copy control point data from Unity to FBX
        //            for (int v = 0; v < NumControlPoints; v++)
        //            {
        //                // convert from left to right-handed by negating x (Unity negates x again on import)
        //                fbxMesh.SetControlPointAt(new FbxVector4(-mesh.vertices[v].x, mesh.vertices[v].y, mesh.vertices[v].z), v);
        //            }

        //            /* 
        //             * Create polygons after FbxGeometryElementMaterial are created. 
        //             * TODO: Assign material indices.
        //             * Triangles have to be added in reverse order, 
        //             * or else they will be inverted on import 
        //             * (due to the conversion from left to right handed coords)
        //             */
        //            for (int f = 0; f < mesh.triangles.Length / 3; f++)
        //            {
        //                fbxMesh.BeginPolygon();
        //                fbxMesh.AddPolygon(mesh.triangles[3 * f + 2]);
        //                fbxMesh.AddPolygon(mesh.triangles[3 * f + 1]);
        //                fbxMesh.AddPolygon(mesh.triangles[3 * f]);
        //                fbxMesh.EndPolygon();
        //            }
        //            fbxNode.SetNodeAttribute(fbxMesh);
        //            fbxNode.SetShadingMode(FbxNode.EShadingMode.eWireFrame);
        //            // Export the scene to the file.
        //            exporter.Export(scene);
        //        }
        //    }
        //}
        //ExportScene("MYSCENE.fbx");
    }
}
