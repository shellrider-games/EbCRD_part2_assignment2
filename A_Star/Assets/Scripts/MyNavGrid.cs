using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteAlways]
public class MyNavGrid : MonoBehaviour
{
    [SerializeField] private Vector2Int dimensions = new Vector2Int(10, 10);
    [SerializeField, Range(0f, 1f)] private float fieldFill = 0.8f;


    [SerializeField] private Material material;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    
    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _meshFilter.mesh = GenerateGridMesh(dimensions.x, dimensions.y, fieldFill);
        _meshRenderer.sharedMaterial = material;
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    private Mesh GenerateGridMesh(int columns, int rows, float fieldFill)
    {
        Vector3[] vertices = new Vector3[columns * rows * 4];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                vertices[4 * (i * rows + j) + 0] = new Vector3(
                    j / (float)columns + 1f / columns * fieldFill,
                    0,
                    i / (float)rows + 1f / rows * fieldFill
                );
                vertices[4 * (i * rows + j) + 1] = new Vector3(
                    j / (float)columns + 1f / columns * fieldFill,
                    0,
                    (i + 1) / (float)rows - 1f / rows * fieldFill
                );
                vertices[4 * (i * rows + j) + 2] = new Vector3(
                    (j + 1) / (float)columns - 1f / columns * fieldFill,
                    0, 
                    i / (float)rows + 1f / rows * fieldFill
                );
                vertices[4 * (i * rows + j) + 3] = new Vector3(
                    (j + 1) / (float)columns - 1f/ columns * fieldFill,
                    0,
                    (i + 1) / (float)rows - 1f / rows * fieldFill);
            }
        }
        int[] triangles = new int[columns * rows * 6];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                triangles[6 * (i * rows + j) + 0] = 4 * (i * rows + j) + 0;
                triangles[6 * (i * rows + j) + 1] = 4 * (i * rows + j) + 1;
                triangles[6 * (i * rows + j) + 2] = 4 * (i * rows + j) + 2;
                triangles[6 * (i * rows + j) + 3] = 4 * (i * rows + j) + 2;
                triangles[6 * (i * rows + j) + 4] = 4 * (i * rows + j) + 1;
                triangles[6 * (i * rows + j) + 5] = 4 * (i * rows + j) + 3;
            }
        }
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }
}