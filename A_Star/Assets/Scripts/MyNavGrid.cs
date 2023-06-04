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

    public int Distance(Vector2 start, Vector2 goal)
    {
        int distance = 0;
        Vector2Int target = new Vector2Int((int)goal.x, (int)goal.y);
        Vector2Int current = new Vector2Int((int) start.x, (int) start.y);

        while (current != target)
        {
            distance += DistanceForNextField(current, target);

            current = MoveOneFieldCloser(current, target);
        }
        return distance;
    }

    private int DistanceForNextField(Vector2Int current, Vector2Int target)
    {
        if (current.x != target.x && current.y != target.y) return 14;
        return 10;
    }

    private Vector2Int MoveOneFieldCloser(Vector2Int current, Vector2Int target)
    {
        Vector2Int nextField = current;
        if (nextField.x < target.x) nextField.x += 1;
        else if (nextField.x > target.x) nextField.x -= 1;

        if (nextField.y < target.y) nextField.y += 1;
        else if (nextField.y > target.y) nextField.y -= 1;

        return nextField;
    }
    
    private List<Vector2Int> GetReachableNeighboursOf(Vector2Int position, List<Vector2Int> closed)
    {
        List<Vector2Int> reachableNeighbours = new List<Vector2Int>();
        
        //Adjacent
        if (position.x > 0 && !_blocked[position.y, position.x - 1])
        {
            reachableNeighbours.Add(new Vector2Int(position.x - 1,position.y));
        }
        if (position.x < _blocked.GetLength(1) && !_blocked[position.y, position.x + 1])
        {
            reachableNeighbours.Add(new Vector2Int(position.x + 1, position.y));
        }
        if (position.y > 0 && !_blocked[position.y - 1, position.x])
        {
            reachableNeighbours.Add(new Vector2Int(position.x,position.y - 1));
        }
        if (position.y < _blocked.GetLength(0) && !_blocked[position.y + 1, position.x])
        {
            reachableNeighbours.Add(new Vector2Int(position.x, position.y+1));
        }

        //Diagonal with edge free rule
        if (reachableNeighbours.Contains(new Vector2Int(position.x - 1, position.y)) &&
            reachableNeighbours.Contains(new Vector2Int(position.x, position.y - 1)))
        {
            if(!_blocked[position.y-1, position.x-1]) reachableNeighbours.Add(new Vector2Int(position.x-1, position.y-1));
        }
        if (reachableNeighbours.Contains(new Vector2Int(position.x + 1, position.y)) &&
            reachableNeighbours.Contains(new Vector2Int(position.x, position.y - 1)))
        {
            if(!_blocked[position.y-1, position.x-1]) reachableNeighbours.Add(new Vector2Int(position.x+1, position.y-1));
        }
        if (reachableNeighbours.Contains(new Vector2Int(position.x - 1, position.y)) &&
            reachableNeighbours.Contains(new Vector2Int(position.x, position.y + 1)))
        {
            if(!_blocked[position.y-1, position.x-1]) reachableNeighbours.Add(new Vector2Int(position.x-1, position.y+1));
        }
        if (reachableNeighbours.Contains(new Vector2Int(position.x + 1, position.y)) &&
            reachableNeighbours.Contains(new Vector2Int(position.x, position.y + 1)))
        {
            if(!_blocked[position.y-1, position.x-1]) reachableNeighbours.Add(new Vector2Int(position.x+1, position.y+1));
        }

        return reachableNeighbours;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
    
}