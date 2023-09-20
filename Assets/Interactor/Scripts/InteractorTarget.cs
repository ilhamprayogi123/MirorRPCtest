using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

namespace razz
{
    [HelpURL("https://negengames.com/interactor/components.html#interactortargetcs")]
    [DisallowMultipleComponent]
    public class InteractorTarget : MonoBehaviour
    {
        public InteractorObject intObj;
        public InteractorObject IntObj
        {
            get
            {
                if (!this) return null;
                if (!intObj) intObj = GetComponentInParent<InteractorObject>();
                return intObj;
            }
        }

        #region InteractorTargetVariables
        public Interactor.FullBodyBipedEffector effectorType;
        public bool setPosition = true;
        public bool setRotation = true;
        public bool matchChildBones = true;
        public InteractorTarget matchSource;
        public InteractorTarget MatchSource
        {
            get
            {
                if (!matchSource) matchSource = this;
                return matchSource;
            }
            set { matchSource = value; }
        }
        public Transform[] excludeFromBones;
        public bool overrideEffector;
        [Range(0, 360)]
        public float oHorizontalAngle = 0;
        [Range(-180, 180)]
        public float oHorizontalOffset = 0;
        [Range(0, 360)]
        public float oVerticalAngle = 0;
        [Range(-180, 180)]
        public float oVerticalOffset = 0;
        public float oMaxRange = 0.5f;
        public float oMinRange = 0;
        public Transform[] targetBones;
#if UNITY_EDITOR
        public int speedDebug = 1;
        public Vector3[] targetSegmentPoints;
        public bool asset;
#endif
        #endregion

        #region InteractorPathVariables
        [SerializeField]
        public List<PathSegment> pathSegments = new List<PathSegment>();
        public int targetCount //Total segment count to go to target
        {//Always returns target count
            get
            {
                if (backCount != 0)
                    return pathSegments.Count - backCount;
                else return pathSegments.Count;
            }
        }
        public int backCount; //Total segment count to go to back position
        public int AllCount //Target + Back segment count
        {//Returns with Back count if exist
            get { return pathSegments.Count; }
        }
        public bool bezierMode;
        public List<UnityEvent> endEvents;
        public List<Transform> followTransforms;
        public float targetTotalLength;
        public float backTotalLength;
        public bool resetAfterInteraction;
        public int pullSpeed = 5;
        public bool lerpBackwards = false;
        public BackPath backPath = BackPath.Same;
        public bool stationaryPoints = false;
        public bool dontScale = false;
        public bool stationaryStartTangents = false;
        public bool stationaryEndTangents = false;
        public bool lockTimings = false;
        public bool scaleChanged;
        public Vector3 storedFirstPosition;
        public bool interacting;
        public bool[] eventBools = new bool[0];
        public bool[] followBools = new bool[0];
        public bool[] matchParentBools = new bool[0];
        public bool matchParentRotation = false;
        private bool _matchParentToTarget, _matchParentToBack = false;
        public int callStartEvent = -1;

        public const int SegmentPointsPerSegment = 20;
        public Vector3[] segmentPoints;
        public bool segmentsValidated;

        private Quaternion _rotationOffset = Quaternion.identity;
        private Transform _thisTransform;
        private int _currentSegment;
        private float _currentSegmentLerp;
        private bool _toTarget = true;
        private float _lerpedLength;
        private Vector3 _bonePositionBeforeIK;

        private bool _backedUp;
        private Vector3[] _segmentPointsBackup;
        private PathSegment[] _pathSegmentsBackup;
        private int _backCountBackup;
        private bool _bezierModeBackup;
        private BackPath _backPathBackup;
        private Vector3 _storedFirstPositionBackup;
        private float[] _originalSegmentLengths;
        private float _interactedBackPathLength, _intBackTemp;

#endregion

        private void Start()
        {
            _thisTransform = this.transform;
            targetBones = new Transform[0];
            if (matchChildBones && effectorType != Interactor.FullBodyBipedEffector.Body)
            {
                if (!matchSource || matchSource.effectorType != this.effectorType || matchSource == this)
                {//If other target selected, no need for targetBones because its target bones will be used. If null, use this ones children by assinging them to targetBones.
                    matchSource = this;
                    targetBones = matchSource.GetComponentsInChildren<Transform>();
                    targetBones = ExcludedBones();
                }
            }
            _backedUp = false;

            //InteractorTargets created on 0.89 update check
            if (targetCount == 0) FirstInit();
        }
        private void OnDestroy()
        {
            if (_backedUp) LoadBackup();
            interacting = false;
            if (IntObj) IntObj.RemoveTargetFromChildTargets(this);
        }
        private void FirstInit()
        { //Normally targets added in the scene won't need this (after v0.89), old targets (but 
          //never selected in the editor until first interaction) in the scenes will need.
            bezierMode = false;
            pathSegments.Add(new InteractorTarget.PathSegment(new Vector3(0.2f, 0, 0), Vector3.zero, 0));
            backPath = InteractorTarget.BackPath.Same;
            pathSegments.Add(new InteractorTarget.PathSegment(Vector3.zero, new Vector3(0.2f, 0, 0), 0));
            backCount = 1;
            segmentPoints = new Vector3[SegmentPointsPerSegment * 2];
            storedFirstPosition = new Vector3(0.2f, 0, 0);
            CalculateAllLengths();
            string intObjName = "";
            if (IntObj) intObjName = (IntObj.gameObject.name + "'s \"");
            Debug.LogWarning(intObjName + this.gameObject.name + "\" InteractorTarget has created on v0.89. Please select this InteractorTarget in scene hierarchy (not in Play Mode) and save the scene.", this);
        }

