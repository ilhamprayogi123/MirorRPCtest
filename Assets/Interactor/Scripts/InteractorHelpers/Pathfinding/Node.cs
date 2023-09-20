using UnityEngine;

namespace razz
{
	public class Node : IHeapItem<Node>
	{
		public bool walkable;
		public Vector3 worldPosition;
		public int gridX;
		public int gridY;
		public int movementPenalty;

		public int gCost;
		public int hCost;
		public Node parent;

		private int _heapIndex;

		public Node(bool walkable, Vector3 worldPos, int gridX, int gridY, int penalty)
		{
			this.walkable = walkable;
			this.worldPosition = worldPos;
			this.gridX = gridX;
			this.gridY = gridY;
			this.movementPenalty = penalty;
		}

		public int FCost
		{
			get { return gCost + hCost; }
		}

		public int HeapIndex
		{
			get { return _heapIndex; }
			set { _heapIndex = value; }
		}

		public int CompareTo(Node nodeToCompare)
		{
			int compare = FCost.CompareTo(nodeToCompare.FCost);
			if (compare == 0) compare = hCost.CompareTo(nodeToCompare.hCost);
			return -compare;
		}
	}
}
