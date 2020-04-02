using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridHelper : MonoBehaviour {
    public static Vector2Int GetRandomPointInRadius(Vector2Int position, int radius) { //Get random 2D point in radius of another
        int xoff = Random.Range(-radius + 1, radius + 1);
        int yoff = Random.Range(-radius + 1, radius + 1);
        Vector2Int point = new Vector2Int(position.x + xoff, position.y + yoff);
        return point;
    }

    public static Vector2Int WorldPosToGridPos(Vector3 pos) { //Calculate the 2D position on the grid from the world position of the animal
        Vector2Int planePos = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
        return planePos;
    }

    public static Vector3 GridPosToWorldPos(Vector2Int pos, float yCoord) { //Calculate the world position of the animal from a 2D point
        Vector3 worldPos = new Vector3(pos.x, yCoord, pos.y);
        return worldPos;
    }

    public static Vector2Int GetMiddlePoint(Vector2Int pos1, Vector2Int pos2) {
        Vector2 middlePoint = (Vector2)(pos1 + pos2) / 2f;
        return Vector2Int.RoundToInt(middlePoint);
    }
}
