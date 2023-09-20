using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace razz
{
	[HelpURL("https://negengames.com/interactor/components.html#interactoraics")]
	[DefaultExecutionOrder(-20)]//To set the inputs before your controller reads
	[RequireComponent(typeof(Interactor))]
	public class InteractorAi : MonoBehaviour
	{
        #region Variables
        [Tooltip("Assign the Interactor manually for the best practice.")]
		public Interactor interactor;
		[Tooltip("Assign this transform manually for the best practice.")]
		public Transform playerTransform;
		[Tooltip("Assign the Pathfinder in the scene manually for the best practice. If left empty, it will automatically create a new GameObject and add the Pathfinder component if one does not already exist in the scene.")]
		public Pathfinder pathfinder;
		[Header("Controller Settings")]
		[Tooltip("Please select your controller type from the following options: \n\nTwoAxis: Camera orientation does not affect the player's direction. The player moves forward when the forward button is pressed or the forward stick is used. The player's speed also depends on the forward input (can be adjusted with stick push). \n\nTwoAxisCam (Most common): The player moves in the camera's forward direction when the forward button or analog sticks are used. The movement axes constantly adjust based on the camera direction. The movement speed is adjustable. \n\nForward Button: The player moves forward based on its own forward direction, independent of the camera. Speed change is not adjustable, and forward movement is activated by a button press (commonly used in FPS games). \n\nForward String: Similar to the Forward Button option, but instead of a button, you can use a custom input name assigned for the forward movement. \n\nAi: If this character is AI controller, select Ai so InteractorAi won't set any inputs. But you need to get forward and rotation values from InteractorAi functions with scripting (Like in the BasicBots examples).")]
		public CS controlSchema = CS.TwoAxisCam;
		[Tooltip("Vertical Axis input name.")][Conditional(Condition.Show, nameof(ShowVertical))]
		public string verticalName = "Vertical";
		[Tooltip("Horizontal Axis input name.")][Conditional(Condition.Show, nameof(ShowHorizontal))]
		public string horizontalName = "Horizontal";
		[Tooltip("Assign your player camera which rotates around your player so InteractorAi can adjust inputs for setting the forward direction.")][Conditional(Condition.Show, nameof(ShowCam))]
		public Transform playerCamera;
		[Tooltip("Select your forward button. If your game allows keybinding configuration, remember to update the corresponding key in InteractorAi with script whenever any changes are made. Or you can use Forward String instead.")][Conditional(Condition.Show, nameof(ShowForwardButton))]
		public KeyCode forwardButton;
		[Tooltip("Forward button input name.")][Conditional(Condition.Show, nameof(ShowForwardString))]
		public string forwardButtonString;
		[Tooltip("If you want InteractorAi to use running as well, set your Run button.")][Conditional(Condition.Show, nameof(ShowRun))]
		public RS runSchema = RS.NoRunning;
		[Tooltip("Select your run button. If your game allows keybinding configuration, remember to update the corresponding key in InteractorAi with script whenever any changes are made. Or you can use Run String instead.")][Conditional(Condition.Show, nameof(ShowRunButton))]
		public KeyCode runButton;
		[Tooltip("Run button input name.")][Conditional(Condition.Show, nameof(ShowRunString))]
		public string runButtonString;
		[Tooltip("If your controller doesn't allow rotation with adjusting directly transform.rotation, you can disable this and get rotation from InteractorAi functions with scripting.")]
		public bool rotatePlayerDirectly = true;
		[Tooltip("Prevents player inputs when InteractorAi takes control. Otherwise it will stop moving with any input.")]
		public bool preventInput;
		[Header("Move Settings")]
		[Tooltip("If your input axes allow slower movements, you can adjust speed that InteractorAi will use.")]
		public float speed = 1;
		[Tooltip("If your input axes allow slower movements, InteractorAi will reduce the player's speed when it comes within a certain distance (in meters). Set it 0 if it stops or stutters your player (which means your controller don't allow slower movement).")]
		public float stoppingDistance = 1;
		[Tooltip("Minimum speed when stoppingDistance is enabled. To prevent unnecessary slowing down when your character is nearly stopped at low speeds.")]
		public float minimumStopSpeed = 0.35f;
		[Tooltip("Enables the player to start its path from the forward direction, preventing immediate rotation when interaction is at a different direction. Particularly useful when the player is running. Cancels if forward area is unwalkable.")]
		public bool forwardStart = true;
		[Tooltip("Node amount for forward start. More nodes, more forward before turning.")][Range(0, 32)]
		public int forwardNodes = 4;
		[Header("Turn Settings")]
		[Tooltip("Adjust the rotation speed for turning the player. Increase the speed if the player is unable to follow its path effectively, and decrease it if the rotation is too fast.")]
		public float turnSpeed = 4;
		[Tooltip("When following a path, the player will initiate rotation earlier to achieve smoother movement. Lower turn distance allows the player to precisely follow the path but may appear unnatural. On the other hand, a higher turn distance creates smoother and more natural movement but may cause the player to deviate from the path, potentially resulting in collisions with obstacles. (in meters)")]
		public float turnDistance = 1;
		[Tooltip("If your input axes allow slower movements, slows down the player when turning. 1 means no slowdown.")][Range(0.1f, 1f)]
		public float slowdownOnTurns = 0.9f;
		[Tooltip("Minimum angle to start slowing down on rotations.")][Range(1, 180f)]
		public float slowdownAngleLimit = 30f;
		[Tooltip("Disables running while rotating.")]
		public bool runningOff = true;
		[Header("Other Settings")]
		[Tooltip("InteractorAI attempts to update the path if the interaction object is moved or if there are changes in the grid due to moving colliders. If everything is static in your scene, you can increase this because there is no need to update the path. If you have moving interactions or colliders, you can lower this to update faster. (in seconds)")]
		public float minPathUpdateTime = 0.5f;
		[Tooltip("Shows player path, logs error messages if movement cancelled.")]
		public bool showDebug = true;

        private bool _forwardState;
		private float _rotationAngle;
		private Vector2 _dots;
		private Quaternion _finalRotation;

		private Path _path;
		private InteractorObject _intObj;
		private List<PathGrid> _activeGrids;
		private bool _followingPath;
		private bool _run;
		private float _slowdown = 1f;
		private Vector2 _startPos;
		private Coroutine _updateCoroutine;
		private Coroutine _updateMovColsLastTime;
		private bool _earlyStarted;
		private int _inputIndex;
		private bool _firstInitiated;
		private int _cs;
		private int _rs;
		private int _pathIndex;
		private float _pathUpdateMoveThreshold = 0.5f;
		private float _smoothForwardTurn = 0.5f;
		#endregion

		private void Start()
        {
			Init();
			_firstInitiated = true;
		}
        private void OnEnable()
        {
            if (_firstInitiated) Init();
        }
        private void OnDisable()
        {
			InteractorAiInput.RemoveInteractorAi(_inputIndex);
        }
        private void Update()
        {
			if (!_followingPath) return;

			if (!preventInput && CheckUserInput())
			{
				StopFollowing();
				return;
			}
			SetForward();
		}
        private void FixedUpdate()
        {
			if (!_followingPath) return;

			FollowPath();
		}

		#region Public Access Functions To Get Rotation and Forward
		public bool GetForward()
		{
			return _forwardState;
		}
		public float GetYaw()
		{
			return _rotationAngle;
		}
		public Quaternion GetRotation()
		{
			return _finalRotation;
		}
		#endregion

		private void Init()
        {
			_activeGrids = new List<PathGrid>();

			if (controlSchema != CS.Ai)
            {
				_inputIndex = InteractorAiInput.AddInteractorAi(transform, verticalName, horizontalName, runButton, runButtonString, forwardButton, forwardButtonString);
			}
			
			if (_inputIndex < 0)
			{
				Debug.Log("InteractorAi could not get index from InteractorAiInput!", this);

				this.enabled = false;
			}

			if (!pathfinder)
			{
				if (!(pathfinder = FindObjectOfType<Pathfinder>()))
				{
					GameObject pathfinderGO = new GameObject("Pathfinder");
					Pathfinder pathfinder = pathfinderGO.AddComponent<Pathfinder>();
					this.pathfinder = pathfinder;
					Debug.LogWarning("Pathfinder could not be found in this scene and created automatically. Please create one yourself and assign it to this PathfinderAI for best practice.", this);
				}
				else Debug.LogWarning("Please assign Pathfinder to this InteractorAI for best practice.", this);
			}

			if (!interactor)
			{
				interactor = GetComponent<Interactor>();
				Debug.LogWarning("Please assign Interactor to this InteractorAI for best practice.", this);
			}
			interactor.interactorAi = this;

			if (rotatePlayerDirectly && !playerTransform)
			{
				playerTransform = interactor.transform;
				Debug.Log("Player Transform on this InteractorAi is null and has been set as the Interactor gameobject. Please assign main Player object parent.", this);
			}

            switch (controlSchema)
            {
                case CS.TwoAxis:
                    if (string.IsNullOrEmpty(verticalName))
                    {
						Debug.Log("Vertical name on InteractorAi is empty! Write your respective input name on inspector or assign it from your input class at awake.", this);
                    }
					_cs = 0;
					break;
                case CS.TwoAxisCam:
					if (string.IsNullOrEmpty(verticalName))
					{
						Debug.Log("Vertical name on InteractorAi is empty! Write your respective input name on inspector or assign it from your input class at awake.", this);
					}
					if (string.IsNullOrEmpty(horizontalName))
					{
						Debug.Log("Horizontal name on InteractorAi is empty! Write your respective input name on inspector or assign it from your input class at awake.", this);
					}
                    if (playerCamera == null)
                    {
						playerCamera = Camera.main.transform;
						if (playerCamera == null) Debug.Log("Player camera is not assigned on InteractorAi.", this);
					}
					_cs = 1;
					break;
                case CS.ForwardButton:
					if (forwardButton == KeyCode.None)
					{
						Debug.Log("Forward button on InteractorAi is none! Set your respective input on inspector or assign it from your input class at awake.", this);
					}
					_cs = 2;
					break;
                case CS.ForwardString:
					if (string.IsNullOrEmpty(forwardButtonString))
					{
						Debug.Log("Forward name on InteractorAi is empty! Write your respective button name on inspector or assign it from your input class at awake.", this);
					}
					_cs = 2;
					break;
                case CS.Ai:
					_cs = 3;
					break;
            }

            switch (runSchema)
            {
                case RS.NoRunning:
					_rs = 0;
					break;
                case RS.RunButton:
					if (runButton == KeyCode.None)
					{
						Debug.Log("Run button on InteractorAi is none! Set your respective input on inspector or assign it from your input class at awake.", this);
					}
					_rs = 1;
					break;
                case RS.RunString:
					if (string.IsNullOrEmpty(runButtonString))
					{
						Debug.Log("Run name on InteractorAi is empty! Write your respective button name on inspector or assign it from your input class at awake.", this);
					}
					_rs = 2;
					break;
            }
        }

		public void StartPathfinding(InteractorObject intObj)
        {
			for (int i = 0; i < _activeGrids.Count; i++)
            {
                if (_activeGrids[i].CheckInteraction(intObj))
                {
                    if (_rs > 0)
                    {
						if (_rs == 1 && InteractorAiInput.GetKey(runButton))
							_run = true;
                        else if (_rs == 2 && InteractorAiInput.GetButton(runButtonString))
							_run = true;
					}

					_intObj = intObj;
					_earlyStarted = false;
					if (_updateCoroutine != null) StopFollowing();
					if (CheckTargetPosRot()) StartInteraction(_intObj);
                    else _updateCoroutine = StartCoroutine(UpdatePath(_activeGrids[i]));
					return;
				}
            }
        }

		private bool CheckTargetPosRot()
        {
			float angleDiff = Vector3.Dot(_intObj.aiTarget.forward, transform.forward);
			float posDiff = (_intObj.aiTarget.position - transform.position).sqrMagnitude;
			if (angleDiff > 0.98f && posDiff < 0.02f) return true;
			else return false;
        }

		public void StartInteraction(InteractorObject intObj)
        {
			if (intObj.used || _earlyStarted) return;

			intObj.Reached = true;
            interactor.StartStopInteractionAi(intObj, false);
        }

		public void AddInteractions(List<InteractorObject> intObjs, PathGrid grid)
        {//Grid trigger Enter
            if (!_activeGrids.Contains(grid)) _activeGrids.Add(grid);

            foreach (InteractorObject intObj in intObjs)
            {
                if (intObj.aiTarget == null)
                {
					Debug.Log("InteractorObject added to PathGrid as an AI interaction but doesn't have any aiTarget. Assign one if you wish to use it automatically.", intObj.gameObject);
                }
				else interactor.AddInteractionManual(intObj);
            }
        }
		public void AddInteraction(InteractorObject intObj)
		{
			if (intObj.aiTarget == null)
			{
				Debug.Log("InteractorObject added to PathGrid as an AI interaction but doesn't have any aiTarget. Assign one if you wish to use it automatically.", intObj.gameObject);
			}
			else interactor.AddInteractionManual(intObj);
		}

		public void RemoveInteractions(List<InteractorObject> intObjs, PathGrid grid)
		{//Grid trigger Exit
			foreach (InteractorObject intObj in intObjs)
			{
				interactor.RemoveInteractionManual(intObj);
			}

            if (_activeGrids.Contains(grid)) _activeGrids.Remove(grid);
		}
		public void RemoveInteraction(InteractorObject intObj)
		{
			interactor.RemoveInteractionManual(intObj);
		}

		private void OnPathFound(Vector3[] waypoints)
		{
			_path = new Path(waypoints, playerTransform.position, turnDistance, stoppingDistance, forwardStart, forwardNodes, _intObj.startEarly);
			_followingPath = true;
			_pathIndex = 0;
		}

		private IEnumerator UpdatePath(PathGrid grid)
		{
			if (Time.timeSinceLevelLoad < .3f) yield return new WaitForSeconds(.3f);
            if (_activeGrids.Count == 0)
            {
				Debug.Log("There is no active grid for this InteractorAI.", this);
				yield break;
            }

			_pathUpdateMoveThreshold = grid.nodeRadius;
			Vector3[] waypoints = null;
			_startPos = SetStartPos();
			waypoints = pathfinder.FindPath(grid, this, _intObj.aiTarget, playerTransform.forward, forwardStart, forwardNodes, true);
			if (waypoints != null) OnPathFound(waypoints);
            else
            {
                if (showDebug) Debug.Log(this.name + " can't find path to InteractorAi target.", _intObj);
				StopFollowing();
				yield break;
			}

			float sqrMoveThreshold = _pathUpdateMoveThreshold * _pathUpdateMoveThreshold;
			Vector3 targetPosOld = _intObj.aiTarget.position;

			while (true)
			{
				yield return new WaitForSeconds(minPathUpdateTime);

                if (!_intObj)
                {
					StopFollowing();
					yield break;
				}

				if ((_intObj.aiTarget.position - targetPosOld).sqrMagnitude > sqrMoveThreshold)
                {
					_startPos = SetStartPos();
					waypoints = pathfinder.FindPath(grid, this, _intObj.aiTarget, playerTransform.forward, forwardStart, forwardNodes, false);
					if (waypoints != null) OnPathFound(waypoints);
					else
                    {
						if (showDebug) Debug.Log(this.name + " can't update the path to InteractorAi target.", _intObj);
						StopFollowing();
					}

					targetPosOld = _intObj.aiTarget.position;
				}
				else if ((pathfinder.MovingColliderExist(grid) && (_intObj.aiTarget.position - playerTransform.position).sqrMagnitude > sqrMoveThreshold) && _path.finishLineIndex > 2)
				{
					_startPos = SetStartPos();
					waypoints = pathfinder.FindPath(grid, this, _intObj.aiTarget, playerTransform.forward, forwardStart, forwardNodes, false);
					if (waypoints != null) OnPathFound(waypoints);
					else
					{
						if (showDebug) Debug.Log(this.name + " can't update the path to InteractorAi target.", _intObj);
						StopFollowing();
					}
				}
			}
		}

		private void FollowPath()
        {
			_slowdown = 1;
			Vector3 thisPosition = playerTransform.position;
			Quaternion thisRotation = playerTransform.rotation;
			Vector2 pos2D = new Vector2(thisPosition.x, thisPosition.z);
			float dist = 0;

			if (_pathIndex >= _path.startEarlyIndex && !_earlyStarted)
			{
				dist = _path.turnBoundaries[_path.finishLineIndex].DistanceFromPoint(pos2D);
				if (dist < _intObj.startEarly)
				{
					StartInteraction(_intObj);
					_earlyStarted = true;
				}
			}

			if (_path.turnBoundaries[_pathIndex].HasCrossedLine(pos2D))
			{
				if (_pathIndex == _path.finishLineIndex)
				{
					StopFollowing();
					StartInteraction(_intObj);
					return;
				}
				else _pathIndex++;
			}

			if (_pathIndex == _path.turnBoundaries.Length - 2) _run = false;

			if (_pathIndex >= _path.slowDownIndex && stoppingDistance > 0)
			{
                if (dist == 0) dist = _path.turnBoundaries[_path.finishLineIndex].DistanceFromPoint(pos2D);
				_slowdown = Mathf.Clamp(dist / stoppingDistance, minimumStopSpeed, 1f);
            }

			Quaternion targetRotation = Quaternion.LookRotation(_path.lookPoints[_pathIndex] - thisPosition);

			if (_pathIndex == _path.turnBoundaries.Length - 1)
			{
				Vector2 minusOnePos = _startPos;
				if (_path.turnBoundaries.Length > 1)
				{
					minusOnePos = new Vector2(_path.lookPoints[_path.lookPoints.Length - 2].x, _path.lookPoints[_path.lookPoints.Length - 2].z);
				}

				float minusOneDist = _path.turnBoundaries[_path.finishLineIndex].DistanceFromPoint(minusOnePos);
				float playerDist = _path.turnBoundaries[_path.finishLineIndex].DistanceFromPoint(pos2D);
				playerDist = Mathf.Clamp(playerDist, 0, minusOneDist);
				float val = 1.4f - (playerDist / minusOneDist);
				targetRotation = Quaternion.Lerp(targetRotation, _intObj.aiTarget.rotation, val);
				Vector3 eulerRot0 = thisRotation.eulerAngles;
				Vector3 eulerYrot0 = new Vector3(eulerRot0.x, targetRotation.eulerAngles.y, eulerRot0.z);
				thisRotation = Quaternion.Lerp(thisRotation, Quaternion.Euler(eulerYrot0), Time.fixedDeltaTime * turnSpeed);
			}

			if (slowdownOnTurns != 1f)
			{
				float angle = Quaternion.Angle(targetRotation, thisRotation);
				if (slowdownAngleLimit <= angle)
					_slowdown *= slowdownOnTurns;
			}

			Vector3 eulerRot1 = thisRotation.eulerAngles;
			Vector3 eulerYrot1 = new Vector3(eulerRot1.x, targetRotation.eulerAngles.y, eulerRot1.z);

			float slowTurn = 1f;
			if (_pathIndex < 2 && forwardStart) slowTurn = 1f - _smoothForwardTurn;

			_finalRotation = Quaternion.Lerp(thisRotation, Quaternion.Euler(eulerYrot1), Time.fixedDeltaTime * turnSpeed * slowTurn);
			SetRotation();
		}

		private void StopFollowing()
        {
			_followingPath = false;
			_run = false;
			InteractorAiInput.Reset(_inputIndex);
			_forwardState = false;
			_rotationAngle = 0;
			if (_updateCoroutine != null)
            {
				StopCoroutine(_updateCoroutine);
				_updateCoroutine = null;
			}
            if (_activeGrids.Count > 0) _updateMovColsLastTime = StartCoroutine(UpdateMovColsLastTime());
		}

		private IEnumerator UpdateMovColsLastTime()
        {//Calls for all grids that player entered, also calls them in seperate frames
		 //because when one grid updates moving objects, it sets their transform.hasChanged to false.
			float elapsedTime = 0f;
			int currentIndex = 0;
			while (elapsedTime < 5f)
			{
				if (currentIndex < _activeGrids.Count)
				{
					_activeGrids[currentIndex].UpdateMovingColliders();
					currentIndex++;
				}

				if (currentIndex >= _activeGrids.Count)
				{
					yield return new WaitForSeconds(minPathUpdateTime);
					elapsedTime += minPathUpdateTime;
					currentIndex = 0;
				}
				else yield return null;
			}
			if (_updateMovColsLastTime != null) StopCoroutine(_updateMovColsLastTime);
			_updateMovColsLastTime = null;
		}

		private void SetForward()
        {
			if (_run)
			{
				InteractorAiInput.SetRunButton(true, _inputIndex);
				if (runningOff && _slowdown != 1f) InteractorAiInput.SetRunButton(false, _inputIndex);
			}
			else InteractorAiInput.SetRunButton(false, _inputIndex);

            switch (_cs)
            {
                case 0: InteractorAiInput.SetVertical(speed * _slowdown, _inputIndex);
					break;
                case 1:
                    {
						_dots = Vector2.zero;
						if (playerCamera) _dots = CameraRelatedForward();
						InteractorAiInput.SetVertical(_dots.x * speed * _slowdown, _inputIndex);
						InteractorAiInput.SetHorizontal(-_dots.y * speed * _slowdown, _inputIndex);
					}
                    break;
                case 2: InteractorAiInput.SetForwardButton(true, _inputIndex);
                    break;
                case 3:
					_forwardState = true;
					break;
            }
		}
		private void SetRotation()
		{
			if (rotatePlayerDirectly) playerTransform.rotation = _finalRotation;
            else AddYaw(_finalRotation);
		}
		private void AddYaw(Quaternion finalRotation)
        {
			float angle = Quaternion.Angle(playerTransform.rotation, finalRotation);
			Vector3 perp = Vector3.Cross(playerTransform.forward, (_path.lookPoints[_pathIndex] - playerTransform.position));
			float dir = Vector3.Dot(perp, playerTransform.up);
			if (dir < 0) angle *= -1f;
			_rotationAngle = angle;
		}

        private bool CheckUserInput()
        {
			if (_cs == 3) return false;

			if (_cs == 2)
			{
				if (controlSchema == CS.ForwardButton && InteractorAiInput.GetKey(forwardButton)) return true;
				else if (controlSchema == CS.ForwardString && InteractorAiInput.GetButton(forwardButtonString)) return true;
				else return false;
			}
            else
            {
                if (_cs == 0) return InteractorAiInput.CheckUserInput(false, _inputIndex);
				else return InteractorAiInput.CheckUserInput(true, _inputIndex);
			}
        }

		private Vector2 SetStartPos()
        {
			return new Vector2(playerTransform.position.x, playerTransform.position.z);
        }

		private Vector2 CameraRelatedForward()
        {
			Vector3 camForward = Vector3.Scale(playerCamera.forward, new Vector3(1, 0, 1)).normalized;
			float vDot = Vector3.Dot(camForward, playerTransform.forward);
			float hDot = Vector3.Dot(camForward, playerTransform.right);
			return new Vector2(vDot, hDot);
        }

		public void OnDrawGizmos()
		{
			if (_path != null && showDebug) _path.DrawWithGizmos();
		}

		#region Variable and Inspector Conditions
		public bool ShowVertical()
		{
			if (controlSchema == CS.TwoAxis || controlSchema == CS.TwoAxisCam) return true;
			else return false;
		}
		public bool ShowHorizontal()
		{
			if (controlSchema == CS.TwoAxisCam) return true;
			else return false;
		}
		public bool ShowCam()
		{
			if (controlSchema == CS.TwoAxisCam) return true;
			else return false;
		}
		public bool ShowForwardButton()
		{
			if (controlSchema == CS.ForwardButton) return true;
			else return false;
		}
		public bool ShowForwardString()
		{
			if (controlSchema == CS.ForwardString) return true;
			else return false;
		}
		public bool ShowRun()
		{
			if (controlSchema != CS.Ai) return true;
			else return false;
		}
		public bool ShowRunButton()
		{
			if (runSchema == RS.RunButton && controlSchema != CS.Ai) return true;
			else return false;
		}
		public bool ShowRunString()
		{
			if (runSchema == RS.RunString && controlSchema != CS.Ai) return true;
			else return false;
		}
		public enum CS{ TwoAxis, TwoAxisCam, ForwardButton, ForwardString, Ai }
		public enum RS { NoRunning, RunButton, RunString }
		#endregion
	}
}
