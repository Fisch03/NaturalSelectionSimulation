using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;
using System;

public class AnimalSimulation : MonoBehaviour {
    public GameObject animal;

    public FoodSpawner foodSpawner;

    public Transform animalCanvas;
    public Camera mainCamera;

    public int startAnimalCount;
    public Vector3 centerpos;
    public int radius;

    [Range(1, 1000)]
    public int simulationSpeed;

    public bool writeDataToFile;
    public float logFrequency;
    public int valueMultiplier;

    List<Vector2Int> allAnimalPositions = new List<Vector2Int>();

    int numberOfListEntries = 0;

    void Start() {
        while(allAnimalPositions.Count < startAnimalCount) {
            Vector2Int newPos = GridHelper.GetRandomPointInRadius(new Vector2Int(Mathf.RoundToInt(centerpos.x), Mathf.RoundToInt(centerpos.z)), radius);
            if (!allAnimalPositions.Contains(newPos)) {
                allAnimalPositions.Add(newPos);
                GameObject newAnimal = Instantiate(animal, GridHelper.GridPosToWorldPos(newPos, centerpos.y), Quaternion.LookRotation(Vector3.forward), transform);
                newAnimal.name = "Animal " + Mathf.Abs(newAnimal.GetInstanceID());
            }
        }

        if(writeDataToFile) {
            StartCoroutine(DataLogger());
        }
    }

    public GameObject NewAnimal(Vector3 position) {
        GameObject newAnimal = Instantiate(animal, position, Quaternion.LookRotation(Vector3.forward), transform);
        newAnimal.name = "Animal " + Mathf.Abs(newAnimal.GetInstanceID());
        return newAnimal;
    }

    IEnumerator DataLogger() {
        string filePath = Path.Combine(@"./Assets/SimulationLogs/", System.DateTime.Now.ToString("ddMMyyyy_HHmmss") + ".csv");
        using(StreamWriter writer = new StreamWriter(filePath)) {
            writer.WriteLine("Time,Number of Animals,Average sense radius, Average speed,Average reproduction threshold,Average passed energy");
            while(transform.childCount > 0) {
                numberOfListEntries++;
                float timeStamp = numberOfListEntries * (1/logFrequency);
                float animalCount = transform.childCount;
                
                float senseRadius = 0;
                float speed = 0;
                float reproductionThreshold = 0;
                float passedEnergy = 0;
                foreach(Transform animal in transform) {
                    AnimalBehaviour behaviour = animal.GetComponent<AnimalBehaviour>();
                    senseRadius += behaviour.senseRadius;
                    speed += behaviour.speed;
                    reproductionThreshold += behaviour.reproductionThreshold;
                    passedEnergy += behaviour.passedEnergy;
                }
                senseRadius = senseRadius / animalCount;
                speed = speed / animalCount;
                reproductionThreshold = reproductionThreshold / animalCount;
                passedEnergy = passedEnergy / animalCount;
                
                writer.WriteLine(FormatOutput($"{timeStamp},{animalCount},{senseRadius*valueMultiplier},{speed*valueMultiplier},{reproductionThreshold*valueMultiplier},{passedEnergy*valueMultiplier}"));
                yield return new WaitForSeconds(1 / (logFrequency * simulationSpeed));
            }
        }
    }

    static string FormatOutput(FormattableString formattable) { //Formats Strings with US Format (Decimals have a . instead of the German ,)
        return formattable.ToString(new CultureInfo("en-US"));
    }
}
