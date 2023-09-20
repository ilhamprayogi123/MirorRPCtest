using UnityEngine;
using System.Collections.Generic;

namespace razz
{
	[HelpURL("https://negengames.com/interactor/components.html#pathgridcs")]
	[RequireComponent(typeof(BoxCollider))]
	public class PathGrid : MonoBehaviour
	{
        #region Variables
        [Tooltip("Lazy initiation allows grids to be created when an InteractorAi enters its trigger, rather than all at once during the Start phase.")]
		public bool lazyInit = true;
		[Tooltip("Specify the player layer for the grid trigger to limit the checks to only players.")]
		public LayerMask interactorLayer;
		[Tooltip("Exclude specified layers, including the player and floor layers, when checking the grid for unwalkable areas.")]
		public LayerMask ignoreMask;
		[Tooltip("Determine the grid size by visually inspecting the area in the SceneView that needs to be covered.")]
		public Vector2 gridWorldSize;
		[Tooltip("InteractorObjects added to the grid are checked if their aiTargets are within the grid bounds. Because if they are not in bounds, they can't be pathfinded. This value expands the those limits (not the trigger) and pathfinding will end at the edges if they're at out of bounds.")]
		public float gridBoundExcess = 2f;
		[Tooltip("Increase the height of the grid trigger to account for sloped terrain or floors, providing better coverage.")]
		public float extraTriggerHeight = 1f;
		[Tooltip("Smaller nodes offer higher resolution mapping accuracy, but they also increase the time required for both grid creation and the pathfinding process. You can enable debug setting below to see how many nodes created on logs.")]
		public float nodeRadius = 0.1f;
		[Tooltip("The capsule, with playerRadius and playerHeight, checks each node, marking it as unwalkable if it intersects with a collider. Increasing the playerRadius expands the unwalkable areas around obstacles.")]
		public float playerRadius = 0.3f;
		[Tooltip("The capsule checks for obstacles on the grid height only. If you have sloped ground, using smaller values is recommended (or greater playerStepHeight) since it won't detect changes in the ground height.")]
		public float playerHeight = 1.6f;
		[Tooltip("Specify the maximum step height that your controller can navigate.")]
		public float playerStepHeight = 0.02f;
		[Tooltip("PathGrid applies proximity penalties on nodes around unwalkable areas, enabling the pathfinder to prioritize paths with lower penalties and keep the player away from obstacles. However, the path can still be set on these grids if it results in a shorter path. Increasing the penalty makes these areas more undesirable. Enable Debug Detailed setting to visualize these penalties in the SceneView.")]
		public int proximityPenalty = 50;
		[Tooltip("Expands proximity area around the unwalkable nodes.")]
		public int proximityCount = 1;
		[Tooltip("PathGrid sends these interactions to InteractorAIs that enter the trigger. You can dynamically add more objects during runtime, see example scripts or docs.")]
		public List<InteractorObject> intObjs = new List<InteractorObject>();
		[Header("Dynamic Grid Settings")]
		[Tooltip("If you have moving obstacles, you can add them to list so their nodes will be updated if they move in the pathfinding process. Also they will be updated a few more seconds when pathfinding is done. You can add or remove objects with scripts or events in runtime.")]
		public List<Collider> movingColliders;
		[Tooltip("To update the PathGrid for moving objects added to the list, only the However, their areas can be bigger than their colliders, based on the playerRadius or nodeRadius. This value increase the area to be updated.")]
		public int movingExcessSize = 2;
		[Header("Debug Settings")]
		[Tooltip("Simplified debug mode shows only unwalkable areas, offering improved performance. Also enables grid details with logs and shows interaction targets on this grid.")]
		public bool debugSimple;
		[Tooltip("More detailed debug, shows proximity areas as well. Also enables grid details with logs and shows interaction targets on this grid.")]
		public bool debugDetailed;

		private List<Vector4> _bounds; //(bounds.min.gridX, bounds.min.gridY, bounds.max.gridX, bounds.max.gridY)
		private Vector3 _minHeightOffset, _maxHeightOffset;
		private bool _initiated = false;

		private Node[,] _grid;
		private List<InteractorAi> _interactorAis;

		private float _nodeDiameter;
		private int _gridSizeX, _gridSizeY;

		private int _penaltyMin = int.MaxValue;
		private int _penaltyMax = int.MinValue;
		private BoxCollider _box;
		private Vector3 _triggerMin, _triggerMax;
        
