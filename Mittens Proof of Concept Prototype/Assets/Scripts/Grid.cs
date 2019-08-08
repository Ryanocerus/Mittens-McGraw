using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Transform startPosition;
    public LayerMask wallMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public float Distance;


    Node[,] grid;
    public List<Node> FinalPath;

    float nodeDiameter;
    int gridSizeX;
    int gridSizeY;

    private void Start()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();
    }

    void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 bottomLeft = transform.position - (Vector3.right * (gridWorldSize.x / 2)) - (Vector3.forward * (gridWorldSize.y / 2));

        for (int y = 0;  y < gridSizeY; y++ )
        {
            for (int x = 0; x < gridSizeY; x++)
            {
                Vector3 worldPoint = bottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool Wall = true;

                if (Physics.CheckSphere(worldPoint, nodeRadius, wallMask))
                {
                    Wall = false;
                }

                grid[x, y] = new Node(Wall, worldPoint, x, y);
            }
        }
    }

    public Node NodeFromWorldPosition(Vector3 a_WorldPosition)
    {
        float xPoint = ((a_WorldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x);
        float yPoint = ((a_WorldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y);

        xPoint = Mathf.Clamp01(xPoint);
        yPoint = Mathf.Clamp01(yPoint);

        int x = Mathf.RoundToInt((gridSizeX - 1) * xPoint);
        int y = Mathf.RoundToInt((gridSizeY - 1) * yPoint);

        return grid[x, y];
    }

    public List<Node> GetNeighboringNodes(Node a_Node)
    {
        List<Node> NeighboringNodes = new List<Node>();
        

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                //if we are on the node tha was passed in, skip this iteration.
                if (x == 0 && y == 0)
                {
                    continue;
                }

                int checkX = a_Node.gridX + x;
                int checkY = a_Node.gridY + y;
                
                //Make sure the node is within the grid.
                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    NeighboringNodes.Add(grid[checkX, checkY]); //Adds to the neighbours list.
                }

            }
        }
        
        return NeighboringNodes;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
        
        if(grid != null)
        {
            foreach (Node node in grid)
            {
                if(node.isWall)
                {
                    Gizmos.color = Color.white;
                }
                else
                {
                    Gizmos.color = Color.yellow;
                }
                if (FinalPath != null)
                {
                    if (FinalPath.Contains(node))
                    {
                        Gizmos.color = Color.red;
                    }
                }

                Gizmos.DrawCube(node.Position, Vector3.one * (nodeDiameter - Distance));
            }
        }
    }

}
