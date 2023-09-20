using System.Collections.Generic;
using UnityEngine;

namespace razz
{
	//Instantiates a pool filled with assigned prefab then spawns (activates) these prefabs in its boundaries randomly.
	[HelpURL("https://negengames.com/interactor/components.html#instantiaterandomareapoolcs")]
	[DisallowMultipleComponent]
	public class InstantiateRandomAreaPool : MonoBehaviour
    {
        [Tooltip("Prefab to instantiate")]
		public GameObject[] poolPrefabs;
        [Tooltip("Pool size that filled with prefabs to spawn")]
		public int maxPrefabCount = 20;
        [Tooltip("Spawn count per button press")]
		public int spawnPerPress = 3;
        [Tooltip("Random spawn area color")]
		public Color color = Color.gray;
		[Header("Press Enter to spawn prefabs?")]
		public bool activateWithEnter;

		//Accessed by InteractorObject to get children after instantiation.
		[HideInInspector] public List<GameObject> _prefabList;

		private bool _initiated;

		private void Awake()
		{
			if (!(GetComponentInParent<InteractorObject>()))
			{
				Debug.Log("InstantiateRandomAreaPool or its parents has no InteractorObject. Pool not instantiated.");
				return;
			}
			
            //Instantiate maxPrefabCount prefabs within random positions of boundaries and set them ready to activate.
            InstantiatePool();

			if (spawnPerPress > maxPrefabCount)
			{
				spawnPerPress = maxPrefabCount;
			}

			_initiated = true;
		}

		private void Update()
		{
			if (!_initiated) return;
			if (!activateWithEnter) return;

			if (BasicInput.GetEnter())
				SetPooledPrefabActive(spawnPerPress);
		}

		public Vector3 GetPoint()
		{
			float minX = transform.position.x - (transform.localScale.x / 2);
			float maxX = transform.position.x + (transform.localScale.x / 2);

			float minY = transform.position.y - (transform.localScale.y / 2);
			float maxY = transform.position.y + (transform.localScale.y / 2);

			float minZ = transform.position.z - (transform.localScale.z / 2);
			float maxZ = transform.position.z + (transform.localScale.z / 2);

			float x = Random.Range(minX, maxX);
			float y = Random.Range(minY, maxY);
			float z = Random.Range(minZ, maxZ);

			return RotatePointAroundPivot(new Vector3(x, y, z), transform.position, transform.eulerAngles);
		}

		private Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
		{
			Vector3 dir = point - pivot;
			dir = Quaternion.Euler(angles) * dir;
			point = dir + pivot;
			return point;
		}

		private void InstantiatePool()
		{
			_prefabList = new List<GameObject>();

			for (int i = 0; i < poolPrefabs.Length; i++)
			{
				if (poolPrefabs[i] == null)
				{
					Debug.Log("There is null value in InstantiateRandomAreaPool prefabs.");
					return;
				}
			}

			for (int i = 0; i < maxPrefabCount; i++)
			{
				GameObject prefab = poolPrefabs[Random.Range(0, poolPrefabs.Length)];
				//Prefabs arent parented to this object because this object isnt scaled as 1 to set spawn area. So Unity gets weird when parent scale isnt (1,1,1) for children.
				_prefabList.Add(GameObject.Instantiate(prefab, GetPoint(), prefab.transform.rotation));
				_prefabList[i].SetActive(false);
			}
        }

		public void SetPooledPrefabActive(int byCount)
		{
			int counter = byCount;

			for (int i = 0; i < byCount; i++)
			{
				for (int a = 0; a < _prefabList.Count; a++)
				{
					if (!_prefabList[a].activeInHierarchy)
					{
						_prefabList[a].SetActive(true);
						counter--;
						if (counter == 0) return;
					}
				}
			}

			for (int j = 0; j < counter; j++)
			{
				int random = Random.Range(0, _prefabList.Count - 1);

				if (_prefabList[random].transform.position.y <= transform.position.y)
				{
					_prefabList[random].GetComponent<Rigidbody>().velocity = Vector3.zero;
					_prefabList[random].transform.position = GetPoint();
				}
				else
				{
					if (counter > _prefabList.Count) return;

					counter++;
				}
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = color;
			DrawCube(transform.position, transform.rotation, transform.localScale);
		}

		private static void DrawCube(Vector3 position, Quaternion rotation, Vector3 scale)
		{
			Matrix4x4 cubeTransform = Matrix4x4.TRS(position, rotation, scale);
			Matrix4x4 oldGizmosMatrix = Gizmos.matrix;

			Gizmos.matrix *= cubeTransform;
			Gizmos.DrawCube(Vector3.zero, Vector3.one);
			Gizmos.matrix = oldGizmosMatrix;
		}
	}
}