        public int MaxSize
		{
			get { return _gridSizeX * _gridSizeY; }
		}
		#endregion

		private void Awake()
		{
			this.gameObject.isStatic = true;
			_box = GetComponent<BoxCollider>();
			_box.isTrigger = true;
			_box.size = new Vector3(Mathf.RoundToInt(gridWorldSize.x), playerHeight + extraTriggerHeight, Mathf.RoundToInt(gridWorldSize.y));
			_box.center = new Vector3(0, _box.size.y * 0.5f, 0);
			_triggerMin = transform.position + _box.center - _box.size * 0.5f - new Vector3(gridBoundExcess, gridBoundExcess, gridBoundExcess);
			_triggerMax = transform.position + _box.center + _box.size * 0.5f + new Vector3(gridBoundExcess, gridBoundExcess, gridBoundExcess);

			if (!lazyInit) Init();
		}
        private void Start()
        {
			_interactorAis = new List<InteractorAi>();
			ClearNullDuplicatesIntObjs();
			if (TriggerZoneCheck()) Init();
		}
		[ContextMenu("Reinitialize Grid")]
		public void Reinit()
		{//For recreating grid with changed settings in inspector right click menu
			_initiated = false;
			Init();
		}
		private void Init()
		{
			if (_initiated) return;

			nodeRadius = Mathf.Max(nodeRadius, 0.01f);
			_nodeDiameter = nodeRadius * 2f;
			playerStepHeight = Mathf.Max(0, playerStepHeight);
			if (movingExcessSize < 1) movingExcessSize = Mathf.Max(1, Mathf.RoundToInt(playerRadius * 2f / _nodeDiameter));
			_gridSizeX = Mathf.RoundToInt(gridWorldSize.x / _nodeDiameter);
			_gridSizeY = Mathf.RoundToInt(gridWorldSize.y / _nodeDiameter);

			CreateGrid();
			CacheColliders();
			BlurPenaltyMap(proximityCount);
			_initiated = true;
		}
		private void OnTriggerEnter(Collider other)
        {
			int otherLayer = other.gameObject.layer;
			if (((1 << otherLayer) & interactorLayer.value) != 0)
            {
				InteractorAi interactorAi;
				if (interactorAi = other.GetComponent<InteractorAi>())
                {
					Init();
					AddInteractorAi(interactorAi);
				}
			}
        }
        private void OnTriggerExit(Collider other)
        {
			int otherLayer = other.gameObject.layer;
			if (((1 << otherLayer) & interactorLayer.value) != 0)
			{
				InteractorAi interactorAi;
				if (interactorAi = other.GetComponent<InteractorAi>())
                {
					RemoveInteractorAi(interactorAi);
				}
			}
		}

        #region Public Access Methods
        public void AddInteractionManual(InteractorObject interactorObject)
        {//Adds a new InteractorObject to this grid
            if (interactorObject && !intObjs.Contains(interactorObject))
            {
				if (!CheckInteractionForGridBounds(interactorObject)) return;

				intObjs.Add(interactorObject);
				for (int i = 0; i < _interactorAis.Count; i++)
					_interactorAis[i].AddInteraction(interactorObject);
			}
        }
		public void RemoveInteractionManual(InteractorObject interactorObject)
		{//Removes an InteractorObject from this grdi
            if (interactorObject && intObjs.Contains(interactorObject))
            {
				for (int i = 0; i < _interactorAis.Count; i++)
					_interactorAis[i].RemoveInteraction(interactorObject);
				intObjs.Remove(interactorObject);
            }
		}

		public void AddCollider(Collider col)
		{//Adds a new moving collider to this grid
			RemoveNullColliders();
			if (movingColliders.Contains(col)) return;

			movingColliders.Add(col);
			int addedCol = movingColliders.Count - 1;

			Vector4 tempVector4;

			Node tempNode = NodeFromWorldPoint(movingColliders[addedCol].bounds.min);
			tempVector4.x = tempNode.gridX;
			tempVector4.y = tempNode.gridY;

			tempNode = NodeFromWorldPoint(movingColliders[addedCol].bounds.max);
			tempVector4.z = tempNode.gridX;
			tempVector4.w = tempNode.gridY;
			_bounds.Add(tempVector4);
			UpdateGrid(tempVector4);
		}
		public void RemoveCollider(Collider col)
		{//Removes a moving collider from this grid
			RemoveNullColliders();
			if (col == null) return;

			int index = movingColliders.IndexOf(col);

			if (index >= 0)
			{
				movingColliders.RemoveAt(index);
				RemovalUpdate(index);
			}
		}
		public bool CheckInteraction(InteractorObject intObj)
		{//Checks if this has InteractorObject
			if (intObjs.Contains(intObj)) return true;
			else return false;
		}
		#endregion