        public Transform[] ExcludedBones(Transform[] targetBones = null)
        {//Call this as ExcludedBones() if you add new gameobject as child to exclude it in runtime
            if (targetBones == null)
            {
                if (!matchSource || matchSource.effectorType != this.effectorType) 
                    matchSource = this;
                targetBones = matchSource.targetBones;
            }
            if (excludeFromBones == null) return targetBones;
            //Remove excluded transfrom hierarchy from actual bone transforms
            if (excludeFromBones.Length > 0)
            {
                List<Transform> transformRemoval = new List<Transform>();

                for (int a = 0; a < excludeFromBones.Length; a++)
                {
                    Transform[] excludedTransforms;
                    if (excludeFromBones[a])
                    {
                        excludedTransforms = excludeFromBones[a].GetComponentsInChildren<Transform>();
                        transformRemoval.AddRange(excludedTransforms);
                    }
                }

                List<Transform> newTargetBones = new List<Transform>();
                for (int j = 0; j < targetBones.Length; j++)
                {
                    if (!transformRemoval.Contains(targetBones[j]))
                        newTargetBones.Add(targetBones[j]);
                }
                targetBones = newTargetBones.ToArray();
            }
            return targetBones;
        }

        public void SetMaxRange(float newRange)
        {
            oMaxRange = newRange;
        }

#region InteractorPath
        public Vector3 GetTargetPosition(float weight)
        {
            return GetPosition(weight, true);
        }
        public Vector3 GetBackPosition(float weight)
        {
            return GetPosition(weight, false);
        }
        private Vector3 GetPosition(float weight, bool toTarget)
        {
            if (targetCount == 0 || backCount == 0)
            {
                Debug.LogWarning("Path error, please reset this component and set again.", this);
                return Vector3.zero;
            }

            weight = Mathf.Clamp01(weight); //this directions lerp ratio
            _toTarget = toTarget; //current direction
            _currentSegment = 0; //current segment index
            _currentSegmentLerp = 0; //current segments lerp ratio
            _lerpedLength = 0; //current length on this direction
            float lengthCheck = 0;
            int start, end;
            if (interacting && lockTimings)
            {   //Sum again with original segment lengths before scaling path to effector bone.
                //Because we don't want segment times to change when points change unevenly. 
                //When only one segment scale, it will mess rest of timings up (because total time is always one on Custom Curve).
                //So instead we change that segments speed to fit same time 
                //by changing its length @GetSegmentLength() with original.
                CalculateAllLengths();
            }
            if (_toTarget)
            {
                _lerpedLength = weight * GetTargetLength();
                start = 0;
                end = targetCount;
            }
            else
            {
                _lerpedLength = (1f - weight) * GetBackLength();
                start = targetCount;
                end = AllCount;
            }

            for (int i = start; i < end; i++)
            {
                lengthCheck += GetSegmentLength(i);

                if (_lerpedLength <= lengthCheck)
                {
                    _currentSegmentLerp = (_lerpedLength - (lengthCheck - GetSegmentLength(i))) / GetSegmentLength(i);
                    _currentSegment = i;
                    break;
                }
            }

            if (interacting) ProcessEventsFollows();

            if (bezierMode && setRotation) SetRotationOffset();
            if (!setPosition) return _bonePositionBeforeIK;

            if (!bezierMode)
            {
                if (_toTarget)
                    return Vector3.Lerp(GetStartPosition(0), GetEndPosition(0), weight) + _thisTransform.position;
                else
                    return Vector3.Lerp(GetStartPosition(1), GetEndPosition(1), (1f - weight)) + _thisTransform.position;
            }
            if (backPath == BackPath.Straight && !_toTarget)
            {
                return Vector3.Lerp(GetStartPosition(targetCount), GetEndPosition(targetCount), (1f - weight)) + _thisTransform.position;
            }

            return GetBezierPosition(_currentSegment, _currentSegmentLerp) + _thisTransform.position;
        }
        public Quaternion GetRotation(Quaternion rot, float weight)
        {
            if (!setRotation) return rot;

            rot = Quaternion.Lerp(rot, this.transform.rotation, weight);
            return GetRotationOffset() * rot;
        }
        public void RotateChildren(Transform[] originalBones, float weight)
        {//First bone in the array is target itself. So we're skipping it.
            if (MatchSource.targetBones.Length < 2)
            {
                matchChildBones = false;
                return;
            }
            if (originalBones.Length != MatchSource.targetBones.Length)
            {
                Debug.LogWarning(this.gameObject.name + " bone count doesnt match with effector bone." + originalBones.Length + " " + MatchSource.targetBones.Length, MatchSource);
                return;
            }

            int boneLength = originalBones.Length;
            bool blend = false;
            if (_toTarget && _matchParentToTarget) blend = true;
            else if(!_toTarget && _matchParentToBack) blend = true;

            if (!blend)
            {
                for (int i = 1; i < boneLength; i++)
                {
                    originalBones[i].localRotation = Quaternion.Lerp(originalBones[i].localRotation, MatchSource.targetBones[i].localRotation, weight);
                }
                return;
            }

            Transform[] startBlend = originalBones;
            Transform[] endBlend = MatchSource.targetBones;
            
            int first, last, prev, next;
            float prevDist = 0;
            float nextDist = 0;
            if (_toTarget)
            {
                first = 0;
                last = targetCount - 1;
                prev = 0;
                next = targetCount - 1;
                nextDist = GetTargetLength();
            }
            else
            {
                first = targetCount;
                last = AllCount - 1;
                prev = targetCount;
                next = AllCount - 1;
                nextDist = GetBackLength();
                startBlend = MatchSource.targetBones;
                endBlend = originalBones;
            }

            if (_currentSegment != first)
            {
                for (int i = _currentSegment - 1; i >= first; i--)
                {
                    if (matchParentBools[i])
                    {
                        startBlend = pathSegments[i].blendMatchSource.MatchSource.targetBones;
                        prev = i + 1;
                        break;
                    }
                }
            }
            for (int i = _currentSegment; i < last; i++)
            {
                if (matchParentBools[i])
                {
                    endBlend = pathSegments[i].blendMatchSource.MatchSource.targetBones;
                    next = i;
                    break;
                }
            }

            for (int i = first; i < prev; i++)
            {
                prevDist += GetSegmentLength(i);
            }
            prevDist = _lerpedLength - prevDist;
            nextDist = nextDist - _lerpedLength;
            for (int i = last; i > next; i--)
            {
                nextDist -= GetSegmentLength(i);
            }
            float prevToNextDist = prevDist + nextDist;
            weight = prevDist / prevToNextDist;

            if (matchParentRotation)
                originalBones[0].rotation = Quaternion.Lerp(startBlend[0].rotation, endBlend[0].rotation, weight);
            
            for (int i = 1; i < boneLength; i++)
            {
                originalBones[i].localRotation = Quaternion.Lerp(startBlend[i].localRotation, endBlend[i].localRotation, weight);
            }
        }
        private void SetRotationOffset()
        {
            int startSegment = _currentSegment - 1;
            int endSegment = _currentSegment;

            if (!_toTarget)
            {
                if (backPath == BackPath.Same)
                {
                    startSegment = _currentSegment - 1 - targetCount;
                    endSegment = _currentSegment - targetCount;
                }
                else if (_currentSegment == targetCount)
                {
                    startSegment = -1;
                    endSegment = targetCount;
                }
            }

            if (!pathSegments[_currentSegment].bezier)
            {
                _rotationOffset = Quaternion.identity;
                return;
            }
            _rotationOffset = Quaternion.Lerp(GetEndRotationLocal(startSegment), GetEndRotationLocal(endSegment), _currentSegmentLerp);
        }
        private Quaternion GetRotationOffset()
        { //Needs to be called after GetPosition since _rotationOffset updates there
            if (_toTarget || backPath == BackPath.Same) return _rotationOffset;
            else return Quaternion.Inverse(_rotationOffset);
        }
        private Vector3 GetBezierPosition(int segment, float segmentLerp)
        {
            float segmentLerpPow2, SegmentLerpPow3, subtractedLerpVal, subtractedLerpValPow2, subtractedLerpValPow3;

            segmentLerpPow2 = segmentLerp * segmentLerp;
            SegmentLerpPow3 = segmentLerpPow2 * segmentLerp;
            subtractedLerpVal = 1 - segmentLerp;
            subtractedLerpValPow2 = subtractedLerpVal * subtractedLerpVal;
            subtractedLerpValPow3 = subtractedLerpValPow2 * subtractedLerpVal;

            return (subtractedLerpValPow3 * GetStartPosition(segment)) +
                           (3 * subtractedLerpValPow2 * segmentLerp * GetStartTangent(segment)) +
                           (3 * subtractedLerpVal * segmentLerpPow2 * GetEndTangent(segment)) +
                           (SegmentLerpPow3 * GetEndPosition(segment));
        }
        public float BackPathSpeed()
        { //Increase elapsed time smoothly if backPath gets longer and longer
            float currentBackLength = GetBackLength();
            float ratio = currentBackLength / _interactedBackPathLength;
            float pullThreshold = 1f + pullSpeed * 0.00001f;

            if (ratio > pullThreshold * _intBackTemp)
            {
                _intBackTemp = ratio;
                return ratio;
            }
            else return 1f;
        }

