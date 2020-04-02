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
    AnimalSimulation simulation;

    [System.NonSerialized]
    public float energy = 100;
    [System.NonSerialized]
    public int status = 0; //0 = searching for food, 1 = heading towards food, 2 = searching for mate, 3 = heading towards mate, 4 = waiting for mate to arrive

    [Range(0, 100)]
    public int foodEnergyGain; //The amount of energy each food gives back
    [Range(0, 2)]
    public float energyDrainMultiplier; //Multiplier for energy loss
    [Range(1, 10)]
    public int searchRadius; //When searching for food, this is the range in which the next random point will be picked. Increasing this will decrease the chance of going back and fourth in quick succesion
    public float mutationAmount;

    //Genes
    [Range(0.1f, 10)]
    public float speed; //Speed of the animal in steps per second
    [Range(1, 30)]
    public float senseRadius; //The radius in which the Animal senses food
    public float reproductionThreshold; //If the energy is higher than this, the Animal will start searching for Partners to reproduce
    public float passedEnergy;

    [System.NonSerialized]
    public GameObject pathTarget;

    Vector2Int planePosition;

    float EnergyFormula(float energy) { //The formula used for calculating energy loss each step. IMPORTANT: Speed energy loss is not linear as increasing the speed will also cause this to be called more frequently.
        energy -= (speed + senseRadius) * energyDrainMultiplier;
        return energy;
    }

    void Start() {
        pathfinder = GetComponent<AnimalPathFinding>();
        sensing = GetComponent<AnimalSensing>();
        simulation = transform.parent.GetComponent<AnimalSimulation>();


        pathfinder.SetCallbacks(OnStep, OnPathFinished);

        planePosition = GridHelper.WorldPosToGridPos(transform.position);
        pathfinder.GoPath(GridHelper.GridPosToWorldPos(GridHelper.GetRandomPointInRadius(planePosition, searchRadius), transform.position.x), speed); //Start searching for food
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
        sensing.Sense();
        switch(status) {
            case 0:
                if(sensing.closestFood != null) {
                    pathfinder.ClearPath();
                    status = 1;
                    pathTarget = sensing.closestFood.gameObject;
                    pathfinder.GoPath(sensing.closestFood, speed);
                } else if(energy > reproductionThreshold) {
                    status = 2;
                }
                break;
            case 1:
                if(pathTarget == null) {
                    pathfinder.ClearPath();
                    status = 0;
                    GoSearchPath();
                }
                break;
            case 2:
                if(sensing.closestMate != null) {
                    AnimalBehaviour mateBehaviour = sensing.closestMate.GetComponent<AnimalBehaviour>();
                    if (mateBehaviour.status == 2) {
                        Vector2Int meetingPoint = mateBehaviour.PingMate(transform);
                        pathfinder.ClearPath();
                        status = 3;
                        pathTarget = sensing.closestMate.gameObject;
                        pathfinder.GoPath(GridHelper.GridPosToWorldPos(meetingPoint, transform.position.y), speed);
                    }
                } else if(energy < reproductionThreshold) {
                    status = 0;
                }
                break;
            case 3:
                if(pathTarget == null) {
                    pathfinder.ClearPath();
                    if (energy > reproductionThreshold) {
                        status = 2;
                    } else {
                        status = 0;
                    }
                    GoSearchPath();
                }
                break;
            default:
                break;
        }
    }

    public void OnPathFinished() { //Will be called upon reaching the destination
        pathfinder.ClearPath();
        switch(status) {
            case 0:
                GoSearchPath();
                break;
            case 1:
                if(pathTarget != null) {
                    simulation.foodSpawner.allFoodPositions.Remove(GridHelper.WorldPosToGridPos(pathTarget.transform.position));
                    Destroy(pathTarget);
                    energy += foodEnergyGain;
                }
                pathTarget = null;
                if(energy > reproductionThreshold) {
                    status = 2;
                } else {
                    status = 0;
                } 
                GoSearchPath();
                break;
            case 2:
                GoSearchPath();
                break;
            case 3:
                if(pathTarget != null) {
                    AnimalBehaviour mateBehaviour = pathTarget.GetComponent<AnimalBehaviour>();
                    if (mateBehaviour.status == 4) {
                        GameObject childAnimal = transform.parent.GetComponent<AnimalSimulation>().NewAnimal(transform.position);
                        AnimalBehaviour childBehaviour = childAnimal.GetComponent<AnimalBehaviour>();
                        energy -= passedEnergy;
                        mateBehaviour.energy -= mateBehaviour.passedEnergy;
                        childBehaviour.energy = passedEnergy + mateBehaviour.passedEnergy;
                        MutateChild(childBehaviour, mateBehaviour);
                        if (energy > reproductionThreshold) {
                            status = 2;
                        } else {
                            status = 0;
                        }
                        if (mateBehaviour.energy > mateBehaviour.reproductionThreshold) {
                            mateBehaviour.status = 2;
                        }
                        else {
                            mateBehaviour.status = 0;
                        }
                        pathTarget = null;
                        mateBehaviour.pathTarget = null;
                        GoSearchPath();
                        mateBehaviour.GoSearchPath();
                    } else {
                        status = 4;
                    }
                } else {
                    if (energy > reproductionThreshold) {
                            status = 2;
                        } else {
                            status = 0;
                    }
                    GoSearchPath();
                }
                break;
            default:
                break;
        }
    }

    public void GoSearchPath() { //Go a random Path to search for stuff
        Vector2Int target = GridHelper.GetRandomPointInRadius(planePosition, searchRadius);
        Vector3 worldTarget = GridHelper.GridPosToWorldPos(target, transform.position.y);
        pathfinder.GoPath(worldTarget, speed);
    }

    public Vector2Int PingMate(Transform mate) {
        pathfinder.ClearPath();
        status = 3;
        pathTarget = mate.gameObject;
        Vector2Int meetingPoint = GridHelper.GetMiddlePoint(GridHelper.WorldPosToGridPos(transform.position), GridHelper.WorldPosToGridPos(mate.position));
        pathfinder.GoPath(GridHelper.GridPosToWorldPos(meetingPoint, transform.position.y), speed);
        return meetingPoint;
    }

    void MutateChild(AnimalBehaviour child, AnimalBehaviour mate) {
        AnimalBehaviour chosenParent;
        
        chosenParent = (Random.value > 0.5) ? mate : this;
        child.speed = chosenParent.speed + NormalDistribution(0, 0.3f) * mutationAmount;
        if(child.speed < 0.2) {
            child.speed = 0.2f;
        }

        chosenParent = (Random.value > 0.5) ? mate : this;
        child.senseRadius = chosenParent.senseRadius + NormalDistribution(0, 0.5f) * mutationAmount;
        if(child.senseRadius < 1) {
            child.senseRadius = 1;
        }

        chosenParent = (Random.value > 0.5) ? mate : this;
        child.reproductionThreshold = chosenParent.reproductionThreshold + NormalDistribution(0, 1f) * mutationAmount;

        chosenParent = (Random.value > 0.5) ? mate : this;
        child.passedEnergy = chosenParent.passedEnergy + NormalDistribution(0, 1f) * mutationAmount;
        if(child.passedEnergy < 0) {
            child.passedEnergy = 0;
        }
    }

    //From https://stackoverflow.com/questions/218060/random-gaussian-variables
    float NormalDistribution(float mean, float deviation) {
        float u1 = 1f - Random.Range(0f, 0.9999f); //uniform(0,1] random doubles
        float u2 = 1f - Random.Range(0f, 0.9999f);
        float randStdNormal = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Sin(2f * Mathf.PI * u2); //random normal(0,1)
        float randNormal = mean + deviation * randStdNormal; //random normal(mean,stdDev^2)
        return randNormal;
    }
}
