using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolBetween : MonoBehaviour
{
    [SerializeField] private MyNavGrid navGrid;

    [SerializeField] private Transform from;
    [SerializeField] private Transform to;

    [SerializeField] private float walkTime;
    [SerializeField] private float rotateTime;

    private Vector2Int _gridFrom;
    private Vector2Int _gridTo;

    private List<Vector3> _patrolPoints;
    private int _currentPointIndex;
    private float _interpolationTime;

    private Vector3 _fromRotation;
    private Vector3 _toRotation;
    
    private bool _rotating = false;


    // Start is called before the first frame update
    void Start()
    {
        _gridFrom = navGrid.WorldPositionToGridPosition(from.position);
        _gridTo = navGrid.WorldPositionToGridPosition(to.position);
        _patrolPoints = CreatePatrolPoints(navGrid.FindPath(_gridFrom, _gridTo));
        _currentPointIndex = 0;
        _interpolationTime = 0;
        if (_patrolPoints.Count >= 2) transform.rotation = Quaternion.Euler(_patrolPoints[1] - _patrolPoints[0]);
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
            if (_rotating)
            {
                float c1 = 1.70158f;
                float c2 = c1 * 1.525f;
                
                _interpolationTime = Mathf.Clamp( _interpolationTime + Time.deltaTime, 0f, 1f*rotateTime);
                float t = _interpolationTime / rotateTime;
                float x = (t < 0.5f)
                    ? (Mathf.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) / 2
                    : (Mathf.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2f) + c2) + 2f) / 2;
                transform.rotation = Quaternion.Euler((1 - x) * _fromRotation + (x * _toRotation));
                if (t >= 1)
                {
                    _rotating = false;
                    _interpolationTime = 0;
                }
            }
            else
            {
                _interpolationTime = Mathf.Clamp( _interpolationTime + Time.deltaTime, 0f, 1f*walkTime);
                transform.position = _patrolPoints[_currentPointIndex] * (1 - _interpolationTime / walkTime) +
                                     _patrolPoints[_currentPointIndex + 1] * (_interpolationTime / walkTime);
                if (_interpolationTime >= walkTime)
                {
                    _currentPointIndex++;
                    if (_currentPointIndex < _patrolPoints.Count-1)
                    {
                        _interpolationTime = 0;
                        _fromRotation = transform.rotation.eulerAngles;
                        _toRotation = Quaternion.LookRotation((_patrolPoints[_currentPointIndex + 1] -
                                                               _patrolPoints[_currentPointIndex]).normalized, Vector3.up).eulerAngles;
                        _fromRotation.y %= 360f;
                        _toRotation.y %= 360f;
                        float difference = _fromRotation.y - _toRotation.y;
                        if (Mathf.Abs(difference) >= 0.001f)
                        {
                            _rotating = true;
                            if (Mathf.Abs(difference) > 180) _toRotation.y = difference > 0 ? _toRotation.y + 360f : _toRotation.y - 360f;
                        }
                    }
                }
            }
        }
    }
}