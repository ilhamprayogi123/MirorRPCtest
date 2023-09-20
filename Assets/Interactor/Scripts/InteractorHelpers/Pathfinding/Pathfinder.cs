/*Pathfinding algorithm is based on Sebastian Lague's great repo but highly modified.*/
using UnityEngine;
using System.Collections.Generic;
using System;

namespace razz
{
	[HelpURL("https://negengames.com/interactor/components.html#pathfindercs")]
	public class Pathfinder : MonoBehaviour
	{
		private void Awake()
		{
			InteractorAiInput.Init();
		}

		public Vector3[] FindPath(PathGrid grid, InteractorAi interactorAi, Transform targetTransform, Vector3 playerForward, bool forwardStart, int forwardNodes, bool firstRequest)
		{
			grid.UpdateMovingColliders();

			Vector3[] waypoints = new Vector3[0];
			bool pathSuccess = false;

			Node targetNode = grid.NodeFromWorldPoint(targetTransform.position);
			if (targetNode == null) return null;

			Node startNode = grid.NodeFromWorldPoint(interactorAi.transform.position);
			startNode.parent = startNode;

			Node firstTurnNode = null;
			if (forwardStart && firstRequest)
			{
				firstTurnNode = grid.NodeOnRangedDir(interactorAi.transform.position, playerForward, forwardNodes);
				if (firstTurnNode == null && interactorAi.showDebug)
				{
					Debug.Log("InteractorAi forward node area is unwalkable so the path will start without forward.", grid.gameObject);
				}
			}

			List<Node> finalNeigbours = grid.GetNeighboursDirFirst(targetNode, -targetTransform.forward);
			if (finalNeigbours[0] == null) return null;
			bool[] initialStates = new bool[finalNeigbours.Count];
			for (int i = 1; i < finalNeigbours.Count; i++) //0 is directional node
			{
				initialStates[i] = finalNeigbours[i].walkable;
				finalNeigbours[i].walkable = false;
			}

			if (targetNode.walkable)
			{
				Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
				HashSet<Node> closedSet = new HashSet<Node>();
				openSet.Add(startNode);

				while (openSet.Count > 0)
				{
					Node currentNode = openSet.RemoveFirst();
					closedSet.Add(currentNode);

					if (currentNode == targetNode)
					{
						pathSuccess = true;
						break;
					}

					foreach (Node neighbour in grid.GetNeighbours(currentNode))
					{
						if (!neighbour.walkable || closedSet.Contains(neighbour)) continue;

						int directionalNodeCost = 0;
						if (firstTurnNode != null && neighbour == firstTurnNode)
							directionalNodeCost = -250;

						int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenalty + directionalNodeCost;

						if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
						{
							neighbour.gCost = newMovementCostToNeighbour;
							neighbour.hCost = GetDistance(neighbour, targetNode);
							neighbour.parent = currentNode;

							if (!openSet.Contains(neighbour)) openSet.Add(neighbour);
							else openSet.UpdateItem(neighbour);
						}
					}
				}
			}

			if (!startNode.walkable && interactorAi.showDebug)
			{
				Debug.Log(interactorAi.name + " InteractorAi player position is not walkable." , interactorAi);
			}
			if (!targetNode.walkable && interactorAi.showDebug)
			{
				Debug.Log(interactorAi.name + " InteractorAi interaction position is not walkable.", targetTransform);
			}
			else if ((finalNeigbours[0] != null && !finalNeigbours[0].walkable) || finalNeigbours[0] == null)
			{
                if (interactorAi.showDebug) Debug.Log(interactorAi.name + " InteractorAi interaction position is not suitable to reach because of its target direction. Try changing its rotation or position.", targetTransform);
			}

			for (int i = 1; i < finalNeigbours.Count; i++)
				finalNeigbours[i].walkable = initialStates[i];

			if (pathSuccess)
			{
				waypoints = RetracePath(startNode, targetNode, finalNeigbours[0]);

				if (waypoints.Length == 0) return null;
				else
				{
					waypoints[waypoints.Length - 1] = targetTransform.position;
					return waypoints;
				}
			}
			else return null;
		}

		public bool MovingColliderExist(PathGrid grid)
		{
			return grid.movingColliders.Count > 0 ? true : false;
		}

		private Vector3[] RetracePath(Node startNode, Node endNode, Node dirNode)
		{
			List<Node> path = new List<Node>();
			Node currentNode = endNode;

			while (currentNode != startNode)
			{
				path.Add(currentNode);
				currentNode = currentNode.parent;
			}

			if (path.Count == 1)
			{
				if ((startNode.worldPosition - endNode.worldPosition).sqrMagnitude < (startNode.worldPosition - dirNode.worldPosition).sqrMagnitude)
				{
					path.Add(dirNode);
				}
			}

			Vector3[] waypoints;
			waypoints = SetWorldPositions(path);

			Array.Reverse(waypoints);
			return waypoints;
		}

		private Vector3[] SimplifyPath(List<Node> path)
		{
			List<Vector3> waypoints = new List<Vector3>();
			Vector2 directionOld = Vector2.zero;

			for (int i = 1; i < path.Count; i++)
			{
				Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
				if (directionNew != directionOld) waypoints.Add(path[i].worldPosition);

				directionOld = directionNew;
			}
			return waypoints.ToArray();
		}
		private Vector3[] SetWorldPositions(List<Node> path)
		{
			Vector3[] waypoints = new Vector3[path.Count];

			for (int i = 0; i < path.Count; i++)
			{
				waypoints[i] = path[i].worldPosition;
			}
			return waypoints;
		}

		private int GetDistance(Node nodeA, Node nodeB)
		{
			int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
			int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

			if (dstX > dstY) return 14 * dstY + 10 * (dstX - dstY);
			else return 14 * dstX + 10 * (dstY - dstX);
		}
	}
}

