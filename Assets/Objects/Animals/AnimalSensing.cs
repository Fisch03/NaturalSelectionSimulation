/**
--------------AnimalSensing.cs--------------
 Handles the sensing of food for the animal
--------------------------------------------
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AnimalBehaviour))]
[ExecuteAlways]
public class AnimalSensing : MonoBehaviour {
    AnimalBehaviour behaviour;

    public bool drawGizmos;

    LayerMask foodLayer;

    [System.NonSerialized]
    public Collider[] foodInView;
    [System.NonSerialized]
    public Transform closestFood;

    void Start() {
        behaviour = GetComponent<AnimalBehaviour>();

        foodLayer = LayerMask.GetMask("Food");
    }

    void Update() {
        foodInView = Physics.OverlapSphere(transform.position, behaviour.senseRadius, foodLayer); //Overlap a sphere and find all food in it
        if (foodInView.Length != 0) {
            closestFood = FindClosestCollider(transform.position, foodInView).transform;
        } else {
            closestFood = null;
        }
    }

    Collider FindClosestCollider(Vector3 position, Collider[] colliders) { //Find the closest collider to a point from an array of colliders
        Collider bestCollider = colliders[0];
        float bestDistance = Vector3.Distance(position, colliders[0].transform.position);
        foreach(Collider collider in colliders) {
            float distance = Vector3.Distance(position, collider.transform.position);
            if(distance < bestDistance) {
                bestDistance = distance;
                bestCollider = collider;
            }
        }
        return bestCollider;
    }

    void OnDrawGizmos() { //Draw the sphere for debugging
        if (drawGizmos) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, behaviour.senseRadius);
        }
    }

    void OnDrawGizmosSelected() { //Mark the food in the radius and highlight the closest
        if(drawGizmos && foodInView.Length != 0 && closestFood != null) {
            Collider closest = FindClosestCollider(transform.position, foodInView);
            foreach (Collider food in foodInView) {
                if (food == closest) {
                    Gizmos.color = Color.red;
                } else {
                    Gizmos.color = Color.yellow;
                }
                Gizmos.DrawWireCube(food.transform.position, new Vector3(0.5f, 0.5f, 0.5f));
            }
        }    
    }
}
