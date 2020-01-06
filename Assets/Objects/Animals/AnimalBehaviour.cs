/**
-------------AnimalBehaviour.cs-------------
 Houses all the properties of an animal 
 and communicates with the rest 
 of the animal scripts
--------------------------------------------
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AnimalPathFinding), typeof(AnimalUIHandler), typeof(AnimalSensing))]
public class AnimalBehaviour : MonoBehaviour {
    AnimalPathFinding pathfinder;
    AnimalSensing sensing;

    [System.NonSerialized]
    public float energy = 100;
    [System.NonSerialized]
    public int status = 0; //0 = searching for food, 1 = heading towards food

    [Range(0, 100)]
    public int foodEnergyGain; //The amount of energy each food gives back
    [Range(0, 2)]
    public float energyDrainMultiplier; //Multiplier for energy loss
    [Range(0.1f, 10)]
    public float speed; //Speed of the animal in steps per second
    [Range(1, 30)]
    public int senseRadius; //The radius in which the Animal senses food

    [Range(1, 10)]
    public int searchRadius; //When searching for food, this is the range in which the next random point will be picked. Increasing this will decrease the chance of going back and fourth in quick succesion

    GameObject targetFood;

    Vector2Int planePosition;

    float EnergyFormula(float energy) { //The formula used for calculating energy loss each step. IMPORTANT: Speed energy loss is not linear as increasing the speed will also cause this to be called more frequently.
        energy -= (speed + senseRadius) * energyDrainMultiplier;
        return energy;
    }

    void Start() {
        pathfinder = GetComponent<AnimalPathFinding>();
        sensing = GetComponent<AnimalSensing>();

        planePosition = GridHelper.WorldPosToGridPos(transform.position);
        pathfinder.GoPath(GridHelper.GridPosToWorldPos(GridHelper.GetRandomPointInRadius(planePosition, searchRadius), transform.position.x), speed, OnStep, OnPathFinished); //Start searching for food
    }

    void Update() {
        if(energy <= 0) { //Destroy the animal if it has run out of energy
            Destroy(transform.GetComponent<AnimalUIHandler>().infoText);
            Destroy(gameObject);
        }

        planePosition = GridHelper.WorldPosToGridPos(transform.position);
    }

    public void OnStep() { //Will be called each step
        energy = EnergyFormula(energy); //Apply the energy formula
        switch(status) {
            case 0:
                if(sensing.closestFood != null) {
                    pathfinder.ClearPath();
                    status = 1;
                    targetFood = sensing.closestFood.gameObject;
                    pathfinder.GoPath(sensing.closestFood, speed, OnStep, OnPathFinished);
                }
                break;
            default:
                break;
        }
    }

    public void OnPathFinished() { //Will be called upon reaching the destination
        pathfinder.ClearPath();
        switch (status) {
            case 0:
                pathfinder.GoPath(GridHelper.GridPosToWorldPos(GridHelper.GetRandomPointInRadius(planePosition, searchRadius), transform.position.y), speed, OnStep, OnPathFinished);
                break;
            case 1:
                status = 0;
                Destroy(targetFood);
                targetFood = null;
                energy += foodEnergyGain;
                pathfinder.GoPath(GridHelper.GridPosToWorldPos(GridHelper.GetRandomPointInRadius(planePosition, searchRadius), transform.position.y), speed, OnStep, OnPathFinished);
                break;
            default:
                break;
        }
    }
}
