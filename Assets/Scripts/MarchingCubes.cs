using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;

public class MarchingCubes : MonoBehaviour
{
    Vector3Int data_dim;
    private int[][][] data;
    public Vector3Int resolution;


    public enum ComputeMethod
    {
        CPUReuseEdge,
        CPUParallel,
        GPUParallel
    };

    public ComputeMethod computeMethod;

    private string[] DatasetNames;
    private Vector3Int[] DatasetDims;

    Material mat, StdMat;
    public float threshold = .5f;
    public bool SmoothNormal = false;
    bool SmoothNormalSaved = false;
    private static readonly int[,] TriangulationTableRaw = new int[,]
        {
        {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1},
        {3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1},
        {3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1},
        {3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1},
        {9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1},
        {9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
        {2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1},
        {8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1},
        {9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
        {4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1},
        {3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1},
        {1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1},
        {4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1},
        {4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
        {5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1},
        {2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1},
        {9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
        {0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
        {2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1},
        {10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1},
        {4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1},
        {5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1},
        {5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1},
        {9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1},
        {0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1},
        {1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1},
        {10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1},
        {8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1},
        {2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1},
        {7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1},
        {2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1},
        {11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1},
        {5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1},
        {11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1},
        {11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
        {1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1},
        {9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1},
        {5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1},
        {2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
        {5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1},
        {6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1},
        {3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1},
        {6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1},
        {5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1},
        {1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
        {10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1},
        {6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1},
        {8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1},
        {7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1},
        {3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
        {5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1},
        {0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1},
        {9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1},
        {8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1},
        {5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1},
        {0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1},
        {6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1},
        {10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1},
        {10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1},
        {8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1},
        {1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1},
        {0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1},
        {10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1},
        {3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1},
        {6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1},
        {9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1},
        {8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1},
        {3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1},
        {6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1},
        {0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1},
        {10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1},
        {10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1},
        {2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1},
        {7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1},
        {7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1},
        {2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1},
        {1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1},
        {11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1},
        {8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1},
        {0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1},
        {7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
        {10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
        {2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
        {6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1},
        {7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1},
        {2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1},
        {1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1},
        {10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1},
        {10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1},
        {0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1},
        {7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1},
        {6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1},
        {8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1},
        {9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1},
        {6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1},
        {4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1},
        {10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1},
        {8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1},
        {0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1},
        {1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1},
        {8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1},
        {10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1},
        {4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1},
        {10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
        {5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
        {11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1},
        {9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
        {6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1},
        {7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1},
        {3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1},
        {7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1},
        {3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1},
        {6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1},
        {9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1},
        {1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1},
        {4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1},
        {7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1},
        {6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1},
        {3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1},
        {0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1},
        {6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1},
        {0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1},
        {11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1},
        {6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1},
        {5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1},
        {9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1},
        {1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1},
        {1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1},
        {10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1},
        {0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1},
        {5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1},
        {10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1},
        {11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1},
        {9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1},
        {7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1},
        {2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1},
        {8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1},
        {9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1},
        {9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1},
        {1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1},
        {9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1},
        {9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1},
        {5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1},
        {0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1},
        {10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1},
        {2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1},
        {0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1},
        {0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1},
        {9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1},
        {5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1},
        {3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1},
        {5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1},
        {8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1},
        {0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1},
        {9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1},
        {1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1},
        {3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1},
        {4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1},
        {9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1},
        {11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1},
        {11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1},
        {2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1},
        {9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1},
        {3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1},
        {1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1},
        {4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1},
        {4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1},
        {3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1},
        {0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1},
        {9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1},
        {1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1}
        };
    private Vector3Int[][] TriangulationTable = new Vector3Int[256][];
    private static readonly int[,] EdgeConnection = new int[,]
    {
        {0,1}, {1,2}, {2,3}, {3,0},
        {4,5}, {5,6}, {6,7}, {7,4},
        {0,4}, {1,5}, {2,6}, {3,7}
    };
    private int[][] EdgeTable;
    private DataLoader dataLoader;
    private MCGPU mcGPU;

    private int[][][] cellConfig;
    private float[][][] sampleBuffer;
    private Vector3Int[][][] cellVertices;
    private int[][][][] cellEdges;
    private int[][][] vertexIndices;

    Mesh mesh;
    private List<Vector3> final_v;
    private List<int> final_t;
    private List<Triangle> tri_list;
    private object _sync = new object();

    GameObject Canvas;
    UI CanvasUI;
    Vector3Int test_cell;

    Texture3D Tex3D;
    private int ReuseCount;
    private int FailCount;
    private int CalcCount;

    public float actual_proc_time;

    public struct Triangle
    {
        public Vector3 [] v;
    }

    Vector3 VoxToWorld(Vector3 p_vox)
    {
        return new Vector3(p_vox.x/resolution.x*data_dim.x, p_vox.y / resolution.y * data_dim.y, p_vox.z / resolution.z * data_dim.z);
    }

    static Vector3 WorldToVox(Vector3 p_world, Vector3 resolution, Vector3 data_dim)
    {
        
        return new Vector3(p_world.x / (resolution.x) * (data_dim.x), p_world.y / (resolution.y) * (data_dim.y), p_world.z / (resolution.z) * (data_dim.z));
    }

    static float lerp(float x1, float x2, float y1, float y2, float x)
    {
        if(x2-x1 == 0) { return y1;  }
        return (y2 - y1) * ((x - x1) / (x2 - x1)) + y1;
    }

    static int getData(Vector3Int p, int[][][] data)
    {
        return data[p.x][p.y][p.z];
    }

    float getBuffer(Vector3Int p)
    {
        return sampleBuffer[p.x][p.y][p.z];
    }

    float getBufferXYZ(int x, int y, int z)
    {
        return sampleBuffer[x][y][z];
    }

    static float trilinear(Vector3Int p_world, int[][][] data, Vector3 resolution, Vector3 data_dim)
    {
        Vector3 p = WorldToVox(p_world, resolution, data_dim);
        Vector3Int p000 = new Vector3Int(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.y), Mathf.FloorToInt(p.z));
        Vector3Int p111 = new Vector3Int(Mathf.CeilToInt(p.x), Mathf.CeilToInt(p.y), Mathf.CeilToInt(p.z));

        Vector3Int p001 = new Vector3Int(p000.x, p000.y, p111.z);
        Vector3Int p010 = new Vector3Int(p000.x, p111.y, p000.z);
        Vector3Int p100 = new Vector3Int(p111.x, p000.y, p000.z);

        Vector3Int p011 = new Vector3Int(p000.x, p111.y, p111.z);
        Vector3Int p110 = new Vector3Int(p111.x, p111.y, p000.z);
        Vector3Int p101 = new Vector3Int(p111.x, p000.y, p111.z);

        float qx00 = lerp(p000.x, p100.x, getData(p000, data), getData(p100, data), p.x);
        float qx01 = lerp(p001.x, p101.x, getData(p001, data), getData(p101, data), p.x);
        float qx10 = lerp(p010.x, p110.x, getData(p010, data), getData(p110, data), p.x);
        float qx11 = lerp(p011.x, p111.x, getData(p011, data), getData(p111, data), p.x);

        float qxy0 = lerp(p000.y, p010.y, qx00, qx10, p.y);
        float qxy1 = lerp(p001.y, p011.y, qx01, qx11, p.y);

        float qxyz = lerp(p000.z, p001.z, qxy0, qxy1, p.z);

        return qxyz;
    }
    Vector3Int getVertex(int i, Vector3Int p)
    {
        if (i == 0)
        {
            return p;
        }
        else if (i == 1)
        {
            return cellVertices[p.x + 1][p.y][p.z];
        }
        else if (i == 2)
        {
            return cellVertices[p.x + 1][p.y + 1][p.z];
        }
        else if (i == 3)
        {
            return cellVertices[p.x][p.y + 1][p.z];
        }
        else if (i == 4)
        {
            return cellVertices[p.x][p.y][p.z + 1];
        }
        else if (i == 5)
        {
            return cellVertices[p.x + 1][p.y][p.z + 1];
        }
        else if (i == 6)
        {
            return cellVertices[p.x + 1][p.y + 1][p.z + 1];
        }
        else if (i == 7)
        {
            return cellVertices[p.x][p.y + 1][p.z + 1];
        }
        return Vector3Int.zero;
    }
    Vector3 lerpEdge(int[] edge, Vector3Int p)
    {
        Vector3Int c0, c1;
        c0 = getVertex(edge[0], p);
        c1 = getVertex(edge[1], p);
        return Vector3.Lerp(c0, c1, (threshold - getBuffer(c0)) / (getBuffer(c1) - getBuffer(c0)));
    }
    void DrawCell(Vector3Int p)
    {
        int reuseEdge(int edge_idx)
        {
            int edge = -1;
            int edge_0 = -1;
            int edge_1 = -1;
            int edge_2 = -1;
            if (p.x > 0)
            {
                if (edge_idx == 3)
                    edge_0 = cellEdges[p.x - 1][p.y][p.z][1];
                else if (edge_idx == 7)
                    edge_0 = cellEdges[p.x - 1][p.y][p.z][5];
                else if(edge_idx == 8)
                    edge_0 = cellEdges[p.x - 1][p.y][p.z][9];
                else if (edge_idx == 11)
                    edge_0 = cellEdges[p.x - 1][p.y][p.z][10];
            }

            if (p.y > 0)
            {
                if (edge_idx == 0)
                    edge_1 = cellEdges[p.x][p.y-1][p.z][2];
                else if (edge_idx == 4)
                    edge_1 = cellEdges[p.x][p.y-1][p.z][6];
                else if (edge_idx == 8)
                    edge_1 = cellEdges[p.x][p.y - 1][p.z][11];
                else if (edge_idx == 9)
                    edge_1 = cellEdges[p.x][p.y-1][p.z][10];
            }

            if (p.z > 0)
            {
                if (edge_idx == 0)
                    edge_2 = cellEdges[p.x][p.y][p.z - 1][4];
                else if (edge_idx == 1)
                    edge_2 = cellEdges[p.x][p.y][p.z-1][5];
                else if (edge_idx == 2)
                    edge_2 = cellEdges[p.x][p.y][p.z-1][6];
                else if (edge_idx == 3)
                    edge_2 = cellEdges[p.x][p.y][p.z - 1][7];
            }
            edge = Math.Max(edge_0, Math.Max(edge_1, edge_2));

            if (edge == -1)
            {
                CalcCount++;
            }
                
            return edge;
        }

        int cube_index = cellConfig[p.x][p.y][p.z];

        int tri_num = TriangulationTable[cube_index].Length;

        
        for (int t_i = 0; t_i < tri_num; t_i++)
        {
            Vector3Int triangle = TriangulationTable[cube_index][t_i];
            Triangle tri = new Triangle();
            tri.v = new Vector3[3];

            for(int i=0; i<3; i++)
            {
                // vertices[i] = reuseEdge(triangle[i]);
                // if (vertices[i].x == 0f && vertices[i].y == 0f && vertices[i].z == 0f)
                //Vector3 vertex;
                int v_i = reuseEdge(triangle[i]);
                if (v_i == -1)
                {
                    //vertex = lerpEdge(EdgeTable[triangle[i]]);
                    //int idx = final_v.FindIndex(v => v == vertex);
                    //if (idx != -1)
                    //    final_t.Add(idx);
                    //else
                    //{
                    //    final_t.Add(final_v.Count);
                    //    final_v.Add(vertex);
                    //}
                    //cellEdges[p.x][p.y][p.z][triangle[i]] = final_v.Count - 1;


                    FailCount++;
                    cellEdges[p.x][p.y][p.z][triangle[i]] = final_v.Count;
                    final_t.Add(final_v.Count);
                    float t_0 = Time.realtimeSinceStartup;
                    final_v.Add(lerpEdge(EdgeTable[triangle[i]], p));
                    actual_proc_time += Time.realtimeSinceStartup - t_0;
                }
                else
                {
                    cellEdges[p.x][p.y][p.z][triangle[i]] = v_i;
                    // vertices[i] = final_v[v_i];
                    final_t.Add(v_i);
                    ReuseCount++;
                }
            }
            lock(_sync)
            {
                tri_list.Add(tri);
            }
            
            //Change Scale
            //vertices[0] = new Vector3(vertices[0].x / resolution.x, vertices[0].y / resolution.y, vertices[0].z / resolution.z);
            //vertices[1] = new Vector3(vertices[1].x / resolution.x, vertices[1].y / resolution.y, vertices[1].z / resolution.z);
            //vertices[2] = new Vector3(vertices[2].x / resolution.x, vertices[2].y / resolution.y, vertices[2].z / resolution.z);
        }
    }

    void DrawCell_Parallel(Vector3Int p)
    {
        int cube_index = cellConfig[p.x][p.y][p.z];

        int tri_num = TriangulationTable[cube_index].Length;


        for (int t_i = 0; t_i < tri_num; t_i++)
        {
            Vector3Int triangle = TriangulationTable[cube_index][t_i];
            Triangle tri = new Triangle();
            tri.v = new Vector3[3];
            for (int i = 0; i < 3; i++)
            {

                tri.v[i] = lerpEdge(EdgeTable[triangle[i]], p);
                
            }
            lock (_sync)
            {
                tri_list.Add(tri);
            }
        }
    }

    void Reconstruct()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        actual_proc_time = 0f;

        if(computeMethod != ComputeMethod.GPUParallel)
        {
            ReuseCount = 0;
            FailCount = 0;
            CalcCount = 0;

            cellConfig = new int[resolution.x][][];

            Parallel.For(0, resolution.x, i =>
            // for (int i = 0; i < (resolution.x); i++)
            {
                cellConfig[i] = new int[resolution.y][];
                for (int j = 0; j < (resolution.y); j++)
                {
                    cellConfig[i][j] = new int[resolution.z];
                    for (int k = 0; k < (resolution.z); k++)
                    {
                        int cube_index = 0;
                        if (getBufferXYZ(i, j, k) < threshold) { cube_index += 1; }
                        if (getBufferXYZ(i + 1, j, k) < threshold) { cube_index += 2; }
                        if (getBufferXYZ(i + 1, j + 1, k) < threshold) { cube_index += 4; }
                        if (getBufferXYZ(i, j + 1, k) < threshold) { cube_index += 8; }
                        if (getBufferXYZ(i, j, k + 1) < threshold) { cube_index += 16; }
                        if (getBufferXYZ(i + 1, j, k + 1) < threshold) { cube_index += 32; }
                        if (getBufferXYZ(i + 1, j + 1, k + 1) < threshold) { cube_index += 64; }
                        if (getBufferXYZ(i, j + 1, k + 1) < threshold) { cube_index += 128; }
                        cellConfig[i][j][k] = cube_index;

                    }
                }
            });

            if (computeMethod == ComputeMethod.CPUParallel)
            {
                Parallel.For(0, resolution.x, i =>
                {
                    for (int j = 0; j < (resolution.y); j++)
                    {
                        for (int k = 0; k < (resolution.z); k++)
                        {
                            if (TriangulationTable[cellConfig[i][j][k]].Length > 0)
                            {
                                // cellEdges[i][j][k] = new int[12] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
                                DrawCell_Parallel(new Vector3Int(i, j, k));
                            }

                        }
                    }
                });


                Vector3[] vertices = new Vector3[tri_list.Count * 3];
                int[] triangles = new int[tri_list.Count * 3];

                // print(String.Format("v: {0}", tri_list[0].v[0]));
                float t_0 = Time.realtimeSinceStartup;
                Parallel.For(0, tri_list.Count, i =>
                {
                    for (int j = 0; j < 3; j++)
                    {
                        vertices[i * 3 + j] = tri_list[i].v[j];
                        triangles[i * 3 + j] = i * 3 + j;
                    }
                });
                actual_proc_time += Time.realtimeSinceStartup - t_0;
                tri_list.Clear();
                mesh.vertices = vertices;
                mesh.triangles = triangles;
            }
            else
            {
                for (int i = 0; i < (resolution.x); i++)
                {
                    for (int j = 0; j < (resolution.y); j++)
                    {
                        for (int k = 0; k < (resolution.z); k++)
                        {
                            if (TriangulationTable[cellConfig[i][j][k]].Length > 0)
                            {
                                cellEdges[i][j][k] = new int[12] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
                                DrawCell(new Vector3Int(i, j, k));
                            }

                        }
                    }
                }

                mesh.vertices = final_v.ToArray();
                mesh.triangles = final_t.ToArray();
                final_v.Clear();
                final_t.Clear();
                print(String.Format("Calculated: {0}, Reuse: {1}, Fail: {2}", CalcCount, ReuseCount, FailCount));
            }

            //Vector3[] normals = new Vector3[mesh.vertices.Length];
            //for (int i = 0; i < normals.Length; i++)
            //{
            //    Vector3 v = mesh.vertices[i];
            //    v.x /= resolution.x;
            //    v.y /= resolution.y;
            //    v.z /= resolution.z;
            //    float nx = Tex3D.GetPixelBilinear(v.x-1, v.y, v.z).r - Tex3D.GetPixelBilinear(v.x + 1, v.y, v.z).r;
            //    float ny = Tex3D.GetPixelBilinear(v.x, v.y-1, v.z).r - Tex3D.GetPixelBilinear(v.x, v.y+1, v.z).r;
            //    float nz = Tex3D.GetPixelBilinear(v.x, v.y, v.z-1).r - Tex3D.GetPixelBilinear(v.x, v.y, v.z+1).r;

            //    normals[i] = new Vector3(nx, ny, nz);
            //}

            //mesh.normals = normals;
            mesh.RecalculateNormals();
            transform.localScale = new Vector3(1f / resolution.x, 1f / resolution.y, 1f / resolution.z);
        }
        // USE GPU
        else
        {
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> triangles = new List<int>();

            //Vector3[] vertices;
            //Vector3[] normals;
            //int[] triangles;

            int n_sec;
            if(resolution.x > 128)
            {
                n_sec = Mathf.RoundToInt(Mathf.Pow(resolution.x / 128, 3));
            }
            else
            {
                n_sec = 1;
            }

            //if(resolution.x == 302)
            //{
            //    n_sec = 2;
            //}

            float[] tri_data;

            int step_size = resolution.z / n_sec;
            print(n_sec);
            //float[] buffer = new float[0];
            //Array buffer = new Array(0);

            //float[][] tri_buffer = new float[n_sec][];
            //int[] v_count = new int[n_sec];
            //int sec_count = 0;
            ComputeBuffer argBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
            int[] args = new int[1];
            // for (int sec_i=0; sec_i < n_sec; sec_i++)
            for (int sec_i=0; sec_i<resolution.z; sec_i+=step_size)
            {
                
                if (step_size > resolution.z - sec_i)
                    step_size = resolution.z - sec_i;
                ComputeBuffer cb = mcGPU.ComputeSection(sec_i, step_size, Vector3Int.one * resolution.x, Tex3D, threshold / 255);
                // float[] tri_data = mcGPU.ComputeSection(sec_i, n_sec, Vector3Int.one * resolution.x, Tex3D, threshold/255);
                
                
                // int[] args_set = new int[] { 0, 1, 0, 0 };
                
                // argBuffer.SetData(args_set);


                ComputeBuffer.CopyCount(cb, argBuffer, 0);

                //float t_0 = Time.realtimeSinceStartup;
                argBuffer.GetData(args);
                //actual_proc_time += Time.realtimeSinceStartup - t_0;
                //print(Time.realtimeSinceStartup - t_0);
                // argBuffer.Release();

                args[0] *= 3;
                
                tri_data = new float[args[0] * 6];
                
                cb.GetData(tri_data, 0, 0, args[0] * 6);

                cb.Release();

                

                int n = tri_data.Length / 6;

                n = (n / 3) * 3;

                //buffer = buffer.Concat(tri_data).ToArray();

                //Vector3[] vs = new Vector3[n];
                //Vector3[] ns = new Vector3[n];

                //tri_buffer[sec_count] = tri_data;
                //v_count[sec_count] = n;
                
                // Parallel.For(0, n/3, p_i =>
                for (int i = 0; i < n; i += 3)
                {
                    vertices.Add(new Vector3(tri_data[i * 6], tri_data[i * 6 + 1], tri_data[i * 6 + 2]));
                    normals.Add(new Vector3(tri_data[i * 6 + 3], tri_data[i * 6 + 4], tri_data[i * 6 + 5]));
                    triangles.Add(triangles.Count);

                    vertices.Add(new Vector3(tri_data[(i + 1) * 6], tri_data[(i + 1) * 6 + 1], tri_data[(i + 1) * 6 + 2]));
                    normals.Add(new Vector3(tri_data[(i + 1) * 6 + 3], tri_data[(i + 1) * 6 + 4], tri_data[(i + 1) * 6 + 5]));
                    triangles.Add(triangles.Count);

                    vertices.Add(new Vector3(tri_data[(i + 2) * 6], tri_data[(i + 2) * 6 + 1], tri_data[(i + 2) * 6 + 2]));
                    normals.Add(new Vector3(tri_data[(i + 2) * 6 + 3], tri_data[(i + 2) * 6 + 4], tri_data[(i + 2) * 6 + 5]));
                    triangles.Add(triangles.Count);

                }
                
                // sec_count++;

                tri_data = null;
                // GC.Collect();
            }

            //print(buffer.Length);

            //vertices = new Vector3[buffer.Length / 6];
            //normals = new Vector3[buffer.Length / 6];
            //triangles = new int[buffer.Length / 6];

            //Parallel.For(0, buffer.Length / 6, i =>
            //// for (int i=0; i< buffer.Length / 6; i++)
            //{
            //    vertices[i] = new Vector3(buffer[i * 6], buffer[i * 6 + 1], buffer[i * 6 + 2]);
            //    normals[i] = new Vector3(buffer[i * 6 + 3], buffer[i * 6 + 4], buffer[i * 6 + 5]);
            //    triangles[i] = i;
            //});

            //mesh.vertices = vertices;
            //mesh.triangles = triangles;
            //mesh.normals = normals;
            
            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.triangles = triangles.ToArray();

            // mesh.RecalculateNormals();

            vertices.Clear();
            normals.Clear();
            triangles.Clear();
            
            transform.localScale = Vector3.one;
        }

        cellConfig = null;

        CanvasUI.VerticesCount.text = String.Format("Vertices Count: {0}", mesh.vertices.Length);
        CanvasUI.TrianglesCount.text = String.Format("Triangles Count: {0}", mesh.triangles.Length / 3);
        CanvasUI.ProcTime.text = String.Format("Processing Time: {0}", actual_proc_time);
        
        print(string.Format("Actual Processing Time: {0}", actual_proc_time));

        gameObject.GetComponent<MeshFilter>().mesh = mesh;
    }
    //void SaveAsset()
    //{
    //    var mf = gameObject.GetComponent<MeshFilter>();
    //    if (mf)
    //    {
    //        var savePath = "Assets/" + threshold.ToString() + ".asset";
    //        Debug.Log("Saved Mesh to:" + savePath);
    //        AssetDatabase.CreateAsset(mf.mesh, savePath);
    //    }
    //}
    // Start is called before the first frame update

    void Init()
    {
        dataLoader.loadData();
        // resolution = dataLoader.getOriginalDim();
        dataLoader.extendRange();

        data_dim = dataLoader.getDim();
        resolution = (data_dim - Vector3Int.one) / 4;
        // ChangeResolution(0.25f);
        data = dataLoader.getData();
        Tex3D = dataLoader.getTex3D();

        // mat = Resources.Load("Materials/Error", typeof(Material)) as Material;
        mat.SetTexture("_Volume", Tex3D);
        mat.SetFloat("_Threshold", threshold);


    }

    void Start()
    {
        mcGPU = GetComponent<MCGPU>();
        dataLoader = GetComponent<DataLoader>();

        DatasetNames = new string[5] { "Neghip", "Bonsai", "Skull", "Nucleon", "Csafe" };
        DatasetDims = new Vector3Int[5] { new Vector3Int(64, 64, 64), new Vector3Int(256, 256, 256), new Vector3Int(256, 256, 256), new Vector3Int(41, 41, 41), new Vector3Int(302, 302, 302) };

        mat = Resources.Load("Materials/Error", typeof(Material)) as Material;
        StdMat = Resources.Load("Materials/Standard", typeof(Material)) as Material;

        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mr.material = StdMat;


        final_v = new List<Vector3>();
        final_t = new List<int>();
        tri_list = new List<Triangle>();

        Canvas = GameObject.Find("Canvas");
        CanvasUI = Canvas.GetComponent<UI>();

        gameObject.AddComponent<MeshFilter>();

        threshold = Mathf.RoundToInt(CanvasUI.getThresholdSliderValue() * 255f);

        Vector3Int test_cell = Vector3Int.zero;
        

        for (int i=0; i<256; i++)
        {
            if (TriangulationTableRaw[i, 9] != -1)
            {
                TriangulationTable[i] = new Vector3Int[4];
                TriangulationTable[i][0] = new Vector3Int(TriangulationTableRaw[i, 0], TriangulationTableRaw[i, 1], TriangulationTableRaw[i, 2]);
                TriangulationTable[i][1] = new Vector3Int(TriangulationTableRaw[i, 3], TriangulationTableRaw[i, 4], TriangulationTableRaw[i, 5]);
                TriangulationTable[i][2] = new Vector3Int(TriangulationTableRaw[i, 6], TriangulationTableRaw[i, 7], TriangulationTableRaw[i, 8]);
                TriangulationTable[i][3] = new Vector3Int(TriangulationTableRaw[i, 9], TriangulationTableRaw[i, 10], TriangulationTableRaw[i, 11]);
                
            }

            else if (TriangulationTableRaw[i, 6] != -1)
            {
                TriangulationTable[i] = new Vector3Int[3];
                TriangulationTable[i][0] = new Vector3Int(TriangulationTableRaw[i, 0], TriangulationTableRaw[i, 1], TriangulationTableRaw[i, 2]);
                TriangulationTable[i][1] = new Vector3Int(TriangulationTableRaw[i, 3], TriangulationTableRaw[i, 4], TriangulationTableRaw[i, 5]);
                TriangulationTable[i][2] = new Vector3Int(TriangulationTableRaw[i, 6], TriangulationTableRaw[i, 7], TriangulationTableRaw[i, 8]);
            }

            else if (TriangulationTableRaw[i, 3] != -1)
            {
                TriangulationTable[i] = new Vector3Int[2];
                TriangulationTable[i][0] = new Vector3Int(TriangulationTableRaw[i, 0], TriangulationTableRaw[i, 1], TriangulationTableRaw[i, 2]);
                TriangulationTable[i][1] = new Vector3Int(TriangulationTableRaw[i, 3], TriangulationTableRaw[i, 4], TriangulationTableRaw[i, 5]);
            }

            else if (TriangulationTableRaw[i, 0] != -1)
            {
                TriangulationTable[i] = new Vector3Int[1];
                TriangulationTable[i][0] = new Vector3Int(TriangulationTableRaw[i, 0], TriangulationTableRaw[i, 1], TriangulationTableRaw[i, 2]);
            }

            else if (TriangulationTableRaw[i, 0] == -1)
            {
                TriangulationTable[i] = new Vector3Int[] { };
            }
        }

        EdgeTable = new int[EdgeConnection.Length/2][];
        for(int i=0;i<EdgeConnection.Length / 2; i++)
        {
            EdgeTable[i] = new int[]{ EdgeConnection[i, 0] , EdgeConnection[i, 1] };
        }

        Debug.Log(TriangulationTable.Length);

        sampleBuffer = new float[resolution.x + 1][][];
        cellVertices = new Vector3Int[resolution.x + 1][][];
        cellEdges = new int[resolution.x + 1][][][];

        Init();
        ChangeResolution(0.25f);
        Reconstruct();

    }

    // Update is called once per frame
    void Update()
    {
        int NewThresholdValue = Mathf.RoundToInt(CanvasUI.getThresholdSliderValue() * 255f);
        if(threshold != NewThresholdValue)
        {
            threshold = NewThresholdValue;
            mat.SetFloat("_Threshold", threshold/255f);
            float start = Time.realtimeSinceStartup;
            Reconstruct();
            print(string.Format("Reconstruction Time: {0}", Time.realtimeSinceStartup - start));
            CanvasUI.RecoTime.text = String.Format("Reconstruction Time: {0}", Time.realtimeSinceStartup - start);
        }

        if(SmoothNormalSaved != SmoothNormal)
        {
            SmoothNormalSaved = SmoothNormal;
            Reconstruct();
        }

        mat.SetVector("_Position", transform.position);
    }

    public void SetMatToStandard()
    {
        gameObject.GetComponent<MeshRenderer>().material = StdMat;
    }

    public void SetMatToError()
    {
        gameObject.GetComponent<MeshRenderer>().material = mat;
    }

    public void ChangeResolution(float scale)
    {
        resolution.x = Mathf.FloorToInt((data_dim.x - 1) * scale);
        resolution.y = Mathf.FloorToInt((data_dim.y - 1) * scale);
        resolution.z = Mathf.FloorToInt((data_dim.z - 1) * scale);
        print(String.Format("Resolution Changed to: {0}", resolution));

        sampleBuffer = new float[resolution.x + 1][][];
        cellVertices = new Vector3Int[resolution.x + 1][][];
        cellEdges = new int[resolution.x + 1][][][];
        float start = Time.realtimeSinceStartup;
        Parallel.For(0, resolution.x + 1, i =>
        {
            sampleBuffer[i] = new float[resolution.y + 1][];
            cellVertices[i] = new Vector3Int[resolution.y + 1][];
            cellEdges[i] = new int[resolution.y + 1][][];
            //Parallel.For(0, resolution.y+1, j =>
            for (int j = 0; j < resolution.y + 1; j++)
            {
                sampleBuffer[i][j] = new float[resolution.z + 1];
                cellVertices[i][j] = new Vector3Int[resolution.z + 1];
                cellEdges[i][j] = new int[resolution.z + 1][];
                // Parallel.For(0, resolution.z+1, k =>
                for (int k = 0; k < resolution.z + 1; k++)
                {
                    Vector3Int curr_p = new Vector3Int(i, j, k);
                    sampleBuffer[i][j][k] = trilinear(curr_p, data, resolution, data_dim);
                    cellVertices[i][j][k] = new Vector3Int(i, j, k);
                }
            }
        });
        print(string.Format("Interpolation Time: {0}", Time.realtimeSinceStartup - start));
        start = Time.realtimeSinceStartup;
        Reconstruct();
        print(string.Format("Reconstruction Time: {0}", Time.realtimeSinceStartup - start));
    }

    public void ChangeComputeMethod(int idx)
    {
        computeMethod = (ComputeMethod)idx;
        float start = Time.realtimeSinceStartup;
        Reconstruct();
        CanvasUI.RecoTime.text = String.Format("Reconstruction Time: {0}", Time.realtimeSinceStartup - start);
    }

    public void ChangeDataset(int idx)
    {
        print(DatasetNames);
        dataLoader.file_name = DatasetNames[idx];
        dataLoader.data_dim = DatasetDims[idx];
        
        Init();
        int NewThresholdValue = Mathf.RoundToInt(CanvasUI.getThresholdSliderValue() * 255f);
        threshold = NewThresholdValue;
        mat.SetFloat("_Threshold", threshold / 255f);
        ChangeResolution(0.25f);
        CanvasUI.ResDropdown.value = 0;
        Reconstruct();
    }
}
