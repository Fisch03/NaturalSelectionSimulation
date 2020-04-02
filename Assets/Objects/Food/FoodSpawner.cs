using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FoodSpawner : MonoBehaviour {
    public GameObject food;
    public AnimalSimulation animalSimulation;

    public int startFoodNumber;
    public int maxFood;
    public Vector3 centerpos;
    public int radius;
    public AnimationCurve foodSpawnCurve = AnimationCurve.Linear(0, 0, 250, 5);
    [Range(0, 0.01f)]
    public float decreaseOverTime;

    float currentFoodAmount = 1;

    [System.NonSerialized]
    public List<Vector2Int> allFoodPositions = new List<Vector2Int>();

    void Start() {
        while(allFoodPositions.Count < startFoodNumber) {
            CreateNewFood();
        }

        StartCoroutine(FoodSpawnLoop());
        StartCoroutine(DecreaseFoodSpawn());
    }

    IEnumerator FoodSpawnLoop() {
        while(true) {
            if(allFoodPositions.Count < maxFood) {
                CreateNewFood();
            }
            yield return new WaitForSeconds(1 / (animalSimulation.simulationSpeed * currentFoodAmount * foodSpawnCurve.Evaluate(animalSimulation.transform.childCount) + 1));
        }
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

    IEnumerator DecreaseFoodSpawn() {
        while(true) {
            currentFoodAmount = currentFoodAmount / (decreaseOverTime+1);
            yield return new WaitForSeconds(1 / (animalSimulation.simulationSpeed + 1));
        }
    }

}