		private bool TriggerZoneCheck()
        {//Checks if any InteractorAi on grid at start
			bool interactorAiExist = false;
			Collider[] colliders = Physics.OverlapBox(transform.position, _box.size * 0.5f, Quaternion.identity, interactorLayer, QueryTriggerInteraction.Ignore);
            foreach (Collider col in colliders)
            {
				int otherLayer = col.gameObject.layer;
				if (((1 << otherLayer) & interactorLayer.value) != 0) continue;

				InteractorAi interactorAi;
                if ((interactorAi = col.GetComponent<InteractorAi>()) && !_interactorAis.Contains(interactorAi))
                {
					AddInteractorAi(interactorAi);
					interactorAiExist = true;
				}
			}
			return interactorAiExist;
		}

		private void AddInteractorAi(InteractorAi interactorAi)
        {
            if (interactorAi && !_interactorAis.Contains(interactorAi))
            {
				_interactorAis.Add(interactorAi);
				interactorAi.AddInteractions(intObjs, this);
			}
            else if (!interactorAi) _interactorAis.Remove(interactorAi);
        }

		private void RemoveInteractorAi(InteractorAi interactorAi)
		{
			if (_interactorAis.Count == 0) return;

			if (interactorAi && _interactorAis.Contains(interactorAi))
            {
				interactorAi.RemoveInteractions(intObjs, this);
				_interactorAis.Remove(interactorAi);
			}
		}

		private void ClearNullDuplicatesIntObjs()
        {
			HashSet<InteractorObject> uniqueIntObjs = new HashSet<InteractorObject>();
			List<InteractorObject> duplicatesAndNulls = new List<InteractorObject>();
			for (int i = 0; i < intObjs.Count; i++)
			{
				if (intObjs[i] != null && !uniqueIntObjs.Contains(intObjs[i]))
				{
                    if (CheckInteractionForGridBounds(intObjs[i]))
						uniqueIntObjs.Add(intObjs[i]);
				}
			}
			intObjs.Clear();
			intObjs.AddRange(uniqueIntObjs);
		}

		private bool CheckInteractionForGridBounds(InteractorObject intObj)
        {
			if (!intObj) return false;
            if (!intObj.aiTarget)
            {
				Debug.Log(intObj.gameObject.name + " InteractorObject don't have aiTarget for " + this.name + " PathGrid.", intObj);
				return false;
            }

			Vector3 position = intObj.aiTarget.transform.position;
			if (position.x >= _triggerMin.x && position.x <= _triggerMax.x && position.z >= _triggerMin.z && position.z <= _triggerMax.z) return true;
            else
            {
				Debug.Log("One of InteractorObject aiTargets of this PathGrid(" + this.name + ") is out of the grids bounds. Place the InteractorObject within the grids area.", intObj);
				return false;
			}
        }

		public void UpdateMovingColliders()
        {
			bool updateBlur = false;
			for (int i = 0; i < movingColliders.Count; i++)
			{
				if (movingColliders[i] == null)
				{
					RemoveNullColliders();
					return;
				}

				if (movingColliders[i].transform.hasChanged)
				{
					UpdateGrid(_bounds[i]);
					UpdateBounds(i);
					UpdateGrid(_bounds[i]);
					movingColliders[i].transform.hasChanged = false;
					updateBlur = true;
				}
			}
            if (updateBlur)
            {
                ClearPenalties();
                BlurPenaltyMap(proximityCount);
            }
		}

		private void CacheColliders()
        {
            if (movingColliders == null) movingColliders = new List<Collider>();
			_bounds = new List<Vector4>();
			if (movingColliders.Count == 0) return;

			RemoveNullColliders();
			Vector4 tempVector4;
            for (int i = 0; i < movingColliders.Count; i++)
            {
				Node tempNode = NodeFromWorldPoint(movingColliders[i].bounds.min);
				tempVector4.x = tempNode.gridX;
				tempVector4.y = tempNode.gridY;

				tempNode = NodeFromWorldPoint(movingColliders[i].bounds.max);
				tempVector4.z = tempNode.gridX;
				tempVector4.w = tempNode.gridY;
				_bounds.Add(tempVector4);
				movingColliders[i].transform.hasChanged = false;
			}
        }