        public void SetSegmentDirty(int segment)
        {
            if (!SegmentExist(segment)) return;

            pathSegments[segment].segmentChanged = true;
            segmentsValidated = false;
        }
        private void SetAllSegmentsDirty()
        {
            for (int i = 0; i < AllCount; i++)
            {
                SetSegmentDirty(i);
            }
        }
        public float GetSegmentLength(int segment) //Segment out of array error debug with negative
        {
            if (!SegmentExist(segment)) return -1000;

            if (interacting && lockTimings && Application.isPlaying)
                return _originalSegmentLengths[segment];

            if (!pathSegments[segment].segmentChanged)
                return pathSegments[segment].length;
            return CalculateSegmentLength(segment);
        }
        private float CalculateSegmentLength(int segment)
        {
            pathSegments[segment].length = 0;
            UpdateSegmentPoints(segment);
            for (int i = 0; i < SegmentPointsPerSegment - 1; i++)
            {
                pathSegments[segment].length += Vector3.Distance(segmentPoints[i + (SegmentPointsPerSegment * segment)], segmentPoints[i + (SegmentPointsPerSegment * segment) + 1]);
            }
            pathSegments[segment].segmentChanged = false;
            return pathSegments[segment].length;
        }
        private void UpdateSegmentPoints(int segment)
        {
            int start = SegmentPointsPerSegment * segment;
            int end = SegmentPointsPerSegment * (segment + 1);
            float interval = 1 / (float)(SegmentPointsPerSegment - 1f);
            if (!_thisTransform) _thisTransform = this.transform;
            Vector3 currentWorldPosition = _thisTransform.position;

            for (int i = start; i < end; i++)
            {
                if (!pathSegments[segment].bezier)
                    segmentPoints[i] = Vector3.Lerp(GetStartPosition(segment), GetEndPosition(segment), (float)(i - start) * interval) + currentWorldPosition;
                else
                    segmentPoints[i] = GetBezierPosition(segment, (float)(i - start) * interval) + currentWorldPosition;
            }
        }
        private void CalculateAllLengths()
        {
            if (scaleChanged) TranslatePath();

            targetTotalLength = 0;
            backTotalLength = 0;
            for (int i = 0; i < AllCount; i++)
            {
                if (i >= targetCount)
                    backTotalLength += GetSegmentLength(i);
                else
                    targetTotalLength += GetSegmentLength(i);
            }
        }
        public float GetTargetLength()
        {
            if (targetCount <= 0) return -2000; //Segment array is empty error

            for (int i = 0; i < targetCount; i++)
            {
                if (pathSegments[i].segmentChanged)
                {
                    CalculateAllLengths();
                    break;
                }
            }
            return targetTotalLength;
        }
        public float GetBackLength()
        {
            if (targetCount <= 0) return -2000; //Segment array is empty error
            if (backCount <= 0) return -3000; //GoToBack array is empty error

            for (int i = targetCount; i < AllCount; i++)
            {
                if (pathSegments[i].segmentChanged)
                {
                    CalculateAllLengths();
                    break;
                }
            }
            return backTotalLength;
        }

