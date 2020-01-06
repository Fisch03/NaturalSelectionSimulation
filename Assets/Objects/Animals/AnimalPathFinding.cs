/**
------------AnimalPathFinding.cs------------
 Handles all the pathfinding for the animal 
--------------------------------------------
 **/

using System.Collections;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Seeker), typeof(Animation))]
public class AnimalPathFinding : MonoBehaviour {
	public float nextWaypointDistance;

	Path path; //Saves the path once it has been calculated

    //Delegates for the Methods for each Step and completing the Path
    public delegate void StepDel(); 
    StepDel onStepCallback;
    public delegate void FinishedDel();
    FinishedDel onFinishCallback;
    bool alreadyCalledBack;

    float speed; //Saves the speed of the animation, 1.0 is 1 Jump per second, 2.0 is 2 Jumps etc.

    int currentWaypoint = 0; //The Waypoint the Animal is currently moving towards
    bool reachedEnd;
    bool jumping = false;

    //Calculate the path to a point and move along it (accepts either a transform or a point as parameter
    public void GoPath(Transform target, float jumpSpeed, StepDel stepCallback, FinishedDel finishedCallback) {
        speed = jumpSpeed;
        onStepCallback = stepCallback;
        onFinishCallback = finishedCallback;
        alreadyCalledBack = false;
        GetComponent<Seeker>().StartPath(transform.position, target.position, OnPathComplete); //Start calculation
    }
    public void GoPath(Vector3 target, float jumpSpeed, StepDel stepCallback, FinishedDel finishedCallback) {
        speed = jumpSpeed;
        onStepCallback = stepCallback;
        onFinishCallback = finishedCallback;
        alreadyCalledBack = false;
        GetComponent<Seeker>().StartPath(transform.position, target, OnPathComplete); //Start calculation
    }

    public void ClearPath() {
        path = null;
    }

    void OnPathComplete(Path p) { //Gets called when the Path is finished calculating
	   if(p.error) {
			Debug.LogError(p.error);
		} else {
			path = p;
			currentWaypoint = 0;
		}
	}

    IEnumerator Jump() { //Coroutine for playing the Jump animation
        jumping = true;
        Animation anim = transform.GetChild(0).GetComponent<Animation>();
        anim[anim.clip.name].speed = speed;
        anim.Play();
    
        while (anim.isPlaying) {
            yield return null;
        }

        transform.GetChild(0).transform.localPosition = Vector3.zero;
        transform.position = transform.position + transform.forward;

        jumping = false;
    }

	void Update() {
		if(path != null) { //Check if a path has been calculated
			reachedEnd = false;

			float waypointDistance;

			while (!reachedEnd) { //While moving along the path, check if the Animal has arrived at the Waypoint and if so, advance to the next one
				waypointDistance = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
				if(waypointDistance < nextWaypointDistance) {
					if(currentWaypoint+1< path.vectorPath.Count) {
                        currentWaypoint++;
					} else {
					    reachedEnd = true;
					}
				} else {
                    break;
                }
			}

            if (!jumping && !reachedEnd) { //While moving along, if the Animal is not currently jumping, start a Jump towards the next waypoint
                Vector3 targetDirection = new Vector3(path.vectorPath[currentWaypoint].x, transform.position.y, path.vectorPath[currentWaypoint].z) - transform.position;
                Vector3 rot = Quaternion.LookRotation(targetDirection).eulerAngles;
                rot.y = ClampRotation(rot.y);
                transform.localEulerAngles = rot;
                StartCoroutine(Jump());
                onStepCallback();
            } else if(reachedEnd && !alreadyCalledBack) { //Call back the specified method on finish
                alreadyCalledBack = true;
                onFinishCallback();
            }
        }
	}

    float ClampRotation(float rot) { //Clamp a rotation to 0/90/180/270 degrees
        float newrot;
        newrot = Mathf.Round(rot / 90);
        newrot *= 90;
        return newrot;
    }
}
