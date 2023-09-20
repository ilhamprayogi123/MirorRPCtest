using UnityEngine;
using System.Collections;

namespace razz
{
    [HelpURL("https://negengames.com/interactor/components.html#automovercs")]
    [DisallowMultipleComponent]
    public class AutoMover : MonoBehaviour
    {
        private float _rot = -1f;

        public enum MoveType { Once, Loop, Wave }
        public enum Reverse { Default = 1, Reversed = -1 }
        public MoveType moveType;
        [Tooltip("Total duration for Loop or Wave move types")]
        public float totalDuration = 0;
        public EaseType positionEase;
        public EaseType rotationEase;
        public Reverse leftRight;
        public float duration;
        [Range(0, 1)] public float offset;
        public Vector3 fromPosition;
        public Vector3 toPosition;
        public Vector3 fromRotation;
        public Vector3 toRotation;
        public bool reversable;

        [HideInInspector] public bool started;
        [HideInInspector] public bool half;
        [HideInInspector] public bool ended;
        [HideInInspector] public bool reverse;

        private float _passedTotalDuraion;
        private bool _done;

        public void StartMovement(float y)
        {
            if (_rot < 0)
            {
                _rot = y;
                return;
            }

            if (leftRight == Reverse.Reversed)
            {
                if (y < _rot)
                {
                    reverse = true;

                    if (fromPosition != toPosition)
                        StartCoroutine(Position());
                    if (fromRotation != toRotation)
                        StartCoroutine(Rotation());

                    _rot = y;
                }
                else if (y > _rot)
                {
                    reverse = false;

                    if (fromPosition != toPosition)
                        StartCoroutine(Position());
                    if (fromRotation != toRotation)
                        StartCoroutine(Rotation());

                    _rot = y;
                }
            }
            else
            {
                if (y > _rot)
                {
                    reverse = true;

                    if (fromPosition != toPosition)
                        StartCoroutine(Position());
                    if (fromRotation != toRotation)
                        StartCoroutine(Rotation());

                    _rot = y;
                }
                else if (y < _rot)
                {
                    reverse = false;

                    if (fromPosition != toPosition)
                        StartCoroutine(Position());
                    if (fromRotation != toRotation)
                        StartCoroutine(Rotation());

                    _rot = y;
                }
            }
        }

        public void StartMovement()
        {
            ended = false;
            started = true;

            if (fromPosition != toPosition)
                StartCoroutine(Position());
            if (fromRotation != toRotation)
                StartCoroutine(Rotation());

            if (reversable)
                reverse = !reverse;
        }

        public void MovementHalf()
        {
            half = true;
        }

        public void MovementEnd()
        {
            ended = true;
        }

        public void ResetBools()
        {
            ended = false;
            half = false;
            started = false;
            _done = false;
            _passedTotalDuraion = 0;

            if (!reversable)
            {
                transform.localPosition = fromPosition;
                transform.localRotation = Quaternion.Euler(fromRotation);
            }
        }

        IEnumerator Position()
        {
            var from = fromPosition;
            var to = toPosition;

            if (moveType == MoveType.Once)
            {
                if (!reverse)
                {
                    StartCoroutine(Auto.MoveTo(this.transform, to, duration, Ease.FromType(positionEase), this));
                }
                else
                {
                    StartCoroutine(Auto.MoveTo(this.transform, from, duration, Ease.FromType(positionEase), this));
                }
                yield return 0;
            }
            else if (moveType == MoveType.Loop)
            {
                while (_passedTotalDuraion < totalDuration)
                {
                    _passedTotalDuraion += Time.deltaTime;
                    transform.localPosition = Auto.Loop(duration, from, to, offset);
                    yield return 0;
                }
                _done = true;
            }
            else if (moveType == MoveType.Wave)
            {
                while (_passedTotalDuraion < totalDuration)
                {
                    _passedTotalDuraion += Time.deltaTime;
                    transform.localPosition = Auto.Wave(duration, from, to, offset);
                    yield return 0;
                }
                _done = true;
            }
        }

        IEnumerator Rotation()
        {
            var from = Quaternion.Euler(fromRotation);
            var to = Quaternion.Euler(toRotation);

            if (moveType == MoveType.Once)
            {
                if (!reverse)
                {
                    StartCoroutine(Auto.RotateTo(this.transform, to, duration, Ease.FromType(rotationEase)));
                }
                else
                {
                    StartCoroutine(Auto.RotateTo(this.transform, from, duration, Ease.FromType(rotationEase)));
                }
                yield return 0;
            }
            else if (moveType == MoveType.Loop)
            {
                while (!_done)
                {
                    transform.localRotation = Auto.Loop(duration, from, to, offset);
                    yield return 0;
                }
                MovementEnd();
            }
            else if (moveType == MoveType.Wave)
            {
                while (!_done)
                {
                    transform.localRotation = Auto.Wave(duration, from, to, offset);
                    yield return 0;
                }
                MovementEnd();
            }
        }
    }
}