        public void PrepareTarget(Vector3 bonePosition)
        {//Called on interaction start
            SaveBackup();
            //Event & FollowTransform setup
            interacting = true;
            CheckEvents();
            CheckFollows();
            CheckMatchParents();
            //Set first position
            SetStartPositionWorld(0, bonePosition);
            _interactedBackPathLength = GetBackLength();
            _intBackTemp = 1f;
        }
        private void CheckEvents()
        {
            if (callStartEvent == -1)
            {
                if (endEvents == null || endEvents.Count == 0) return;
            }

            int events = 0;
            if (callStartEvent > -1) events++;
            for (int i = 0; i < AllCount; i++)
            {
                if (pathSegments[i].callEndEvent > -1)
                    events++;
            }
            if (events == 0)
            {
                endEvents = null;
                return;
            }

            eventBools = new bool[AllCount + 1];
            if (callStartEvent > -1) eventBools[AllCount] = true;
            for (int i = 0; i < AllCount; i++)
            {
                if (pathSegments[i].callEndEvent > -1)
                    eventBools[i] = true;
            }
        }
        private void CheckFollows()
        {
            if (followTransforms == null || followTransforms.Count == 0) return;

            int follows = 0;
            for (int i = 0; i < AllCount; i++)
            {
                if (pathSegments[i].followTransform > -1 && followTransforms[pathSegments[i].followTransform] != null)
                    follows++;
            }
            if (follows == 0)
            {
                followTransforms = null;
                return;
            }

            followBools = new bool[AllCount];
            for (int i = 0; i < AllCount; i++)
            {
                if (pathSegments[i].followTransform > -1)
                    followBools[i] = true;
            }
        }
        private void CheckMatchParents()
        {
            int matchParents = 0;
            matchParentBools = new bool[AllCount];
            for (int i = 0; i < AllCount; i++)
            {
                if (pathSegments[i].blendMatchSource != null)
                {
                    matchParentBools[i] = true;
                    matchParents++;
                }
            }

            if (matchParents == 0)
            {
                matchParentBools = new bool[0];
            }
            else
            {
                _matchParentToTarget = false;
                for (int i = 0; i < targetCount; i++)
                {
                    if (matchParentBools[i])
                    {
                        _matchParentToTarget = true;
                        break;
                    }
                }

                _matchParentToBack = false;
                for (int i = targetCount; i < AllCount; i++)
                {
                    if (matchParentBools[i])
                    {
                        _matchParentToBack = true;
                        break;
                    }
                }
            }
        }
        private void ProcessEventsFollows()
        {
            if (followTransforms != null && followTransforms.Count != 0 && _currentSegment < followBools.Length)
            {
                if (followBools[_currentSegment] == true)
                {
                    int index = pathSegments[_currentSegment].followTransform;
                    if (followTransforms[index] != null && followTransforms[index].position != GetEndPositionWorld(_currentSegment))
                    {
                        SetMidEndPoint(_currentSegment, followTransforms[index].position, true);
                    }
                }

                if (_currentSegment > 0 && followBools[_currentSegment - 1] == true)
                {
                    int index = pathSegments[_currentSegment - 1].followTransform;
                    if (followTransforms[index] != null && followTransforms[index].position != GetEndPositionWorld(_currentSegment - 1))
                    {
                        SetMidEndPoint(_currentSegment - 1, followTransforms[index].position, true);
                    }
                }
            }

            if (endEvents != null && endEvents.Count != 0)
            {//Checking events in a few possible frames
                if (eventBools[AllCount])
                {
                    endEvents[callStartEvent]?.Invoke();
                    eventBools[AllCount] = false;
                }
                if (_currentSegment > 0 && eventBools[_currentSegment - 1] == true)
                {//This can be a few frames later
                    int index = pathSegments[_currentSegment - 1].callEndEvent;
                    endEvents[index]?.Invoke();
                    eventBools[_currentSegment - 1] = false;
                }
                if (eventBools[_currentSegment] == true && _currentSegmentLerp > 0.99f)
                {//This can be a few frames before
                    int index = pathSegments[_currentSegment].callEndEvent;
                    endEvents[index]?.Invoke();
                    eventBools[_currentSegment] = false;
                }
            }
        }
        public void AddFollowTransform(Transform follow, int segment)
        {//Adds follow transform for existing segment in runtime (needs to be added before interaction)
            if (!SegmentExist(segment)) return;
            if (follow == null) return;
            if (followTransforms == null) followTransforms = new List<Transform>();

            followTransforms.Add(follow);
            pathSegments[segment].followTransform = segment;
        }
        private void SaveBackup()
        {
            if (_backedUp) return;

            _segmentPointsBackup = new Vector3[segmentPoints.Length];
            _segmentPointsBackup = segmentPoints;
            _pathSegmentsBackup = new PathSegment[AllCount];
            _originalSegmentLengths = new float[AllCount];
            for (int i = 0; i < AllCount; i++)
            {
                PathSegment temp;
                if (pathSegments[i].bezier)
                {
                    temp = new PathSegment(pathSegments[i].startPosition, pathSegments[i].endPosition, pathSegments[i].startTangent, pathSegments[i].endTangent, pathSegments[i].endRotation, pathSegments[i].length);
                }
                else
                {
                    temp = new PathSegment(pathSegments[i].startPosition, pathSegments[i].endPosition, pathSegments[i].length);
                }
                temp.callEndEvent = pathSegments[i].callEndEvent;
                temp.followTransform = pathSegments[i].followTransform;
                temp.blendMatchSource = pathSegments[i].blendMatchSource;
                _pathSegmentsBackup[i] = temp;
                _originalSegmentLengths[i] = pathSegments[i].length;
            }
            _backCountBackup = backCount;
            _bezierModeBackup = bezierMode;
            _backPathBackup = backPath;
            _storedFirstPositionBackup = storedFirstPosition;
            _backedUp = true;
        }
        private void LoadBackup()
        {
            segmentPoints = new Vector3[_segmentPointsBackup.Length];
            segmentPoints = _segmentPointsBackup;
            pathSegments.Clear();
            for (int i = 0; i < _pathSegmentsBackup.Length; i++)
            {
                PathSegment temp;
                if (_pathSegmentsBackup[i].bezier)
                {
                    temp = new PathSegment(_pathSegmentsBackup[i].startPosition, _pathSegmentsBackup[i].endPosition, _pathSegmentsBackup[i].startTangent, _pathSegmentsBackup[i].endTangent, _pathSegmentsBackup[i].endRotation, _pathSegmentsBackup[i].length);
                }
                else
                {
                    temp = new PathSegment(_pathSegmentsBackup[i].startPosition, _pathSegmentsBackup[i].endPosition, _pathSegmentsBackup[i].length);
                }

                temp.callEndEvent = _pathSegmentsBackup[i].callEndEvent;
                temp.followTransform = _pathSegmentsBackup[i].followTransform;
                temp.blendMatchSource = _pathSegmentsBackup[i].blendMatchSource;
                pathSegments.Add(temp);
            }
            backCount = _backCountBackup;
            bezierMode = _bezierModeBackup;
            backPath = _backPathBackup;
            storedFirstPosition = _storedFirstPositionBackup;
            SetAllSegmentsDirty();
            CalculateAllLengths();
        }
        public void EndTarget()
        {//Called when on successful interaction end
            interacting = false;
            if (resetAfterInteraction) LoadBackup();
        }
        public void ResetTarget()
        {
            interacting = false;
            if (resetAfterInteraction) LoadBackup();
        }
        public void UpdateFirstAndLastPosition(Vector3 bonePositionBeforeIK)
        {//Called when on first target segment or last back segment (in case of player or target movement for first seg) (bone will always move for last seg)
            _bonePositionBeforeIK = bonePositionBeforeIK;
            bool temp = stationaryPoints;
            stationaryPoints = true;
            SetStartPositionWorld(0, bonePositionBeforeIK);
            stationaryPoints = temp;
        }

