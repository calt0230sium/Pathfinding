using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    public Vector2Int gridSize;
    public float cellSize = 1.0f;
    public bool displayGrid;
    public bool displayFlowField;

    public FlowField currentFlowField;

    private bool isFlowFieldInit;

    void Start()
    {
        isFlowFieldInit = false;
        displayGrid = false;
        displayFlowField = true;
    }

    private void InitializeFlowField()
    {
        currentFlowField = new FlowField((cellSize), gridSize);
        currentFlowField.CreateGrid();

        // step 1 : init the cost field
        currentFlowField.CreateCostField();
        // step 2 : init the destination cell
        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 30f);
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        Cell destination = currentFlowField.GetCellPosWorldToGrid(worldPos);
        // step 3 : init the integration field
        currentFlowField.CreateIntegrationField(destination);
        // step 4 : init the flow field
        currentFlowField.CreateFlowField();

        isFlowFieldInit = true;
    }

    private void Update() {
        if (Input.GetMouseButtonDown(1)) InitializeFlowField();
        if (Input.GetKeyDown(KeyCode.G)) displayGrid = !displayGrid;
        if (Input.GetKeyDown(KeyCode.F)) displayFlowField = !displayFlowField;
    }

    private void OnDrawGizmos()
    {
        if (isFlowFieldInit)
        {
            if (displayGrid) DrawGrid(gridSize, Color.yellow, cellSize);
            if (displayFlowField) DrawFlowField(gridSize, cellSize);
        }
    }

    private void DrawGrid(Vector2Int gridSize, Color color, float cellSize)
    {
        Gizmos.color = color;

        foreach (Cell cell in currentFlowField.grid)
        {
            Vector3 center = cell.worldPos;
            Vector3 size = Vector3.one * cellSize;

            Gizmos.DrawWireCube(center, size);
            UnityEditor.Handles.Label(center, cell.bestCost.ToString());
        }
    }

    private static void DrawArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        if (direction != Vector3.zero)
        {
            Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
            Gizmos.DrawRay(pos, right * arrowHeadLength);
            Gizmos.DrawRay(pos, left * arrowHeadLength);
        }
    }

    private void DrawFlowField(Vector2Int gridSize, float cellSize)
    {
        foreach (Cell cell in currentFlowField.grid)
        {
            Vector3 center = cell.worldPos;
            DrawArrow(center, new Vector3(cell.bestDirection.x, 0f, cell.bestDirection.y));
        }
    }
}
