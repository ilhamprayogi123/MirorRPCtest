using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace razz
{
	[HelpURL("https://negengames.com/interactor/components.html#interactorikcs")]
	[DisallowMultipleComponent]
	public class InteractorIK : MonoBehaviour
	{
		public enum IKPart
		{
			LeftFoot = 0,
			RightFoot = 1,
			LeftHand = 2,
			RightHand = 3,
			Body = 4,
			//LeftShoulder = 5,
			//RightShoulder = 6,
			//LeftThigh = 7,
			//RightThigh = 8
		};

		/*public enum FullBodyBipedEffector
		{
			Body,
			LeftShoulder,
			RightShoulder,
			LeftThigh,
			RightThigh,
			LeftHand,
			RightHand,
			LeftFoot,
			RightFoot
		}*/

		//Interactor checks this for integration automation.
		public static short defaultFiles = 0;

		[SerializeField] private Animator _animator;
		public Animator Animator
		{
			get
			{
				if (_animator == null)
				{
					_animator = GetComponentInChildren<Animator>();
					if (_animator == null) Debug.LogWarning("Animator component could not found. Please assign it to InteractorIK manually.", this);
					else Debug.Log("Assign Animator to InteractorIK manually for best practice.", this);
				}
				return _animator;
			}
			set { _animator = value; }
		}
		public bool isHumanoid = true;
		public IKParts[] ikParts;

		[HideInInspector] public bool lookEnabled = true;
		[HideInInspector] public Transform lookTarget;
		[HideInInspector] public float lookWeight;
		[HideInInspector] public Transform headBone;
		[HideInInspector] public Vector3 lastHeadDirection;

		//Used for holding IKParts' ikpart values as effector type int (FullBodyBipedEffector)
		private int[] _effectorOrder;
		private bool _useLateFixedUpdate = false;
		private bool _lateFixedUpdating = false;

		private void OnValidate()
		{
			if (ikParts == null || isHumanoid) return;

			for (int i = 0; i < ikParts.Length; i++)
			{
				ikParts[i].Validate();
			}
		}
		private void OnEnable()
		{
            if (Animator && Animator.updateMode == AnimatorUpdateMode.AnimatePhysics && !_lateFixedUpdating)
            {
				_useLateFixedUpdate = true;
				StartCoroutine("LateFixedUpdate");
			}
		}
		private void OnDisable()
		{
			if (_lateFixedUpdating) StopCoroutine("LateFixedUpdate");
		}

		private void Start()
		{
			if (ikParts.Length == 0) return;
            if (!Animator) return;

			//10 long array is enough to hold all parts
			_effectorOrder = new int[10];
            if (Animator.updateMode == AnimatorUpdateMode.AnimatePhysics && !_lateFixedUpdating)
            {
				_useLateFixedUpdate = true;
				StartCoroutine("LateFixedUpdate");
			}

			for (int i = 0; i < ikParts.Length; i++)
			{
				ikParts[i].Init(Animator, isHumanoid);
				bool success = ikParts[i].Init(Animator, isHumanoid);
				if (!success) continue;
				if ((int)ikParts[i].part > 3) continue; //Skip other than hands and feet

				if (ikParts[i].matchChildBones)
				{
					ikParts[i].childBones = ikParts[i].boneTransform.GetComponentsInChildren<Transform>();

					//Remove excluded transfrom hierarchy from actual bone transforms
                    if (ikParts[i].excludeFromBones.Length > 0)
                    {
						List<Transform> transformRemoval = new List<Transform>();

						for (int a = 0; a < ikParts[i].excludeFromBones.Length; a++)
                        {
							Transform[] excludedTransforms;
							if (ikParts[i].excludeFromBones[a])
                            {
								excludedTransforms = ikParts[i].excludeFromBones[a].GetComponentsInChildren<Transform>();
								transformRemoval.AddRange(excludedTransforms);
							}
                        }

						List<Transform> newChildBones = new List<Transform>();
						for (int j = 0; j < ikParts[i].childBones.Length; j++)
                        {
                            if (!transformRemoval.Contains(ikParts[i].childBones[j]))
								newChildBones.Add(ikParts[i].childBones[j]);
                        }
						ikParts[i].childBones = newChildBones.ToArray();
					}
                }
			}
			SetEffectorOrder();
		}

		public void SetHeadBone(Transform head)
        {
			headBone = head;
        }

		//Caching ikparts' part ints as FullBodyBipedEffector ints, so we dont have to check every call
		//which part is for which effector type
		private void SetEffectorOrder()
		{
			for (int i = 0; i < ikParts.Length; i++)
			{
				_effectorOrder[AvatorGoalToEffector(ikParts[i].part)] = i;
			}
		}

		private int EffectorToIKpart(Interactor.FullBodyBipedEffector effector)
		{
			int i = _effectorOrder[(int)effector];

			if (ikParts[i] == null)
			{
				Debug.LogWarning("Interactor has " + effector + ", but InteractorIK has not that part.", this);
				return -1;
			}
			return i;
		}

		public void StartInteraction(Interactor.FullBodyBipedEffector effector, InteractorTarget interactorTarget, InteractorObject interactorObject)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].StartInteraction(interactorTarget, interactorObject);
		}

		public void PauseInteraction(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].PauseInteraction();
		}

		public void ResumeInteraction(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].ResumeInteraction();
		}

		public void ResumeInteractionWithoutReset(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].ResumeInteractionWithoutReset();
		}

		public void ResetAfterResume(Interactor.FullBodyBipedEffector effector)
        {
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].ResetAfterResume();
		}

		public void ResumeAll()
		{
			for (int i = 0; i < ikParts.Length; i++)
			{
				if (ikParts[i].pause)
				{
					ikParts[i].ResumeInteraction();
				}
			}
		}

		public void ReverseInteraction(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].ReverseInteraction();
		}

		public void ReverseAll()
		{
			for (int i = 0; i < ikParts.Length; i++)
			{
				if (ikParts[i].enabled)
				{
					ikParts[i].ReverseInteraction();
				}
			}
		}

		public void StopInteraction(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].StopInteraction();
		}

		public void StopAll()
		{
			for (int i = 0; i < ikParts.Length; i++)
			{
				if (ikParts[i].enabled)
				{
					ikParts[i].StopInteraction();
				}
			}
		}

		public float GetProgress(Interactor.FullBodyBipedEffector effector)
		{//0 to 1f target path, 1f is target, 1f to 2f back path
			int i = EffectorToIKpart(effector);
			if (i < 0) return 0;

			return ikParts[i].GetProgress();
		}

		public Transform GetTargetTransform(Interactor.FullBodyBipedEffector effector)
        {
			int i = EffectorToIKpart(effector);
			if (i < 0) return null;
			if (!ikParts[i].currentTarget) return null;

			return ikParts[i].currentTarget.transform;
		}

		public bool IsPaused(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return false;

			return ikParts[i].IsPaused();
		}

		public bool IsInInteraction(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return false;

			return ikParts[i].enabled;
		}

		public Transform GetBone(Interactor.FullBodyBipedEffector effector)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return null;

			return ikParts[i].boneTransform;
		}

		//Converting FullBodyBipedEffector int (Coming from Interactor) to AvatarGoal int (used by Unity IK)
		private int EffectorToAvatarGoal(Interactor.FullBodyBipedEffector effector)
		{
			switch ((int)effector)
			{
				case 0:
					return 4;
				case 1:
					return 5;
				case 2:
					return 6;
				case 3:
					return 7;
				case 4:
					return 8;
				case 5:
					return 2;
				case 6:
					return 3;
				case 7:
					return 0;
				case 8:
					return 1;
				default:
					return -1;
			}
		}
		//Converting AvatarGoal int (used by Unity IK) to FullBodyBipedEffector int (Coming from Interactor)
		public static int AvatorGoalToEffector(IKPart part)
		{
			switch ((int)part)
			{
				case 0:
					return 7;
				case 1:
					return 8;
				case 2:
					return 5;
				case 3:
					return 6;
				case 4:
					return 0;
				case 5:
					return 1;
				case 6:
					return 2;
				case 7:
					return 3;
				case 8:
					return 4;
				default:
					return -1;
			}
		}

		public void ChangeIKPartTarget(Interactor.FullBodyBipedEffector effector, InteractorTarget newTarget, InteractorObject newInteractorObject)
        {
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].currentTarget = newTarget;
			ikParts[i].targetDuration = newInteractorObject.targetDuration;
			ikParts[i].backDuration = newInteractorObject.backDuration;
			ikParts[i].pause = newInteractorObject.pauseOnInteraction;
			ikParts[i].easer = Ease.FromType(newInteractorObject.easeType);
			ikParts[i].currentTarget.PrepareTarget(ikParts[i].positionBeforeIK);
		}

		public void ChangeIKPartWeight(Interactor.FullBodyBipedEffector effector, float newWeight)
		{
			int i = EffectorToIKpart(effector);
			if (i < 0) return;

			ikParts[i].SetNewWeight(newWeight);
		}

		public void OnAnimatorIK(int layerIndex)
		{//Needs to be called by Animator. If Animator is on different object (which is wrong), put AnimatorCallback.cs on that object and assign InteractorIK.
            if (isHumanoid)
            {
				for (int i = 0; i < ikParts.Length; i++)
				{
					CacheOriginals(ikParts[i]);

					if (ikParts[i].enabled && ikParts[i].currentTarget)
					{
						ikParts[i].UpdateWeight();
						SetAnimatorIKPos(ikParts[i]);

                        if (ikParts[i].currentTarget && ikParts[i].fixWristDeformation && ikParts[i].currentTarget.setRotation)
							SetAnimatorIKRot(ikParts[i]);
					}
				}

				if (!lookEnabled) return;

				if (lookTarget != null && lookWeight > 0)
				{
					SetLook(lookTarget, lookWeight);
				}
			}
		}

		private void LateUpdate()
		{
			if (_useLateFixedUpdate) return;

			CalculateAfterAnim();
		}

		private IEnumerator LateFixedUpdate()
		{
			_lateFixedUpdating = true;
			while (_useLateFixedUpdate)
			{
				yield return new WaitForFixedUpdate();
				CalculateAfterAnim();
			}
			_lateFixedUpdating = false;
		}

		private void CalculateAfterAnim()
        {
			if (!isHumanoid) //TwoBoneIK
			{
				for (int i = 0; i < ikParts.Length; i++)
				{
					if (ikParts[i].enabled && ikParts[i].currentTarget)
					{
						ikParts[i].UpdateWeight();
						ikParts[i].positionBeforeIK = ikParts[i].boneTransform.position;
						ikParts[i].SolveIKPart();
					}
				}

				if (lookTarget != null && lookWeight > 0)
				{
					SetLookAlternativeHeadBone(lookTarget, lookWeight);
				}
			}

			for (int i = 0; i < ikParts.Length; i++)
			{
				if (ikParts[i].enabled && ikParts[i].currentTarget)
				{
					//We're changing bone rotation here instead of SetAnimatorIKRot because
					//SetIKRotation needs a direction then it calculates target rotation,
					//not the goal rotation itself. We already have final bone rotation.
					//But SetIKRotation also fixes wrist rotation deformations.
					ikParts[i].boneTransform.rotation = ikParts[i].currentTarget.GetRotation(ikParts[i].boneTransform.rotation, ikParts[i].weight);

					if (ikParts[i].matchChildBones) //Global toggle on InteractorIK
					{
						if ((int)ikParts[i].part > 3) continue; //Skip other than hands and feet

						//This is for matching the effector bones to children bones of target. 
						//Its LateUpdate because it needs to be after Unity animation jobs done.
						SetChildBones(ikParts[i]);
					}
				}
				else if (ikParts[i].matchChildBones && ikParts[i].waitForReset)
				{
					if ((int)ikParts[i].part > 3) continue;

					SetChildBones(ikParts[i]);
				}
			}

			//Cache the last head forward direction for LookAtTarget look ending process.
			//We need it here because it needs to be after IK and look updates.
			if (headBone) lastHeadDirection = headBone.forward;
		}

		private void CacheOriginals(IKParts ikPart)
        {
			if ((int)ikPart.part < 4)
            {
				if (!ikPart.boneTransform) return;

				ikPart.positionBeforeIK = ikPart.boneTransform.position;
				ikPart.rotationBeforeIK = ikPart.boneTransform.rotation;
			}
			else
            {
				ikPart.positionBeforeIK = Animator.bodyPosition;
				ikPart.rotationBeforeIK = Animator.bodyRotation;
			}
		}

		private void SetAnimatorIKPos(IKParts ikPart)
		{
			//Hands & Feet
			if ((int)ikPart.part < 4)
			{
				Animator.SetIKPosition((AvatarIKGoal)ikPart.part, ikPart.weightedPosition);
				Animator.SetIKPositionWeight((AvatarIKGoal)ikPart.part, Mathf.Ceil(ikPart.weight));
                return;
			}
			//Body
			else if ((int)ikPart.part == 4)
			{
				Animator.bodyPosition = ikPart.weightedPosition;
				return;
			}
		}
		private void SetAnimatorIKRot(IKParts ikPart)
		{
			if ((int)ikPart.part < 4)
			{
				Animator.SetIKRotation((AvatarIKGoal)ikPart.part, ikPart.currentTarget.transform.rotation);
				Animator.SetIKRotationWeight((AvatarIKGoal)ikPart.part, ikPart.weight);
				return;
			}
		}

		private void SetLook(Transform target, float weight)
		{
			Animator.SetLookAtPosition(target.position);
			Animator.SetLookAtWeight(weight);
		}

		private void SetLookAlternativeHeadBone(Transform target, float weight)
		{
			Quaternion oneBoneTargetRotation = Quaternion.LookRotation(target.position - headBone.position);
			headBone.rotation = Quaternion.Slerp(headBone.rotation, oneBoneTargetRotation, weight);
		}

		private void SetChildBones(IKParts ikpart)
        {
            if (ikpart.currentTarget == null || !ikpart.currentTarget.MatchSource) return;
			if (!ikpart.currentTarget.matchChildBones) return;

            if (ikpart.waitForReset)
            {
				ikpart.currentTarget.RotateChildren(ikpart.childBones, 1f);
				return;
			}

			ikpart.currentTarget.RotateChildren(ikpart.childBones, ikpart.weight);
		}

		[System.Serializable]
		public class IKParts
		{
			[Tooltip("Select the body part. This will match with the Interactor effector type.")]
			public IKPart part;
			[Tooltip("Global control for matching child bone rotations (Fingers for hand for example). Only possible for hands and feet. Also you have this option in every InteractorTarget if you wish to disable for a specific target only.")]
			public bool matchChildBones = true;
			[Tooltip("When disabled, hand rotation will focus on wrist but this can cause deformation on the wrist when target rotation is too much. Enabling this option will fix wrist deformation with minor performance cost by distributing some of the rotation to lower arm (Like in real world).")]
			public bool fixWristDeformation;
			[Tooltip("If you wish to exclude a transform (with its children) from this bone hierarchy, assign here. (A child object on hand for example. So this way bone count won't change and excluded objects won't be included for matching child rotations.) Hands and feet only.")]
			public Transform[] excludeFromBones;
			[Tooltip("Current target for this IK Part. Debug purposes only, will be changed by Interator in runtime.")]
			[ReadOnly] public InteractorTarget currentTarget;
			[Tooltip("Current weight for this IK Part (0 is default animation position, 1 is target position). Debug purposes only, will be changed by InteratorIK in runtime.")]
			[ReadOnly] public float weight;

			//TwoBoneIK properties
			[Space(10)]
			[Conditional(Condition.Show, nameof(IsGeneric))]
			[Tooltip("Assign the root bone here. (Arm/shoulder for example)")]
			public Transform rootBone;
			[Conditional(Condition.Show, nameof(IsGeneric))]
			[Tooltip("Assign the middle bone here. (Forearm/elbow for example)")]
			public Transform midBone;
			[Conditional(Condition.Show, nameof(IsGeneric))]
			[Tooltip("Assign the tip bone here. (Hand for example)")]
			public Transform tipBone;
			[Conditional(Condition.Show, nameof(IsGeneric))]
			[Tooltip("Assign the hint transform here. (Middle bone will bend towards this)")]
			public Transform hint;
			[Conditional(Condition.Show, nameof(IsGeneric))]
			[Tooltip("Since bone structures change a lot, you need to set this yourself until you're satisfied with results. If your IK animation looks weird, change the value until it fixes (between 0 - 359 degrees).")]
			public float rootRotationOffset;
			[Conditional(Condition.Show, nameof(IsGeneric))]
			[Tooltip("Since bone structures change a lot, you need to set this yourself until you're satisfied with results. If your IK animation looks weird, change the value until it fixes (between 0 - 359 degrees).")]
			public float midRotationOffset;

			[HideInInspector] public float targetDuration;
			[HideInInspector] public float backDuration;
			[HideInInspector] public bool pause;
			[HideInInspector] public bool waitForReset;
			[HideInInspector] public Transform boneTransform;
			[HideInInspector] public bool enabled;
			[HideInInspector] public bool interrupt;
			[HideInInspector] public Easer easer;
			[HideInInspector] public Transform[] childBones;
			[HideInInspector] public Vector3 weightedPosition;
			[HideInInspector] public Vector3 positionBeforeIK;
			[HideInInspector] public Quaternion rotationBeforeIK;

			private AvatarIKGoal _avatarIKGoal;
			private float _elapsed;
			private bool _halfDone;
			private bool _interactReset;
			private float _lastWeightBeforeHalf;

			//TwoBoneIK
			private TwoBoneIKSolver _twoBoneIKSolver;

			public bool Init(Animator anim, bool isHumanoid)
			{
                if (isHumanoid) //Unity IK
                {
					_avatarIKGoal = (AvatarIKGoal)part;
					boneTransform = anim.GetBoneTransform((HumanBodyBones)AvatarGoaltoHBB(_avatarIKGoal));
				}
				else //TwoBoneIK
				{
                    if (!rootBone)
                    {
						Debug.LogWarning(part + "'s root bone is missing on InteractorIK!");
						return false;
					}
                    if (tipBone)
						boneTransform = tipBone;
                    else if(midBone)
						boneTransform = midBone;
                    else
						boneTransform = rootBone;

					if (!(_twoBoneIKSolver = rootBone.GetComponent<TwoBoneIKSolver>()))
					{
						_twoBoneIKSolver = rootBone.gameObject.AddComponent<TwoBoneIKSolver>();
					}
					_twoBoneIKSolver.Init(rootBone, midBone, tipBone, hint, rootRotationOffset, midRotationOffset);
				}

				//Setting default values for those shouldn't be zero
				if (backDuration <= 0) backDuration = 1f;
				if (targetDuration <= 0) targetDuration = 1f;
				return true;
			}

			//TwoBoneIK
			public void Validate()
            {
                if (_twoBoneIKSolver)
                {
					_twoBoneIKSolver.Validate(rootRotationOffset, midRotationOffset);
				}
            }
			public void SolveIKPart()
            {
				if (_twoBoneIKSolver)
				{
					_twoBoneIKSolver.SolveIK(weightedPosition, weight);
				}
			}

			//Unity IK
			//Converts an int from Unity AvatarGoal value to Unity HumanBodyBones
			private int AvatarGoaltoHBB(AvatarIKGoal input)
			{
				switch ((int)input)
				{
					case 0:
						return 5;
					case 1:
						return 6;
					case 2:
						return 17;
					case 3:
						return 18;
					case 4:
						return 7;
					case 5:
						return 11;
					case 6:
						return 12;
					case 7:
						return 1;
					case 8:
						return 2;
					default:
						return -1;
				}
			}

			public void SetNewWeight(float newWeight)
            {
				currentTarget.PrepareTarget(positionBeforeIK);
				_elapsed = (targetDuration + backDuration) * newWeight;
            }

			public void StartInteraction(InteractorTarget interactorTarget, InteractorObject interactorObject)
			{
				if (enabled && !interrupt) return;
				if (enabled) ResetIK();

				this.targetDuration = interactorObject.targetDuration;
				this.backDuration = interactorObject.backDuration;
				this.pause = interactorObject.pauseOnInteraction;
				this.easer = Ease.FromType(interactorObject.easeType);
				this.interrupt = interactorObject.interruptible;

				this.currentTarget = interactorTarget;
				currentTarget.PrepareTarget(boneTransform.position);

				enabled = true;
			}

			public void PauseInteraction()
			{
				pause = true;
			}

			public void ResumeInteraction()
			{
				pause = false;
			}
			
			public void ResumeInteractionWithoutReset()
			{
				pause = false;
				waitForReset = true;
			}

			public void ResetAfterResume()
			{
				waitForReset = false;
			}

			public void ReverseInteraction()
			{
				if (!_halfDone)
				{
					_halfDone = true;
					_elapsed = 0;
					pause = false;
				}
			}

			public void StopInteraction()
			{
				ResetIK();
				enabled = false;
			}

			public float GetProgress()
			{
				if (!_halfDone) return (_elapsed / targetDuration);
				else return 1f + (_elapsed / backDuration);
			}

			public bool IsPaused()
			{
				if (!_halfDone)
				{
					return false;
				}
				else
				{
					return pause;
				}
			}

			private void CalcWeight()
			{
				if (!enabled) return;
                if (!currentTarget)
                {
					StopInteraction();
					return;
				}

				if (_elapsed < targetDuration && !_halfDone)
				{
					_elapsed += Time.deltaTime;
					weight = Mathf.Clamp01(easer((_elapsed / targetDuration), currentTarget.IntObj.speedCurve));
					currentTarget.UpdateFirstAndLastPosition(positionBeforeIK);
					weightedPosition = currentTarget.GetTargetPosition(weight);
					_lastWeightBeforeHalf = weight;
				}
				else if (_elapsed >= targetDuration && !_halfDone)
				{
					_halfDone = true;
					_elapsed = 0;
				}
                if (_halfDone && pause)
					weightedPosition = currentTarget.GetBackPosition(1f); //Continue to call to update targets values

				if (_elapsed < backDuration && _halfDone && !pause)
				{
					_elapsed += Time.deltaTime;
					_elapsed *= currentTarget.BackPathSpeed();

					if (currentTarget.IntObj.easeType == EaseType.CustomCurve)
                    {
						weight = Mathf.Clamp01(easer((1f + (_elapsed / backDuration)), currentTarget.IntObj.speedCurve));
					}
                    else
                    {
						weight = Mathf.Clamp01(_lastWeightBeforeHalf - easer(_elapsed / backDuration));
					}
					
					currentTarget.UpdateFirstAndLastPosition(positionBeforeIK);
					weightedPosition = currentTarget.GetBackPosition(weight);
				}
				else if (_elapsed >= backDuration && _halfDone && !pause && !waitForReset)
				{
					_interactReset = true;
				}

				if (_interactReset)
				{
					ResetIK();
					enabled = false;
					_interactReset = false;
				}
			}

			private void ResetIK()
			{
				_halfDone = false;
				_elapsed = 0;
				//pause = false;
				weight = 0;
                if (currentTarget)
                {
					currentTarget.EndTarget();
					currentTarget = null;
				}
            }

			public void UpdateWeight()
			{
				if (!enabled) return;

				CalcWeight();
			}
		}

		private bool IsGeneric()
		{
			if (isHumanoid) return false;
			else return true;
		}
	}
}