        public void TranslatePath()
        {//Rotates and scales path points
            if (!scaleChanged) return;
            if (stationaryPoints)
            {
                CalculateRotation();
                if (stationaryStartTangents)
                {
                    SetStartTangent(0, GetStartTangent(0) + GetStartPosition(0) - storedFirstPosition);
                }

                if (backCount > 0)
                {
                    SetEndPosition(AllCount - 1, GetStartPosition(0));
                    if (stationaryStartTangents)
                    {
                        SetEndTangent(AllCount - 1, GetEndTangent(AllCount - 1) + GetStartPosition(0) - storedFirstPosition);
                    }
                    SetSegmentDirty(AllCount - 1);
                }
                storedFirstPosition = GetStartPosition(0);
                SetSegmentDirty(0);
                return;
            }

            Quaternion rotationChange = CalculateRotation();
            Matrix4x4 transformMatrix = Matrix4x4.Rotate(rotationChange);
            //float ratio = GetStartPosition(0).magnitude / storedFirstPosition.magnitude;
            Vector3 rotatedFirstPos = transformMatrix.MultiplyPoint3x4(storedFirstPosition);
            float ratio = GetStartPosition(0).magnitude / rotatedFirstPos.magnitude;
            Vector3 newPosition;
            if (dontScale) ratio = 1f;

            if (!stationaryStartTangents)
                SetStartTangent(0, transformMatrix.MultiplyPoint3x4(GetStartTangent(0)) * ratio);
            else
                SetStartTangent(0, GetStartTangent(0) + GetStartPosition(0) - storedFirstPosition);
            if (targetCount == 1 && !stationaryEndTangents)
                SetEndTangent(0, transformMatrix.MultiplyPoint3x4(GetEndTangent(0)) * ratio);
            Vector3 lastBackTan = GetEndTangent(AllCount - 1) + GetStartPosition(0) - storedFirstPosition;
            if (backCount == 1 && stationaryStartTangents)
            {
                lastBackTan = GetEndTangent(AllCount - 1) + GetStartPosition(0) - storedFirstPosition;
            }
            for (int i = 1; i < AllCount; i++)
            {
                if (i >= targetCount)
                {
                    newPosition = transformMatrix.MultiplyPoint3x4(GetEndPosition(i)) * ratio;
                    SetEndPosition(i, newPosition);
                    SetStartPosition(i + 1, newPosition);
                    SetEndTangent(i, transformMatrix.MultiplyPoint3x4(GetEndTangent(i)) * ratio);
                    SetStartTangent(i + 1, transformMatrix.MultiplyPoint3x4(GetStartTangent(i + 1)) * ratio);
                    SetSegmentDirty(i + 1);
                    SetSegmentDirty(i);

                    if (i == targetCount && backCount != 1 && !stationaryEndTangents)
                        SetStartTangent(targetCount, transformMatrix.MultiplyPoint3x4(GetStartTangent(targetCount)) * ratio);
                }
                else
                {
                    newPosition = transformMatrix.MultiplyPoint3x4(GetStartPosition(i)) * ratio;
                    SetStartPosition(i, newPosition);
                    SetEndPosition(i - 1, newPosition);
                    SetStartTangent(i, transformMatrix.MultiplyPoint3x4(GetStartTangent(i)) * ratio);
                    SetEndTangent(i - 1, transformMatrix.MultiplyPoint3x4(GetEndTangent(i - 1)) * ratio);
                    SetSegmentDirty(i - 1);
                    SetSegmentDirty(i);

                    if (i == targetCount - 1 && targetCount != 1 && !stationaryEndTangents)
                        SetEndTangent(targetCount - 1, transformMatrix.MultiplyPoint3x4(GetEndTangent(targetCount - 1)) * ratio);
                }
            }

            if (backCount > 0)
            {
                SetEndPosition(AllCount - 1, GetStartPosition(0));
                if (backCount == 1 && !stationaryEndTangents)
                    SetStartTangent(AllCount - 1, transformMatrix.MultiplyPoint3x4(GetStartTangent(AllCount - 1)) * ratio);
                if (backCount == 1 && stationaryStartTangents)
                    SetEndTangent(AllCount - 1, lastBackTan);
                SetSegmentDirty(AllCount - 1);
            }
            storedFirstPosition = GetStartPosition(0);
            SetSegmentDirty(0);
            scaleChanged = false;
        }
        private Quaternion CalculateRotation()
        { //Calculates two rotations for yaw and pitch differences and prevents unwanted roll of path without any axis input
            Vector3 oldDir = storedFirstPosition.normalized;
            Vector3 newDir = GetStartPosition(0).normalized;
            Vector3 oldDirH = Vector3.ProjectOnPlane(oldDir, Vector3.up).normalized;
            Vector3 newDirH = Vector3.ProjectOnPlane(newDir, Vector3.up).normalized;

            float yAngle = Vector3.SignedAngle(oldDirH, newDirH, Vector3.up);
            Quaternion yRot = Quaternion.AngleAxis(yAngle, Vector3.up); //rot need for around y axis

            Vector3 oldDirYrotated = yRot * oldDir;
            Vector3 horizontalAxis = Vector3.Cross(oldDirYrotated, newDir);
            float horizontalAngle = Vector3.SignedAngle(oldDirYrotated, newDir, horizontalAxis);
            Quaternion horizontalRot = Quaternion.AngleAxis(horizontalAngle, horizontalAxis); //rot needed for around horizontal axis

            Quaternion totalRot = horizontalRot * yRot;
            RedirectEndRotations(totalRot);
            return totalRot;
        }
        private void RedirectEndRotations(Quaternion rot)
        { //Since endRotations are in world space, they need to be redirected to new path direction.
            for (int i = 0; i < AllCount - 1; i++)
            {
                if (i == targetCount - 1) continue;
                if (!pathSegments[i].bezier) continue;

                SetEndRotationWorld(i, rot * GetEndRotationWorld(i));
            }
        }

