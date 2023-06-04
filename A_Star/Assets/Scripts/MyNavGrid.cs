using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class AStarNode
{
    public Vector2Int Position;
    public int GCost;
    [CanBeNull] public AStarNode Parent;

    public AStarNode(Vector2Int position, int gCost, AStarNode parent)
    {
        this.Position = position;
        this.GCost = gCost;
        this.Parent = parent;
    }
}

[ExecuteAlways]
public class MyNavGrid : MonoBehaviour
{
    [SerializeField] private Vector2Int dimensions = new Vector2Int(10, 10);
    private bool[,] _blocked;

    public Vector2Int Dimensions => dimensions;

    private void OnDrawGizmos()
    {
        CalculateWalkable();
        for (int i = 0; i < dimensions.y; i++)
        {
            for (int j = 0; j < dimensions.x; j++)
            {
                if (_blocked[i, j])
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
                        transform.localScale.x / (4 * dimensions.x),
                        2,
                        transform.localScale.z / (4 * dimensions.y)
                    )
                );
                if (colliders.Length > 0) _blocked[i, j] = true;
            }
        }
    }

    public AStarNode FindPath(Vector2Int from, Vector2Int target)
    {
        List<AStarNode> closed = new List<AStarNode>();
        List<AStarNode> open = new List<AStarNode>();
        AStarNode current = new AStarNode(from, 0, null);
        closed.Add(current);
        do
        {
            List<Vector2Int> neighbours = GetReachableNeighboursOf(current.Position);
            foreach (Vector2Int position in neighbours)
            {
                if (closed.Find(node => node.Position == position) is not null) continue;
                AStarNode alreadyExists = open.Find(node => node.Position == position);

                if (alreadyExists is not null)
                {
                    if (alreadyExists.Position.x != position.x && alreadyExists.Position.y != position.y)
                    {
                        if (current.GCost + 14 < alreadyExists.GCost)
                        {
                            alreadyExists.GCost = current.GCost + 14;
                            alreadyExists.Parent = current;
                        }
                    }
                    else
                    {
                        if (current.GCost + 10 < alreadyExists.GCost)
                        {
                            alreadyExists.GCost = current.GCost + 10;
                            alreadyExists.Parent = current;
                        }
                    }
                }
                else
                {
                    if (current.Position.x != position.x && current.Position.y != position.y)
                    {
                        open.Add(new AStarNode(position, current.GCost + 14, current));
                    }
                    else
                    {
                        open.Add(new AStarNode(position, current.GCost + 10, current));
                    }
                }
            }
            AStarNode next = LowestFCost(open, target);
            if (next.Position == target)
            {
                return next;
            }

            open.Remove(next);
            closed.Add(next);
            current = next;
        } while (open.Count > 0);

        return LowestHCost(closed, target);
    }
    
    public int Distance(Vector2 start, Vector2 goal)
    {
        int distance = 0;
        Vector2Int target = new Vector2Int((int)goal.x, (int)goal.y);
        Vector2Int current = new Vector2Int((int)start.x, (int)start.y);

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
    
    private AStarNode LowestFCost(List<AStarNode> list, Vector2Int target)
    {
        if (list.Count == 0) return null;
        AStarNode lowestFCost = list[0];
        for (int i = 1; i < list.Count; i++)
        {
            if (list[i].GCost + Distance(list[i].Position, target) <
                lowestFCost.GCost + Distance(lowestFCost.Position, target))
            {
                lowestFCost = list[i];
            }
        }

        return lowestFCost;
    }

    private AStarNode LowestHCost(List<AStarNode> list, Vector2Int target)
    {
        if (list.Count == 0) return null;
        AStarNode lowestHCost = list[0];
        
        for (int i = 1; i < list.Count; i++)
        {
            if (Distance(list[i].Position, target) <
                Distance(lowestHCost.Position, target))
            {
                lowestHCost = list[i];
            }
        }

        return lowestHCost;
    }

    private List<Vector2Int> GetReachableNeighboursOf(Vector2Int position)
    {
        List<Vector2Int> reachableNeighbours = new List<Vector2Int>();

        //Adjacent
        if (position.x > 0 && !_blocked[position.y, position.x - 1])
        {
            reachableNeighbours.Add(new Vector2Int(position.x - 1, position.y));
        }

        if (position.x < _blocked.GetLength(1) - 1 && !_blocked[position.y, position.x + 1])
        {
            reachableNeighbours.Add(new Vector2Int(position.x + 1, position.y));
        }

        if (position.y > 0 && !_blocked[position.y - 1, position.x])
        {
            reachableNeighbours.Add(new Vector2Int(position.x, position.y - 1));
        }

        if (position.y < _blocked.GetLength(0) - 1 && !_blocked[position.y + 1, position.x])
        {
            reachableNeighbours.Add(new Vector2Int(position.x, position.y + 1));
        }

        //Diagonal with edge free rule
        if (reachableNeighbours.Contains(new Vector2Int(position.x - 1, position.y)) &&
            reachableNeighbours.Contains(new Vector2Int(position.x, position.y - 1)))
        {
            if (!_blocked[position.y - 1, position.x - 1])
                reachableNeighbours.Add(new Vector2Int(position.x - 1, position.y - 1));
        }

        if (reachableNeighbours.Contains(new Vector2Int(position.x + 1, position.y)) &&
            reachableNeighbours.Contains(new Vector2Int(position.x, position.y - 1)))
        {
            if (!_blocked[position.y - 1, position.x + 1])
                reachableNeighbours.Add(new Vector2Int(position.x + 1, position.y - 1));
        }

        if (reachableNeighbours.Contains(new Vector2Int(position.x - 1, position.y)) &&
            reachableNeighbours.Contains(new Vector2Int(position.x, position.y + 1)))
        {
            if (!_blocked[position.y + 1, position.x - 1])
                reachableNeighbours.Add(new Vector2Int(position.x - 1, position.y + 1));
        }

        if (reachableNeighbours.Contains(new Vector2Int(position.x + 1, position.y)) &&
            reachableNeighbours.Contains(new Vector2Int(position.x, position.y + 1)))
        {
            if (!_blocked[position.y + 1, position.x + 1])
                reachableNeighbours.Add(new Vector2Int(position.x + 1, position.y + 1));
        }

        return reachableNeighbours;
    }
}