		private void UpdateBounds(int index)
        {
			Vector4 tempVector4;
			Node tempNode = NodeFromWorldPoint(movingColliders[index].bounds.min);
			tempVector4.x = tempNode.gridX;
			tempVector4.y = tempNode.gridY;

			tempNode = NodeFromWorldPoint(movingColliders[index].bounds.max);
			tempVector4.z = tempNode.gridX;
			tempVector4.w = tempNode.gridY;
			_bounds[index] = tempVector4;
		}

		private void RemoveNullColliders()
        {
			for (int i = 0; i < movingColliders.Count; i++)
			{
				if (movingColliders[i] == null)
                {
					movingColliders.RemoveAt(i);
					RemovalUpdate(i);
					i--;
				}
			}
		}

		private void RemovalUpdate(int index)
        {
			if (_bounds.Count > index)
			{
				Vector4 removedBounds = _bounds[index];
				_bounds.RemoveAt(index);
				UpdateGrid(removedBounds);
			}
		}

        private void CreateGrid()
		{
			_grid = new Node[_gridSizeX, _gridSizeY];
			Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
			_minHeightOffset = new Vector3(0, playerRadius + playerStepHeight, 0);
			_maxHeightOffset = new Vector3(0, playerHeight - playerRadius + playerStepHeight, 0);

			for (int x = 0; x < _gridSizeX; x++)
			{
				for (int y = 0; y < _gridSizeY; y++)
				{
					Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiameter + nodeRadius) + Vector3.forward * (y * _nodeDiameter + nodeRadius);
					bool walkable = CollisionCheck(worldPoint);

					int movementPenalty = 0;
					if (!walkable) movementPenalty += proximityPenalty;

					_grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
				}
			}
            if (debugDetailed || debugSimple)
            {
				Debug.Log("PathGrid created with " + MaxSize + " nodes on " + this.name, this);
            }
		}
		private void UpdateGrid(Vector4 bounds)
		{
			Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;
			_minHeightOffset = new Vector3(0, playerRadius - playerStepHeight, 0);
			_maxHeightOffset = new Vector3(0, playerHeight - playerRadius + playerStepHeight, 0);
			int minX = Mathf.Max(0, (Mathf.RoundToInt(bounds.x) - movingExcessSize));
			int minY = Mathf.Max(0, (Mathf.RoundToInt(bounds.y) - movingExcessSize));
			int maxX = Mathf.Min(_gridSizeX, (Mathf.RoundToInt(bounds.z) + movingExcessSize + 1));
			int maxY = Mathf.Min(_gridSizeY, (Mathf.RoundToInt(bounds.w) + movingExcessSize + 1));

			for (int x = minX; x < maxX; x++)
			{
				for (int y = minY; y < maxY; y++)
				{
					Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * _nodeDiameter + nodeRadius) + Vector3.forward * (y * _nodeDiameter + nodeRadius);

					bool walkable = CollisionCheck(worldPoint);

					int movementPenalty = 0;
					if (!walkable) movementPenalty += proximityPenalty;

					_grid[x, y] = new Node(walkable, worldPoint, x, y, movementPenalty);
				}
			}
		}

		private bool CollisionCheck(Vector3 worldPoint)
        {
			/*return !(Physics.CheckSphere(worldPoint, playerRadius, ~ignoreMask, QueryTriggerInteraction.Ignore));*/
			return !(Physics.CheckCapsule(worldPoint + _minHeightOffset, worldPoint + _maxHeightOffset, playerRadius, ~ignoreMask, QueryTriggerInteraction.Ignore));
        }

		private void BlurPenaltyMap(int blurSize)
		{
			int kernelSize = blurSize * 2 + 1;
			int kernelExtents = blurSize;

			int[,] penaltiesHorizontalPass = new int[_gridSizeX, _gridSizeY];
			int[,] penaltiesVerticalPass = new int[_gridSizeX, _gridSizeY];

			for (int y = 0; y < _gridSizeY; y++)
			{
				for (int x = -kernelExtents; x <= kernelExtents; x++)
				{
					int sampleX = Mathf.Clamp(x, 0, kernelExtents);
					penaltiesHorizontalPass[0, y] += _grid[sampleX, y].movementPenalty;
				}

				for (int x = 1; x < _gridSizeX; x++)
				{
					int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, _gridSizeX);
					int addIndex = Mathf.Clamp(x + kernelExtents, 0, _gridSizeX - 1);

					penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - _grid[removeIndex, y].movementPenalty + _grid[addIndex, y].movementPenalty;
				}
			}

			for (int x = 0; x < _gridSizeX; x++)
			{
				for (int y = -kernelExtents; y <= kernelExtents; y++)
				{
					int sampleY = Mathf.Clamp(y, 0, kernelExtents);
					penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
				}

				int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
				_grid[x, 0].movementPenalty = blurredPenalty;

				for (int y = 1; y < _gridSizeY; y++)
				{
					int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, _gridSizeY);
					int addIndex = Mathf.Clamp(y + kernelExtents, 0, _gridSizeY - 1);

					penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
					blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
					_grid[x, y].movementPenalty = blurredPenalty;

					if (blurredPenalty > _penaltyMax)
					{
						_penaltyMax = blurredPenalty;
					}
					if (blurredPenalty < _penaltyMin)
					{
						_penaltyMin = blurredPenalty;
					}
				}
			}
		}
		private void ClearPenalties()
		{
			for (int x = 0; x < _gridSizeX; x++)
			{
				for (int y = 0; y < _gridSizeY; y++)
				{
					if (_grid[x, y].walkable) _grid[x, y].movementPenalty = 0;
                    else _grid[x, y].movementPenalty = proximityPenalty;
				}
			}
		}

		public List<Node> GetNeighbours(Node node)
		{
			List<Node> neighbours = new List<Node>();

			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					if (x == 0 && y == 0)
						continue;

					int checkX = node.gridX + x;
					int checkY = node.gridY + y;

					if (checkX >= 0 && checkX < _gridSizeX && checkY >= 0 && checkY < _gridSizeY)
						neighbours.Add(_grid[checkX, checkY]);
				}
			}
			return neighbours;
		}

		public Node NodeOnRangedDir(Vector3 startPos, Vector3 forward, int nodeRange)
		{
			Vector3 turnAreaCenter = startPos + forward * nodeRadius * nodeRange;
            if (debugSimple || debugDetailed) Debug.DrawLine(startPos, turnAreaCenter, Color.red, 3f);
			Node turnNode = NodeFromWorldPoint(turnAreaCenter);
			if (!turnNode.walkable)
			{
				List<Node> neighbours = GetNeighbours(turnNode);
				turnNode = null;
				for (int i = 0; i < neighbours.Count; i++)
					if (neighbours[i].walkable) return neighbours[i];
			}
			return turnNode;
		}

		public List<Node> GetNeighboursDirFirst(Node node, Vector3 direction)
        {
			List<Node> neighbours = new List<Node>();

			direction.x = Mathf.RoundToInt(direction.x);
			direction.z = Mathf.RoundToInt(direction.z);
			Node nodeOnDir = null;

			for (int x = -1; x <= 1; x++)
			{
				for (int y = -1; y <= 1; y++)
				{
					if (x == 0 && y == 0)
						continue;

					int checkX = node.gridX + x;
					int checkY = node.gridY + y;
					if (direction.x != x || direction.z != y)
					{
						if (checkX >= 0 && checkX < _gridSizeX && checkY >= 0 && checkY < _gridSizeY)
							neighbours.Add(_grid[checkX, checkY]);
					}
                    else
                    {
						if (checkX >= _gridSizeX || checkY >= _gridSizeY || checkX < 0 || checkY < 0) nodeOnDir = null;
						else nodeOnDir = _grid[checkX, checkY];
					}
				}
			}
			neighbours.Insert(0, nodeOnDir);
			return neighbours;
		}

		public Node NodeFromWorldPoint(Vector3 worldPosition)
		{
			float percentX = (worldPosition.x - transform.position.x) / gridWorldSize.x + 0.5f;
			float percentY = (worldPosition.z - transform.position.z) / gridWorldSize.y + 0.5f;
			percentX = Mathf.Clamp01(percentX);
			percentY = Mathf.Clamp01(percentY);

			int x = Mathf.RoundToInt((_gridSizeX - 1) * percentX);
			int y = Mathf.RoundToInt((_gridSizeY - 1) * percentY);
			return _grid[x, y];
		}

        #region To Update On Inspector