        public bool SetEndRotationWorld(int segment, Quaternion rotation)
        {
            if (!SegmentExist(segment)) return false;

            if (segment < targetCount && backPath == BackPath.Same)
                pathSegments[AllCount - 2 - segment].endRotation = rotation;

            pathSegments[segment].endRotation = rotation;
            return true;
        }
        public Quaternion GetPathDirection(int segment)
        {
            if (!SegmentExist(segment)) return Quaternion.identity;

            Vector3 rotOffsetForward = -GetStartPosition(0).normalized;
            if (segment >= targetCount)
                rotOffsetForward = GetStartPosition(0).normalized;
            Quaternion xRot = Quaternion.FromToRotation(rotOffsetForward, Vector3.up);
            Vector3 rotOffsetUp = xRot * rotOffsetForward;
            Quaternion rotOffset = Quaternion.identity;
            if (rotOffsetForward != Vector3.zero && rotOffsetUp != Vector3.zero)
                rotOffset = Quaternion.LookRotation(rotOffsetForward, rotOffsetUp);
            return rotOffset;
        }
        public Quaternion GetEndRotationWorld(int segment)
        {
            if (!SegmentExist(segment)) return Quaternion.identity;

            return pathSegments[segment].endRotation;
        }
        public Quaternion GetEndRotationLocal(int segment)
        {
            if (!SegmentExist(segment)) return Quaternion.identity;

            if (segment == targetCount - 1 || segment == AllCount - 1)
                return Quaternion.identity;

            Quaternion pathDir = GetPathDirection(segment);
            if (pathSegments[segment].endRotation == pathDir)
                return Quaternion.identity;

            return pathSegments[segment].endRotation * Quaternion.Inverse(pathDir);
        }

