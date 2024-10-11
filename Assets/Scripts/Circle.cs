using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle : MonoBehaviour
{
    public enum CircleType { player1, player2 };
    public CircleType circleType;
    public Material[] circleColors;

    public void SetCircleType(CircleType type)
    {
        MeshRenderer meshColor = GetComponentsInChildren<MeshRenderer>()[0];
        meshColor.material = circleColors[(int)type];
        circleType = type;
    }
}