#if UNITY_EDITOR
        List<Collider> _inspectorColliders;
		List<InteractorObject> _inspectorIntObjs;
		bool _checkOnce;
		private void OnValidate()
		{
			if (UnityEditor.EditorApplication.isPlaying)
			{
				if (!_checkOnce)
				{
					_inspectorColliders = new List<Collider>(movingColliders);
					_inspectorIntObjs = new List<InteractorObject>(intObjs);
					_checkOnce = true;
					return;
				}

				if (!System.Linq.Enumerable.SequenceEqual(_inspectorColliders, movingColliders))
				{
					CreateGrid();
					CacheColliders();
					BlurPenaltyMap(proximityCount);
					_inspectorColliders = new List<Collider>(movingColliders);
				}

				HashSet<InteractorObject> uniqueIntObjs = new HashSet<InteractorObject>();
				List<InteractorObject> duplicatesAndNulls = new List<InteractorObject>();
				for (int i = 0; i < intObjs.Count; i++)
				{
					if (!CheckInteractionForGridBounds(intObjs[i]))
					{
						intObjs.RemoveAt(i);
						i--;
						continue;
					}

					if (!intObjs[i] || !uniqueIntObjs.Add(intObjs[i]))
					{
						duplicatesAndNulls.Add(intObjs[i]);
					}
				}
				for (int i = 0; i < duplicatesAndNulls.Count; i++)
				{
					for (int j = 0; j < _interactorAis.Count; j++)
						_interactorAis[j].RemoveInteraction(duplicatesAndNulls[i]);
					_inspectorIntObjs.Remove(duplicatesAndNulls[i]);
					intObjs.Remove(duplicatesAndNulls[i]);
				}

				if (_inspectorIntObjs.Count > intObjs.Count)
				{
					for (int i = 0; i < _inspectorIntObjs.Count; i++)
					{
						if (!intObjs.Contains(_inspectorIntObjs[i]))
						{
							for (int j = 0; j < _interactorAis.Count; j++)
								_interactorAis[j].RemoveInteraction(_inspectorIntObjs[i]);
							_inspectorIntObjs.Remove(_inspectorIntObjs[i]);
							i--;
						}
					}
				}
				else if (_inspectorIntObjs.Count < intObjs.Count)
				{
					for (int i = 0; i < intObjs.Count; i++)
					{
						if (!_inspectorIntObjs.Contains(intObjs[i]))
						{
							for (int j = 0; j < _interactorAis.Count; j++)
								_interactorAis[j].AddInteraction(intObjs[i]);
							_inspectorIntObjs.Add(intObjs[i]);
						}
					}
				}
				else
				{
					for (int i = 0; i < _inspectorIntObjs.Count; i++)
					{
						if (_inspectorIntObjs[i] != intObjs[i])
						{
							for (int j = 0; j < _interactorAis.Count; j++)
							{
								_interactorAis[j].RemoveInteraction(_inspectorIntObjs[i]);
								_interactorAis[j].AddInteraction(intObjs[i]);
							}
							_inspectorIntObjs[i] = intObjs[i];
						}
					}
				}
			}
		}