        public bool SetStartPosition(int segment, Vector3 position)
        {
            if (!SegmentExist(segment)) return false;

            if (segment == 0) storedFirstPosition = GetStartPosition(0);
            pathSegments[segment].startPosition = position;
            if (segment == 0)
            {
                if (storedFirstPosition == GetStartPosition(0)) return false;

                scaleChanged = true;
                TranslatePath();
            }
            return true;
        }
        public bool SetStartPositionWorld(int segment, Vector3 position)
        {
            if (!SegmentExist(segment)) return false;

            if (segment == 0) storedFirstPosition = GetStartPosition(0);

            if (!_thisTransform) _thisTransform = this.transform;
            pathSegments[segment].startPosition = position - _thisTransform.position;
            if (segment == 0)
            {
                if (storedFirstPosition == GetStartPosition(0)) return false;

                scaleChanged = true;
                TranslatePath();
            }
            return true;
        }
        public bool SetMidStartPoint(int segment, Vector3 position, bool world)
        {
            if (!SegmentExist(segment)) return false;

            if (segment != 0 && segment != targetCount)
            {
                if (world)
                {
                    if (!_thisTransform) _thisTransform = this.transform;
                    position = position - _thisTransform.position;
                }
                Vector3 change = position - GetStartPosition(segment);

                pathSegments[segment].startPosition = position;
                SetEndPosition(segment - 1, position);
                SetStartTangent(segment, GetStartTangent(segment) + change);
                SetEndTangent(segment - 1, GetEndTangent(segment - 1) + change);
                SetSegmentDirty(segment);
                SetSegmentDirty(segment - 1);
                return true;
            }
            else return false;
        }
        public bool SetEndPosition(int segment, Vector3 position)
        {
            if (!SegmentExist(segment)) return false;

            pathSegments[segment].endPosition = position;
            return true;
        }
        public bool SetEndPositionWorld(int segment, Vector3 position)
        {
            if (!SegmentExist(segment)) return false;

            if (!_thisTransform) _thisTransform = this.transform;
            pathSegments[segment].endPosition = position - _thisTransform.position;
            return true;
        }
        public bool SetMidEndPoint(int segment, Vector3 position, bool world)
        {
            if (!SegmentExist(segment)) return false;

            if (segment != targetCount - 1 && segment != AllCount - 1)
            {
                if (world)
                {
                    if (!_thisTransform) _thisTransform = this.transform;
                    position = position - _thisTransform.position;
                }
                Vector3 change = position - GetEndPosition(segment);

                pathSegments[segment].endPosition = position;
                SetStartPosition(segment + 1, position);
                SetEndTangent(segment, GetEndTangent(segment) + change);
                SetStartTangent(segment + 1, GetStartTangent(segment + 1) + change);
                SetSegmentDirty(segment);
                SetSegmentDirty(segment + 1);
                return true;
            }
            else return false;
        }
        public bool SetStartTangent(int segment, Vector3 position)
        {
            if (!SegmentExist(segment)) return false;

            pathSegments[segment].startTangent = position;
            return true;
        }
        public bool SetStartTangentWorld(int segment, Vector3 position)
        {
            if (!SegmentExist(segment)) return false;

            if (!_thisTransform) _thisTransform = this.transform;
            pathSegments[segment].startTangent = position - _thisTransform.position;
            return true;
        }
        public bool SetEndTangent(int segment, Vector3 position)
        {
            if (!SegmentExist(segment)) return false;

            pathSegments[segment].endTangent = position;
            return true;
        }
        public bool SetEndTangentWorld(int segment, Vector3 position)
        {
            if (!SegmentExist(segment)) return false;

            if (!_thisTransform) _thisTransform = this.transform;
            pathSegments[segment].endTangent = position - _thisTransform.position;
            return true;
        }

