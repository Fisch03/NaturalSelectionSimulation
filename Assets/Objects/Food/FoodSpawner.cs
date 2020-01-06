using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour {
    public GameObject food;

    public int startFoodNumber;
    public int maxFood;
    public float spawnDelay;
    public Vector3 centerpos;
    public int radius;
    

    List<Vector2Int> allFoodPositions = new List<Vector2Int>();

    void Start() {
        while(allFoodPositions.Count < startFoodNumber) {
            CreateNewFood();
        }

        InvokeRepeating("CreateNewFood", spawnDelay, spawnDelay);
    }

    void CreateNewFood() {
        Vector2Int newPos = GridHelper.GetRandomPointInRadius(new Vector2Int(Mathf.RoundToInt(centerpos.x), Mathf.RoundToInt(centerpos.z)), radius);
        int generateTries = 0;
        while (generateTries < 10) {
            if (!allFoodPositions.Contains(newPos)) {
                allFoodPositions.Add(newPos);
                GameObject newFood = Instantiate(food, GridHelper.GridPosToWorldPos(newPos, centerpos.y), Quaternion.LookRotation(Vector3.forward), transform);
                newFood.name = "Food " + Mathf.Abs(newFood.GetInstanceID());
            } else {
                generateTries++;
            }
        }
    }
}
