using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathVisualizer : MonoBehaviour
{
    [SerializeField] private MyNavGrid grid;
    [SerializeField] private Vector2Int from;
    [SerializeField] private Vector2Int to;

    private void OnDrawGizmos()
    {
        if (grid is null) return;
        AStarNode goal = grid.FindPath(from, to);
        AStarNode current = goal;
        while (current != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(
                new Vector3((current.Position.x + 0.5f) * grid.transform.localScale.x / grid.Dimensions.x , 1,
                    (current.Position.y + 0.5f)  * grid.transform.localScale.z / grid.Dimensions.y), .25f);
            current = current.Parent;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}