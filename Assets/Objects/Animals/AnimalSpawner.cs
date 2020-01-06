using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour {
    public GameObject animal;

    public Transform animalCanvas;
    public Camera mainCamera;

    public int animalCount;
    public Vector3 centerpos;
    public int radius;

    List<Vector2Int> allAnimalPositions = new List<Vector2Int>();

    void Start() {
        while(allAnimalPositions.Count < animalCount) {
            Vector2Int newPos = GridHelper.GetRandomPointInRadius(new Vector2Int(Mathf.RoundToInt(centerpos.x), Mathf.RoundToInt(centerpos.z)), radius);
            if (!allAnimalPositions.Contains(newPos)) {
                allAnimalPositions.Add(newPos);
                GameObject newAnimal = Instantiate(animal, GridHelper.GridPosToWorldPos(newPos, centerpos.y), Quaternion.LookRotation(Vector3.forward), transform);
                newAnimal.name = "Animal " + Mathf.Abs(newAnimal.GetInstanceID());
            }
        }
    }
}
