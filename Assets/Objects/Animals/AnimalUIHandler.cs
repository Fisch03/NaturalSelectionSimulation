/**
-------------AnimalUIHandler.cs-------------
 Handles the information that gets 
 displayed above the animal
--------------------------------------------
 **/

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AnimalBehaviour))]
public class AnimalUIHandler : MonoBehaviour {
    Transform canvas;
    Camera mainCamera;

    [System.NonSerialized]
    public GameObject infoText;

    //Various text properties
    public int fontSize;
    public Font font;
    public float xOffset;
    public float yOffset;

    public float cutoffDistance; //The distance at which the Text gets hidden

    AnimalBehaviour behaviour;

    void Start() {
        canvas = transform.parent.GetComponent<AnimalSpawner>().animalCanvas;
        mainCamera = transform.parent.GetComponent<AnimalSpawner>().mainCamera;

        behaviour = transform.GetComponent<AnimalBehaviour>();

        infoText = CreateInfoText();
    }

    void LateUpdate() {
        //Calculate the position of the Point on the Canvas in front of the Animal, apply the offset to it and move the Text accordingly.
        Vector3 canvasPos = mainCamera.WorldToScreenPoint(transform.GetChild(0).transform.position);
        canvasPos.x += xOffset / Vector3.Distance(transform.position, mainCamera.transform.position);
        canvasPos.y += yOffset / Vector3.Distance(transform.position, mainCamera.transform.position);

        if(Vector3.Distance(transform.position, mainCamera.transform.position) > cutoffDistance || canvasPos.z <= 0) {
            infoText.GetComponent<Text>().enabled = false;
        } else {
            infoText.GetComponent<Text>().enabled = true;
        }

        infoText.transform.position = canvasPos;

        string status;
        switch(behaviour.status) {
            case 0:
                status = "Searching food...";
                break;
            case 1:
                status = "Walking towards food...";
                break;
            default:
                status = "Unknown state";
                break;
        }

        //Update the Text
        infoText.GetComponent<Text>().text = "Energy: " + Mathf.Round(behaviour.energy) + "\n" + status;
    }

    GameObject CreateInfoText() { //Create a new infoText GameObject and return it
        GameObject textGameObject = new GameObject("InfoText (Animal" + Mathf.Abs(gameObject.GetInstanceID()) + ")"); //Add the Instance ID to the name so that it is Unique (Numbers are made absolute for Aesthetic Reasons
        textGameObject.transform.SetParent(canvas); //Set the text to be a Child object of the Canvas

        textGameObject.AddComponent<RectTransform>(); //Replace the standart transform with a RectTransform

        //Add the text component and apply all necessary properties
        Text text = textGameObject.AddComponent<Text>();
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = TextAnchor.UpperCenter;

        return textGameObject;
    }

}