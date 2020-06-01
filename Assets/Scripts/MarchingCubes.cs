﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEditor;
using UnityEngine;

public class MarchingCubes : MonoBehaviour
{
    Vector3Int data_dim;
    private int[][][] data;
    public Vector3Int resolution;
    Material mat;
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

    private int[][][] cellConfig;
    private float[][][] sampleBuffer;
    private Vector3Int[][][] cellVertices;
    private Vector3[][][][] cellEdges;

    Mesh mesh;
    private List<Vector3> final_v;
    private List<int> final_t;

    GameObject Canvas;
    UI CanvasUI;
    Vector3Int test_cell;

    Texture3D Tex3D;

    public struct TJob : IJob
    {
        public Vector3Int p;
        public int[][][] data;
        public float[][][] buffer;
        public Vector3Int resolution;
        public Vector3Int data_dim;

        public void Execute()
        {
            buffer[p.x][p.y][p.z] = trilinear(p, data, resolution, data_dim);
        }
    }

    Vector3 VoxToWorld(Vector3 p_vox)
    {
        return new Vector3(p_vox.x/resolution.x*data_dim.x, p_vox.y / resolution.y * data_dim.y, p_vox.z / resolution.z * data_dim.z);
    }

    static Vector3 WorldToVox(Vector3 p_world, Vector3 resolution, Vector3 data_dim)
    {
        return new Vector3(p_world.x / resolution.x * data_dim.x, p_world.y / resolution.y * data_dim.y, p_world.z / resolution.z * data_dim.z);
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

    void DrawCell(Vector3Int p)
    {
        //Vector3Int[] cell = new Vector3Int[8];
        //// cell[0] = new Vector3Int(p.x, p.y, p.z);
        //cell[0] = p;
        //cell[1] = cellVertices[p.x + 1][p.y][p.z];
        //cell[2] = cellVertices[p.x + 1][p.y + 1][p.z];
        //cell[3] = cellVertices[p.x][p.y + 1][p.z];
        //cell[4] = cellVertices[p.x][p.y][p.z + 1];
        //cell[5] = cellVertices[p.x + 1][p.y][p.z + 1];
        //cell[6] = cellVertices[p.x + 1][p.y + 1][p.z + 1];
        //cell[7] = cellVertices[p.x][p.y + 1][p.z + 1];

        Vector3Int getVertex(int i)
        {
            if(i == 0)
            {
                return p;
            }
            else if(i == 1)
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

        // cell[1] = new Vector3Int(p.x + 1, p.y, p.z);
        //cell[2] = new Vector3Int(p.x + 1, p.y + 1, p.z);
        //cell[3] = new Vector3Int(p.x, p.y + 1, p.z);
        //cell[4] = new Vector3Int(p.x, p.y, p.z + 1);
        //cell[5] = new Vector3Int(p.x + 1, p.y, p.z + 1);
        //cell[6] = new Vector3Int(p.x + 1, p.y + 1, p.z + 1);
        //cell[7] = new Vector3Int(p.x, p.y + 1, p.z + 1);

        int cube_index = cellConfig[p.x][p.y][p.z];

        //final_v[p.x + p.y * (resolution.x-1) + p.z * (resolution.y-1) * (resolution.z-1)] = new List<Vector3>();
        // print(p.x);

        int tri_num = TriangulationTable[cube_index].Length;

        if (tri_num == 0) { return; }
        

        for (int t_i = 0; t_i < tri_num; t_i++)
        {
            Vector3[] vertices = new Vector3[3];
            
            Vector3Int c0, c1;
            int[] edge;

            Vector3Int triangle = TriangulationTable[cube_index][t_i];

            edge = EdgeTable[triangle[0]];
            c0 = getVertex(edge[0]);
            c1 = getVertex(edge[1]);
            vertices[0] = Vector3.Lerp(c0, c1, (threshold - getBuffer(c0)) / (getBuffer(c1) - getBuffer(c0)));
            

            edge = EdgeTable[triangle[1]];
            c0 = getVertex(edge[0]);
            c1 = getVertex(edge[1]);

            vertices[1] = Vector3.Lerp(c0, c1, (threshold - getBuffer(c0)) / (getBuffer(c1) - getBuffer(c0)));
            

            edge = EdgeTable[triangle[2]];
            c0 = getVertex(edge[0]);
            c1 = getVertex(edge[1]);
            vertices[2] = Vector3.Lerp(c0, c1, (threshold - getBuffer(c0)) / (getBuffer(c1) - getBuffer(c0)));
            
            //Change Scale
            //vertices[0] = new Vector3(vertices[0].x / resolution.x, vertices[0].y / resolution.y, vertices[0].z / resolution.z);
            //vertices[1] = new Vector3(vertices[1].x / resolution.x, vertices[1].y / resolution.y, vertices[1].z / resolution.z);
            //vertices[2] = new Vector3(vertices[2].x / resolution.x, vertices[2].y / resolution.y, vertices[2].z / resolution.z);

            cellEdges[p.x][p.y][p.z][triangle[0]] = vertices[0];
            cellEdges[p.x][p.y][p.z][triangle[1]] = vertices[1];
            cellEdges[p.x][p.y][p.z][triangle[2]] = vertices[2];

            //if(SmoothNormal)
            //{
            //    int v0_idx = final_v.FindIndex(v => v == vertices[0]);
            //    if (v0_idx != -1)
            //        final_t.Add(v0_idx);
            //    else
            //    {
            //        final_t.Add(final_v.Count);
            //        final_v.Add(vertices[0]);
            //    }

            //    int v1_idx = final_v.FindIndex(v => v == vertices[1]);
            //    if (v1_idx != -1)
            //        final_t.Add(v1_idx);
            //    else
            //    {
            //        final_t.Add(final_v.Count);
            //        final_v.Add(vertices[1]);
            //    }

            //    int v2_idx = final_v.FindIndex(v => v == vertices[2]);
            //    if (v2_idx != -1)
            //        final_t.Add(v2_idx);
            //    else
            //    {
            //        final_t.Add(final_v.Count);
            //        final_v.Add(vertices[2]);
            //    }
            //}
            //else
            //{

            //final_v[p.x + p.y * (resolution.x - 1) + p.z * (resolution.y - 1) * (resolution.z - 1)].Add(vertices[0]);
            //final_v[p.x + p.y * (resolution.x - 1) + p.z * (resolution.y - 1) * (resolution.z - 1)].Add(vertices[1]);
            //final_v[p.x + p.y * (resolution.x - 1) + p.z * (resolution.y - 1) * (resolution.z - 1)].Add(vertices[2]);


            // print(p.x + p.y * (resolution.x-1) + p.z * (resolution.y-1) * (resolution.z-1));
            final_t.Add(final_v.Count);
            final_v.Add(vertices[0]);

            final_t.Add(final_v.Count);
            final_v.Add(vertices[1]);

            final_t.Add(final_v.Count);
            final_v.Add(vertices[2]);
            //}
        }
    }

    void Reconstruct()
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        cellConfig = new int[resolution.x][][];
        Parallel.For(0, resolution.x, i =>
        // for (int i = 0; i < (resolution.x); i++)
        {
            cellConfig[i] = new int[resolution.y][];
            // Parallel.For(0, resolution.y, j =>
            for (int j = 0; j < (resolution.y); j++)
            {
                cellConfig[i][j] = new int[resolution.z];
                // Parallel.For(0, resolution.z, k =>
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

        // TriangulationTable[cube_index].Length

        for (int i=0; i< (resolution.x); i++)
        {
            for (int j = 0; j < (resolution.y); j++)
            {
                for (int k = 0; k < (resolution.z); k++)
                {
                    if(TriangulationTable[cellConfig[i][j][k]].Length > 0)
                    {
                        cellEdges[i][j][k] = new Vector3[12];
                        DrawCell(new Vector3Int(i, j, k));
                    }
                        
                }
            }
        }


        mesh.vertices = final_v.ToArray();
        mesh.triangles = final_t.ToArray();

        mesh.RecalculateNormals();
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
        transform.localScale = new Vector3(1f/resolution.x, 1f/resolution.y, 1f/resolution.z);

        // CanvasUI.VerticesCount.text = final_v.Count.ToString(); 
        // CanvasUI.TrianglesCount.text = (final_t.Count/3).ToString();

        final_v.Clear();
        final_t.Clear();

        print("RECON");
    }
    void SaveAsset()
    {
        var mf = gameObject.GetComponent<MeshFilter>();
        if (mf)
        {
            var savePath = "Assets/" + threshold.ToString() + ".asset";
            Debug.Log("Saved Mesh to:" + savePath);
            AssetDatabase.CreateAsset(mf.mesh, savePath);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        dataLoader = GetComponent<DataLoader>();
        dataLoader.loadData();
        Tex3D = dataLoader.getTex3D();
        dataLoader.extendRange();
        data_dim = dataLoader.getDim();
        data = dataLoader.getData();
        
        
        mat = Resources.Load("Materials/Standard", typeof(Material)) as Material;
        mat.SetTexture("_Volume", Tex3D);
        mat.SetFloat("_Threshold", threshold);
        
        MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
        mr.material = mat;

        final_v = new List<Vector3>();
        final_t = new List<int>();

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

        sampleBuffer = new float[resolution.x+1][][];
        cellVertices = new Vector3Int[resolution.x + 1][][];
        cellEdges = new Vector3[resolution.x + 1][][][];
        float start = Time.realtimeSinceStartup;
        Parallel.For(0, resolution.x+1, i => 
        {
            sampleBuffer[i] = new float[resolution.y + 1][];
            cellVertices[i] = new Vector3Int[resolution.y + 1][];
            cellEdges[i] = new Vector3[resolution.y + 1][][];
            //Parallel.For(0, resolution.y+1, j =>
            for (int j=0; j<resolution.y+1; j++)    
            {
                sampleBuffer[i][j] = new float[resolution.z + 1];
                cellVertices[i][j] = new Vector3Int[resolution.z + 1];
                cellEdges[i][j] = new Vector3[resolution.z + 1][];
                // Parallel.For(0, resolution.z+1, k =>
                for (int k=0; k<resolution.z+1; k++)
                {
                    Vector3Int curr_p = new Vector3Int(i, j, k);
                    sampleBuffer[i][j][k] = trilinear(curr_p, data, resolution, data_dim);
                    cellVertices[i][j][k] = new Vector3Int(i, j, k);
                }
            }
        });
        print(string.Format("Interpolation Time: {0}", Time.realtimeSinceStartup - start));



        Reconstruct();
        
        // Debug.Log(TriangulationTable[162,2]);
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
        }

        if(SmoothNormalSaved != SmoothNormal)
        {
            SmoothNormalSaved = SmoothNormal;
            Reconstruct();
        }
    }

}
