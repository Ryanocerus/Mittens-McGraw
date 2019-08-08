using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public int gridX;
    public int gridY;

    public bool isWall;

    public Vector3 Position;

    public Node parent;
    public int gCost;
    public int hCost;
    public int fCost {  get { return gCost + hCost; } }

    public Node (bool a_IsWall, Vector3 a_Pos, int a_GridX, int a_GridY)
    {
        isWall = a_IsWall;
        Position = a_Pos;
        gridX = a_GridX;
        gridY = a_GridY;
    }
   
}
