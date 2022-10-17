using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class Vector2IntCompare : Comparer<Vector2Int>
{
    public override int Compare(Vector2Int x, Vector2Int y)
    {
        return Math.Abs(x.x) + Math.Abs(x.y) >= Math.Abs(y.x) + Math.Abs(y.y) ? 1 : -1;
    }
}

public class FlowField 
{
    public Cell[,] grid { get; private set; }
    public Vector2Int gridSize { get; private set; }
    public float cellRadius { get; private set; }
    public Cell destination;
    public int numNeighbors;
    private float cellSize;

    public static List<Vector2Int> AllDirections = new List<Vector2Int>();

    public FlowField (float _cellSize, Vector2Int _gridSize) 
    {
        cellSize = _cellSize;
        cellRadius = cellSize / 2;
        gridSize = _gridSize;
        numNeighbors = 2;

        if (AllDirections.Capacity == 0)
        {
            AllDirections.Clear();

            for (int i = -numNeighbors; i <= numNeighbors; i++)
            {
                for (int j = -numNeighbors; j <= numNeighbors; j++)
                {
                    if (i == 0 && j == 0) { continue; }
                    else { AllDirections.Add(new Vector2Int(i, j)); }
                }
            }
        }

        AllDirections.Sort(new Vector2IntCompare());
    }

    public void CreateGrid() 
    {
        grid = new Cell[gridSize.x, gridSize.y];

        for (int x = 0; x < gridSize.x; x++) 
        {
            for (int y = 0; y < gridSize.y; y++) 
            {
                Vector3 worldPos = new Vector3(cellSize * x + cellSize / 2, 0f, cellSize * y + cellSize / 2);
                grid[x, y] = new Cell(worldPos, new Vector2Int(x, y));
            } 
        }
    }

    public void CreateCostField() 
    {
        Vector3 cellHalfExtents = Vector3.one;
        int terrainMask = LayerMask.GetMask("Wall");

        foreach (Cell currentCell in grid) 
        {
            Collider[] obstacles = Physics.OverlapBox(currentCell.worldPos, cellHalfExtents, Quaternion.identity, terrainMask);

            foreach (Collider c in obstacles) 
            {
                if (c.gameObject.layer == 3)
                {
                    currentCell.IncreaseCost(255);
                }
            }
        }
    }

    public void CreateIntegrationField(Cell _destination) 
    {
        destination = _destination;

        destination.cost = 0;
        destination.bestCost = 0;

        Queue<Cell> cellsToCheck = new Queue<Cell>();
        cellsToCheck.Enqueue(destination);

        while (cellsToCheck.Count > 0)
        {
            Cell currentCell = cellsToCheck.Dequeue();
            List<Cell> currentNeighbors = GetNeighbors(currentCell.gridIndex);

            foreach (Cell currentNeighbor in currentNeighbors)
            {
                if (currentNeighbor.cost == byte.MaxValue) { continue; }

                if (currentNeighbor.cost + currentCell.bestCost < currentNeighbor.bestCost)
                {
                    currentNeighbor.bestCost = (ushort)(currentNeighbor.cost + currentCell.bestCost);
                    cellsToCheck.Enqueue(currentNeighbor);
                }
            }
        }
    }

    public void CreateFlowField()
    {
        foreach (Cell currentCell in grid)
        {
            List<Cell> currentNeighbors = GetNeighbors(currentCell.gridIndex);
            int bestCost = currentCell.bestCost;

            foreach (Cell currentNeighbor in currentNeighbors) 
            {
                if (currentNeighbor.bestCost < bestCost)
                {
                    bestCost = currentNeighbor.bestCost;
                    currentCell.bestDirection = currentNeighbor.gridIndex - currentCell.gridIndex;
                }
            }

        }
    }

    private List<Cell> GetNeighbors(Vector2Int nodeIndex)
    {
        List<Cell> neighbors = new List<Cell>();

        foreach (Vector2Int currentDirection in AllDirections)
        {
            Cell currentNeighbor = CellRelativePosition(nodeIndex, currentDirection);
            if (currentNeighbor != null) neighbors.Add(currentNeighbor);
        }

        return neighbors;
    }

    private Cell CellRelativePosition(Vector2Int originPos, Vector2Int relativePos)
    {
        Vector2Int pos = originPos + relativePos;

        if (pos.x < 0 || pos.x >= gridSize.x || pos.y < 0 || pos.y >= gridSize.y) return null;
        return grid[pos.x, pos.y];
    }

    public Cell GetCellPosWorldToGrid(Vector3 worldPos)
    {
        float percentX = Mathf.Clamp01(worldPos.x / (gridSize.x * cellSize));
        float percentY = Mathf.Clamp01(worldPos.z / (gridSize.y * cellSize));

        int x = Math.Clamp(Mathf.FloorToInt((gridSize.x) * percentX), 0, gridSize.x - 1);
        int y = Math.Clamp(Mathf.FloorToInt((gridSize.y) * percentY), 0, gridSize.y - 1);

        return grid[x, y];
    }
}