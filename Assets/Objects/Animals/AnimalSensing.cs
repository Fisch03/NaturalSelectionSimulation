/**
--------------AnimalSensing.cs--------------
 Handles the sensing of food and 
 mates for the animal
--------------------------------------------
 **/

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AnimalBehaviour))]
[ExecuteAlways]
public class AnimalSensing : MonoBehaviour {
    AnimalBehaviour behaviour;

    public bool drawGizmos;

    LayerMask foodLayer;
    LayerMask animalLayer;

    [System.NonSerialized]
    public Collider[] foodInView;
    [System.NonSerialized]
    public Collider[] matesInView;
    [System.NonSerialized]
    public Transform closestFood;
    [System.NonSerialized]
    public Transform closestMate;

    void Start() {
        behaviour = GetComponent<AnimalBehaviour>();

        foodLayer = LayerMask.GetMask("Food");
        animalLayer = LayerMask.GetMask("Animals");
    }

    public void Sense() {
        foodInView = Physics.OverlapSphere(transform.position, behaviour.senseRadius, foodLayer); //Overlap a sphere and find all food in it
        if (foodInView.Length > 0) {
            closestFood = FindClosestCollider(transform.position, foodInView).transform;
        } else {
            closestFood = null;
        }

        gameObject.layer = LayerMask.NameToLayer("Default");
        gameObject.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Default");
        matesInView = Physics.OverlapSphere(transform.position, behaviour.senseRadius, animalLayer); //Overlap a sphere and find all potential mates in it
        gameObject.layer = LayerMask.NameToLayer("Animals");
        gameObject.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Animals");
        if (matesInView.Length > 0) {
            closestMate = FindClosestCollider(transform.position, matesInView).transform.parent;
        } else {
            closestMate = null;
        }
    }

    Collider FindClosestCollider(Vector3 position, Collider[] colliders) { //Find the closest collider to a point from an array of colliders
        Collider bestCollider = colliders[0];
        float bestDistance = Vector3.Distance(position, colliders[0].transform.position);
        foreach(Collider collider in colliders) {
            float distance = Vector3.Distance(position, collider.transform.position);
            if(distance < bestDistance && collider.transform.parent != transform) {
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

    void OnDrawGizmosSelected() { //Mark the potential Mates in view
        if (drawGizmos) {
            if (closestMate != null) {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(closestMate.position, 0.75f);
            }
        }
    }
}