#endif
        #endregion

        #region Gizmos
        private void OnDrawGizmos()
		{
			Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));

			if (_grid == null) return;

            if (debugDetailed)
            {
				foreach (Node n in _grid)
				{
					if (n.walkable && n.movementPenalty == 0) continue;

					float t = Mathf.InverseLerp(_penaltyMin, _penaltyMax, n.movementPenalty);
					Color enhancedColor = Color.Lerp(Color.black * 0.2f, Color.white * 3f, t);
					Gizmos.color = enhancedColor;
                    Gizmos.color = (n.walkable) ? Gizmos.color : Color.red * 0.8f;
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (_nodeDiameter));
				}
			}

            if (debugSimple)
            {
				float _nodeRadius = _nodeDiameter * 0.5f;
				Vector3 transformCenter = transform.position - new Vector3(_gridSizeX * nodeRadius, transform.position.y, _gridSizeY * nodeRadius);

				Color reddish = new Color(0.8f, 0, 0, 0.7f);
				Color whiteish = new Color(1f, 1f, 1f, 0.5f);

				for (int x = 0; x < _gridSizeX; x++)
				{
					for (int y = 0; y < _gridSizeY; y++)
					{
						Node node = _grid[x, y];
						if (node.walkable)
						{
							bool isEdgeNode = IsEdgeNode(x, y);
							Gizmos.color = isEdgeNode ? whiteish : reddish;

							Vector3 center = new Vector3(x * _nodeDiameter + _nodeRadius, 0, y * _nodeDiameter + _nodeRadius) + transformCenter;

							// Top edge
							if (IsUnwalkable(x, y + 1))
								DrawDottedLine(center + new Vector3(-_nodeRadius, 0, _nodeRadius), center + new Vector3(_nodeRadius, 0, _nodeRadius));

							// Bottom edge
							if (IsUnwalkable(x, y - 1))
								DrawDottedLine(center + new Vector3(-_nodeRadius, 0, -_nodeRadius), center + new Vector3(_nodeRadius, 0, -_nodeRadius));

							// Left edge
							if (IsUnwalkable(x - 1, y))
								DrawDottedLine(center + new Vector3(-_nodeRadius, 0, -_nodeRadius), center + new Vector3(-_nodeRadius, 0, _nodeRadius));

							// Right edge
							if (IsUnwalkable(x + 1, y))
								DrawDottedLine(center + new Vector3(_nodeRadius, 0, -_nodeRadius), center + new Vector3(_nodeRadius, 0, _nodeRadius));
						}
					}
				}
			}

			if (!debugSimple && !debugDetailed) return;

			Color greenish = new Color(0, 0.8f, 0, 0.5f);
			Gizmos.color = greenish;
            for (int i = 0; i < intObjs.Count; i++)
            {
				if (!intObjs[i] || !intObjs[i].aiTarget) continue;

				Vector3 tipPosition = intObjs[i].aiTarget.position + intObjs[i].aiTarget.forward * 0.5f;
				Gizmos.DrawLine(intObjs[i].aiTarget.position, tipPosition);

				Vector3 right = Quaternion.Euler(0, 18, 0) * -intObjs[i].aiTarget.forward;
				Vector3 left = Quaternion.Euler(0, -18, 0) * -intObjs[i].aiTarget.forward;

				Gizmos.DrawLine(tipPosition, tipPosition + right * 0.25f);
				Gizmos.DrawLine(tipPosition, tipPosition + left * 0.25f);

				Gizmos.DrawLine(tipPosition + right * 0.25f, tipPosition - intObjs[i].aiTarget.forward * 0.25f);
				Gizmos.DrawLine(tipPosition + left * 0.25f, tipPosition - intObjs[i].aiTarget.forward * 0.25f);
			}
		}

		private void DrawDottedLine(Vector3 start, Vector3 end)
		{
			float segmentLength = 0.03f;
			float segmentSpacing = 0.03f;

			Vector3 direction = (end - start).normalized;
			float remainingDistance = Vector3.Distance(start, end);
			Vector3 currentPoint = start;

			while (remainingDistance > 0)
			{
				Vector3 nextPoint = currentPoint + direction * Mathf.Min(segmentLength, remainingDistance);
				Gizmos.DrawLine(currentPoint, nextPoint);
				remainingDistance -= segmentLength + segmentSpacing;
				currentPoint = nextPoint + direction * segmentSpacing;
			}
		}

		private bool IsEdgeNode(int x, int y)
		{
			Node[] neighbors = GetNeighbours(x, y);
			foreach (Node neighbor in neighbors)
			{
				if (neighbor != null && !neighbor.walkable)
					return false;
				else if (neighbor == null || !neighbor.walkable)
					return true;
			}
			return false;
		}

		private Node[] GetNeighbours(int x, int y)
		{
			Node[] neighbors = new Node[8];
			int index = 0;
			for (int xOffset = -1; xOffset <= 1; xOffset++)
			{
				for (int yOffset = -1; yOffset <= 1; yOffset++)
				{
					if (xOffset == 0 && yOffset == 0)
						continue;

					int neighborX = x + xOffset;
					int neighborY = y + yOffset;

					if (neighborX >= 0 && neighborX < _gridSizeX && neighborY >= 0 && neighborY < _gridSizeY)
						neighbors[index++] = _grid[neighborX, neighborY];
				}
			}
			return neighbors;
		}

		private bool IsUnwalkable(int x, int y)
		{
			if (x < 0 || x >= _gridSizeX || y < 0 || y >= _gridSizeY)
				return true;

			return !_grid[x, y].walkable;
		}
        #endregion
    }
}
