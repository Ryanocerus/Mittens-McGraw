using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    Grid grid;
    public Transform StartPosition;
    public Transform TargetPosition;
    public LayerMask hitLayers;
    public GameObject player;
    public bool isMoving;
    public List<Node> comparePaths;
    public bool newPath;
    public float speed;
    public bool hasFinished = false;
    public Transform npcOriginalPosition;
 

    private void Awake()
    {
        StartPosition = player.transform;
        grid = GetComponent<Grid>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Creates new path
            Vector3 mouse = Input.mousePosition;
            Ray castPoint = Camera.main.ScreenPointToRay(mouse);
            RaycastHit hit;



            if (Physics.Raycast(castPoint, out hit, Mathf.Infinity))
            {
                if (hit.collider.tag == "NPC")
                {
                    npcOriginalPosition = hit.transform;
                    hit.transform.LookAt(player.transform);
                    //set the target position to be infront of the npc;
                    TargetPosition.transform.position = hit.transform.position + hit.transform.forward;
                    hit.transform.rotation = npcOriginalPosition.rotation;
                    hit.transform.LookAt(npcOriginalPosition);
                    Debug.Log("BOMB HAS BEEN PLANTED");
                }
                else if (hit.collider.tag == "GROUND")
                {
                    TargetPosition.transform.position = hit.point;
                    
                }
            }
            FindPath(StartPosition.position, TargetPosition.position);

            // compare new path with old path

            if (grid.FinalPath == comparePaths)
            {
                newPath = false;
            }
            else if (grid.FinalPath != comparePaths)
            {

                newPath = true;
                comparePaths = grid.FinalPath;
            }

            //If the player is clicking outside the playable area, continue on current path
            //If player selects a new path, stop moving, go to new target
            //
            if (!isMoving)
            {
                if (hasFinished && !newPath)
                {
                    return;
                }
                else
                {
                    StartCoroutine("movePlayer");
                    movePlayer();
                }
            }

            if (isMoving && !newPath)
            {
                return;
            }
            if (isMoving && newPath)
            {
                StopAllCoroutines();
                StartPosition = player.transform;
                StartCoroutine("movePlayer");
                movePlayer();
            }


            

        }
    }

    IEnumerator movePlayer()
    {

        isMoving = true;
        foreach (Node node in grid.FinalPath)
        {

            player.transform.position = node.Position;
            yield return new WaitForSeconds(speed);

        }
        isMoving = false;
        hasFinished = true;
        StartPosition = player.transform;
    }

    void FindPath(Vector3 a_startPos, Vector3 a_targetPos)
    {
        Node StartNode = grid.NodeFromWorldPosition(a_startPos);
        Node TargetNode = grid.NodeFromWorldPosition(a_targetPos);

        List<Node> OpenList = new List<Node>();
        HashSet<Node> ClosedList = new HashSet<Node>();

        OpenList.Add(StartNode);

        while (OpenList.Count > 0)
        {
            Node currentNode = OpenList[0];
            for (int i = 1; i < OpenList.Count;i++)
            {
                if (OpenList[i].fCost < currentNode.fCost || OpenList[i].fCost == currentNode.fCost && OpenList[i].hCost < currentNode.hCost)
                {
                    currentNode = OpenList[i];
                }
            }

            OpenList.Remove(currentNode);
            ClosedList.Add(currentNode);

            if(currentNode == TargetNode)
            {
                GetFinalPath(StartNode, TargetNode);
                break;
            }

            foreach (Node NeighborNode in grid.GetNeighboringNodes(currentNode))
            {
                if (!NeighborNode.isWall || ClosedList.Contains(NeighborNode))
                {
                    continue;
                }

                int MoveCost = currentNode.gCost + GetManhattenDistance(currentNode, NeighborNode);

                if (MoveCost < NeighborNode.fCost || !OpenList.Contains(NeighborNode))
                {
                    NeighborNode.gCost = MoveCost;
                    NeighborNode.hCost = GetManhattenDistance(NeighborNode, TargetNode);
                    NeighborNode.parent = currentNode;

                    if (!OpenList.Contains(NeighborNode))
                    {
                        OpenList.Add(NeighborNode);
                    }
                }
            }

        }
    }

    void GetFinalPath(Node a_startingNode, Node a_endNode)
    {
        List<Node> FinalPath = new List<Node>();
        Node currentNode = a_endNode;

        while (currentNode != a_startingNode)
        {
            FinalPath.Add(currentNode);
            currentNode = currentNode.parent;
        }

        FinalPath.Reverse();
        grid.FinalPath = FinalPath;
    }

    int GetManhattenDistance(Node a_nodeA, Node a_nodeB)
    {
        int ix = Mathf.Abs(a_nodeA.gridX - a_nodeB.gridX);
        int iy = Mathf.Abs(a_nodeA.gridY - a_nodeB.gridY);

        return ix + iy;
    }
}
