using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolBetween : MonoBehaviour
{
    [SerializeField] private MyNavGrid navGrid;

    [SerializeField] private Transform from;
    [SerializeField] private Transform to;

    [SerializeField] private float walkTime;

    private Vector2Int _gridFrom;
    private Vector2Int _gridTo;

    private List<Vector3> _patrolPoints;
    private int _currentPointIndex;
    private float _interpolationTime;


    // Start is called before the first frame update
    void Start()
    {
        _gridFrom = navGrid.WorldPositionToGridPosition(from.position);
        _gridTo = navGrid.WorldPositionToGridPosition(to.position);
        _patrolPoints = CreatePatrolPoints(navGrid.FindPath(_gridFrom, _gridTo));
        _currentPointIndex = 0;
        _interpolationTime = 0f;
    }

    private List<Vector3> CreatePatrolPoints(AStarNode node)
    {
        List<Vector3> patrolPoints = new List<Vector3>();
        AStarNode currentNode = node;
        while (currentNode != null)
        {
            patrolPoints.Add(navGrid.GridPositionToWorldPosition(currentNode.Position));
            currentNode = currentNode.Parent;
        }
        patrolPoints.Reverse();
        return patrolPoints;
    }

    // Update is called once per frame
    void Update()
    {
        if (_currentPointIndex < _patrolPoints.Count - 1)
        {
            _interpolationTime = Mathf.Clamp( _interpolationTime + Time.deltaTime, 0f, 1f*walkTime);
            transform.position = _patrolPoints[_currentPointIndex] * (1 - _interpolationTime / walkTime) +
                                 _patrolPoints[_currentPointIndex + 1] * (_interpolationTime / walkTime);
            if (_interpolationTime >= walkTime)
            {
                _currentPointIndex++;
                _interpolationTime = 0;
            }
        }
    }
}