        public Vector3 GetStartPosition(int segment)
        {
            if (!SegmentExist(segment)) return Vector3.zero;

            return pathSegments[segment].startPosition;
        }
        public Vector3 GetStartPositionWorld(int segment)
        {
            if (!SegmentExist(segment)) return Vector3.zero;

            if (!_thisTransform) _thisTransform = this.transform;
            return pathSegments[segment].startPosition + _thisTransform.position;
        }
        public Vector3 GetEndPosition(int segment)
        {
            if (!SegmentExist(segment)) return Vector3.zero;

            return pathSegments[segment].endPosition;
        }
        public Vector3 GetEndPositionWorld(int segment)
        {
            if (!SegmentExist(segment)) return Vector3.zero;

            if (!_thisTransform) _thisTransform = this.transform;
            return pathSegments[segment].endPosition + _thisTransform.position;
        }
        public Vector3 GetStartTangent(int segment)
        {
            if (!SegmentExist(segment)) return Vector3.zero;

            return pathSegments[segment].startTangent;
        }
        public Vector3 GetStartTangentWorld(int segment)
        {
            if (!SegmentExist(segment)) return Vector3.zero;

            if (!_thisTransform) _thisTransform = this.transform;
            return pathSegments[segment].startTangent + _thisTransform.position;
        }
        public Vector3 GetEndTangent(int segment)
        {
            if (!SegmentExist(segment)) return Vector3.zero;

            return pathSegments[segment].endTangent;
        }
        public Vector3 GetEndTangentWorld(int segment)
        {
            if (!SegmentExist(segment)) return Vector3.zero;

            if (!_thisTransform) _thisTransform = this.transform;
            return pathSegments[segment].endTangent + _thisTransform.position;
        }

        public bool SegmentExist(int segment)
        {
            if (AllCount > segment && segment >= 0)
            {
                return true;
            }
            return false;
        }

        [Serializable]
        public class PathSegment
        {//These values are local, need to be relative to this.transform when used
            public Vector3 startPosition; //Local
            public Vector3 endPosition; //Local
            public Vector3 startTangent; //Local
            public Vector3 endTangent; //Local
            public Quaternion endRotation; //World

            public bool bezier;
            public bool segmentChanged;
            public float length;
            public int callEndEvent;
            public int followTransform;
            public InteractorTarget blendMatchSource = null;

            public PathSegment(Vector3 startPosition, Vector3 endPosition, float length)
            {
                this.startPosition = startPosition;
                this.endPosition = endPosition;
                this.bezier = false;
                this.length = length;
                this.callEndEvent = -1;
                this.followTransform = -1;
                this.segmentChanged = true;
            }
            public PathSegment(Vector3 startPosition, Vector3 endPosition, Vector3 startTangent, Vector3 endTangent, Quaternion endRotation, float length)
            {
                this.startPosition = startPosition;
                this.endPosition = endPosition;
                this.startTangent = startTangent;
                this.endTangent = endTangent;
                this.endRotation = endRotation;
                this.bezier = true;
                this.length = length;
                this.callEndEvent = -1;
                this.followTransform = -1;
                this.segmentChanged = true;
            }
        }

        public enum TargetPath
        {
            Straight,
            Bezier
        }

        public enum BackPath
        {
            Straight,
            Same,
            Seperate
        }
#endregion
    }
}
