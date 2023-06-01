using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]
public class MyNavGrid : MonoBehaviour
{
    [SerializeField] private Vector2Int dimensions = new Vector2Int(10, 10);
    private bool[,] _blocked;

    private void OnDrawGizmos()
    {
        
        CalculateWalkable();
        for (int i = 0; i < dimensions.y; i++)
        {
            for (int j = 0; j < dimensions.x; j++)
            {
                if (_blocked[i,j])
                {
                    Gizmos.color = new Color(1, 0, 0, 0.5f);
                }
                else
                {
                    Gizmos.color = new Color(0, 1, 0, 0.5f);
                }
                Gizmos.DrawSphere(transform.position +
                                  new Vector3((transform.localScale.x * (j + 0.5f)) / dimensions.x, 0.25f,
                                      (transform.localScale.z * (i + 0.5f)) / dimensions.y), 0.2f);
            }
        }
    }

    private void Awake()
    {
        CalculateWalkable();
        
    }

    private void CalculateWalkable()
    {
        _blocked = new bool[dimensions.y, dimensions.x];
        for (int i = 0; i < dimensions.y; i++)
        {
            for (int j = 0; j < dimensions.x; j++)
            {
                Collider[] colliders = Physics.OverlapBox(transform.position + new Vector3(
                        transform.localScale.x * (j + 0.5f) / dimensions.x,
                        0,
                        transform.localScale.z * (i + 0.5f) / dimensions.y),
                    new Vector3(
                        transform.localScale.x / (4*dimensions.x) ,
                        2,
                        transform.localScale.z / (4*dimensions.y)
                    )
                );
                if (colliders.Length > 0) _blocked[i,j] = true;
            }
        }
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    
}