using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : MonoBehaviour
{
    public GameObject unitPrefab;

    public int numberUnitsPerSpawn;
    public float speed;

    public List<GameObject> units;
    public GridController gridController;

    void Start()
    {
        numberUnitsPerSpawn = 1;
        speed = 5f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Spawner();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            DestroyAllUnit();
        }
    }

    private void FixedUpdate()
    {
        if (gridController.currentFlowField == null) return;

        foreach (GameObject unit in units)
        {
            Cell nodeBelow = gridController.currentFlowField.GetCellPosWorldToGrid(unit.transform.position);
            Vector3 direction = new Vector3(nodeBelow.bestDirection.x, 0f, nodeBelow.bestDirection.y);
            Rigidbody unitRB = unit.GetComponent<Rigidbody>();
            unitRB.velocity = direction.normalized * speed;
        }
    }

    private void Spawner()
    {
        Vector2Int gridSize = gridController.gridSize;
        Vector2 maxSpawnPos = new Vector2(
            gridSize.x * gridController.cellSize + gridController.cellSize / 2,
            gridSize.y * gridController.cellSize + gridController.cellSize / 2
         );

        int colMask = LayerMask.GetMask("Wall", "Units");
        
        for (int i = 0; i < numberUnitsPerSpawn; i++)
        {
            GameObject unit = Instantiate(unitPrefab);
            unit.transform.parent = transform;

            do
            {
                unit.transform.position = new Vector3(Random.Range(0, maxSpawnPos.x), 0.75f, Random.Range(0, maxSpawnPos.y));
            }
            while (Physics.OverlapSphere(unit.transform.position, 0.25f, colMask).Length > 0);

            units.Add(unit);
        }
    }

    private void DestroyAllUnit()
    {
        foreach (GameObject unit in units)
        {
            Destroy(unit);
        }
        units.Clear();
    }
}
