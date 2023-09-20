using UnityEngine;

namespace razz
{
    public class TwoBoneIKSolver : MonoBehaviour
    {
		[Tooltip("Assign the root bone here. (Arm/shoulder for example)")]
		public Transform rootBone;
		[Tooltip("Assign the middle bone here. (Forearm/elbow for example)")]
		public Transform midBone;
		[Tooltip("Assign the tip bone here. (Hand for example)")]
		public Transform tipBone;
		[Tooltip("Assign the hint transform here. (Middle bone will bend towards this)")]
		public Transform hint;
		[Tooltip("Since bone structures change a lot, you need to set this yourself until you're satisfied with results. If your IK animation looks weird, change the value until it fixes (between 0 - 359 degrees).")]
		public float rootRotationOffset;
		[Tooltip("Since bone structures change a lot, you need to set this yourself until you're satisfied with results. If your IK animation looks weird, change the value until it fixes (between 0 - 359 degrees).")]
		public float midRotationOffset;
		public Vector3 targetPosition;
		[Range(0, 1f)] public float weight;

		private bool _onlyRootSetup;
		private bool _oneBoneSetup;
		private Quaternion _oneBoneTargetRotation;
		private float _cosA, _cosB, _cosC;
		private Vector3 _normalPlane;
		private GameObject _rootBoneTargetGO, _midBoneTargetGO, _tipBoneTargetGO;
		private Transform _rootBoneTarget, _midBoneTarget, _tipBoneTarget;

		public void Init(Transform rootBone, Transform midBone, Transform tipBone, Transform hint, float rootRotationOffset, float midRotationOffset)
        {
			this.rootBone = rootBone;
            if (!midBone)
            {
				_onlyRootSetup = true;
				return;
			}
			if (!tipBone)
			{
				_oneBoneSetup = true;
				return;
			}

			this.midBone = midBone;
			this.tipBone = tipBone;
			this.hint = hint;
			this.rootRotationOffset = rootRotationOffset;
			this.midRotationOffset = midRotationOffset;

			_rootBoneTargetGO = new GameObject();
			_rootBoneTargetGO.name = this.rootBone.name + "_IKTarget";
			_rootBoneTargetGO.hideFlags = HideFlags.DontSave;
			_rootBoneTarget = _rootBoneTargetGO.transform;
			_midBoneTargetGO = new GameObject();
			_midBoneTargetGO.name = this.midBone.name + "_IKTarget";
			_midBoneTargetGO.hideFlags = HideFlags.DontSave;
			_midBoneTarget = _midBoneTargetGO.transform;
			_tipBoneTargetGO = new GameObject();
			_tipBoneTargetGO.name = this.tipBone.name + "_IKTarget";
			_tipBoneTargetGO.hideFlags = HideFlags.DontSave;
			_tipBoneTarget = _tipBoneTargetGO.transform;

			_rootBoneTarget.parent = this.rootBone.parent;
			_midBoneTarget.parent = _rootBoneTarget;
			_tipBoneTarget.parent = _midBoneTarget;

			_rootBoneTarget.position = rootBone.position;
			_rootBoneTarget.rotation = rootBone.rotation;
			_midBoneTarget.localPosition = midBone.localPosition;
			_midBoneTarget.localRotation = midBone.localRotation;
			_tipBoneTarget.localPosition = tipBone.localPosition;
			_tipBoneTarget.localRotation = tipBone.localRotation;
		}

        public void Validate(float rootRotationOffset, float midRotationOffset)
        {
			this.rootRotationOffset = rootRotationOffset;
			this.midRotationOffset = midRotationOffset;
		}

		public void SolveIK(Vector3 targetPos, float weight)
        {
			targetPosition = targetPos;
			this.weight = Mathf.Ceil(weight);
			if (_onlyRootSetup) return;
			if (_oneBoneSetup)
			{
				OneBoneRotation();
				return;
			}

			if (this.weight > 0)
            {
				_cosA = _midBoneTarget.localPosition.magnitude;
				_cosB = _tipBoneTarget.localPosition.magnitude;
				_cosC = Vector3.Distance(_rootBoneTarget.position, targetPosition);
				_normalPlane = Vector3.Cross(targetPosition - _rootBoneTarget.position, hint.position - _rootBoneTarget.position);

				//Set the rotation of the target root bone
				_rootBoneTarget.rotation = Quaternion.LookRotation(targetPosition - _rootBoneTarget.position, Quaternion.AngleAxis(rootRotationOffset, _midBoneTarget.position - _rootBoneTarget.position) * (_normalPlane));
				_rootBoneTarget.rotation *= Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, _midBoneTarget.localPosition));
				_rootBoneTarget.rotation = Quaternion.AngleAxis(-CosAngle(_cosA, _cosC, _cosB), -_normalPlane) * _rootBoneTarget.rotation;
				//Set the rotation of the lower arm
				_midBoneTarget.rotation = Quaternion.LookRotation(targetPosition - _midBoneTarget.position, Quaternion.AngleAxis(midRotationOffset, _tipBoneTarget.position - _midBoneTarget.position) * (_normalPlane));
				_midBoneTarget.rotation *= Quaternion.Inverse(Quaternion.FromToRotation(Vector3.forward, _tipBoneTarget.localPosition));
			}

			rootBone.rotation = Quaternion.Slerp(rootBone.rotation, _rootBoneTarget.rotation, weight);
			midBone.rotation = Quaternion.Slerp(midBone.rotation, _midBoneTarget.rotation, weight);
		}

		private void OneBoneRotation()
        {
			_oneBoneTargetRotation = Quaternion.LookRotation(targetPosition - rootBone.position);
			rootBone.rotation = Quaternion.Slerp(rootBone.rotation, _oneBoneTargetRotation, this.weight);
		}

		private float CosAngle(float a, float b, float c)
		{
			if (!float.IsNaN(Mathf.Acos((-(c * c) + (a * a) + (b * b)) / (-2 * a * b)) * Mathf.Rad2Deg))
				return Mathf.Acos((-(c * c) + (a * a) + (b * b)) / (2 * a * b)) * Mathf.Rad2Deg;
			else
				return 1;
		}
	}
}
