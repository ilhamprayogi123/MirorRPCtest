using UnityEngine;
using System.Collections;

namespace razz
{
    public delegate bool Predicate();
    public delegate float Easer(float t, AnimationCurve animationCurve = null);

    //Main class for changing transform(position or rotation) in time with or without Coroutines
    public static class Auto
    {
        #region Transform Coroutines
        public static IEnumerator MoveTo(this Transform transform, Vector3 target, float duration, Easer ease)
        {
            float elapsed = 0;
            var start = transform.localPosition;
            var range = target - start;

            while (elapsed < duration)
            {
                elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
                transform.localPosition = start + range * ease(elapsed / duration);
                yield return 0;
            }

            transform.localPosition = target;
        }

        public static IEnumerator MoveTo(this Transform transform, Vector3 target, float duration, Easer ease, AutoMover _script)
        {
            float elapsed = 0;
            var start = transform.localPosition;
            var range = target - start;
            bool halfDone = false;

            while (elapsed < duration)
            {
                elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
                transform.localPosition = start + range * ease(elapsed / duration);
                if (elapsed > duration * 0.5f && !halfDone)
                {
                    _script.MovementHalf();
                    halfDone = true;
                }
                yield return 0;
            }

            if (transform.localPosition == target)
            {
                _script.MovementEnd();
            }

            transform.localPosition = target;
        }

        public static IEnumerator MoveTo(this Transform transform, Vector3 target, float duration, EaseType ease, AutoMover _script)
        {
            return MoveTo(transform, target, duration, ease, _script);
        }

        public static IEnumerator MoveTo(this Transform transform, Vector3 target, float duration)
        {
            return MoveTo(transform, target, duration, Ease.Linear);
        }

        public static IEnumerator MoveTo(this Transform transform, Vector3 target, float duration, EaseType ease)
        {
            return MoveTo(transform, target, duration, Ease.FromType(ease));
        }

        public static IEnumerator MoveFrom(this Transform transform, Vector3 target, float duration, Easer ease)
        {
            var start = transform.localPosition;
            transform.localPosition = target;

            return MoveTo(transform, start, duration, ease);
        }

        public static IEnumerator MoveFrom(this Transform transform, Vector3 target, float duration)
        {
            return MoveFrom(transform, target, duration, Ease.Linear);
        }
        public static IEnumerator MoveFrom(this Transform transform, Vector3 target, float duration, EaseType ease)
        {
            return MoveFrom(transform, target, duration, Ease.FromType(ease));
        }

        public static IEnumerator ScaleTo(this Transform transform, Vector3 target, float duration, Easer ease)
        {
            float elapsed = 0;
            var start = transform.localScale;
            var range = target - start;

            while (elapsed < duration)
            {
                elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
                transform.localScale = start + range * ease(elapsed / duration);
                yield return 0;
            }

            transform.localScale = target;
        }

        public static IEnumerator ScaleTo(this Transform transform, Vector3 target, float duration)
        {
            return ScaleTo(transform, target, duration, Ease.Linear);
        }

        public static IEnumerator ScaleTo(this Transform transform, Vector3 target, float duration, EaseType ease)
        {
            return ScaleTo(transform, target, duration, Ease.FromType(ease));
        }

        public static IEnumerator ScaleFrom(this Transform transform, Vector3 target, float duration, Easer ease)
        {
            var start = transform.localScale;
            transform.localScale = target;

            return ScaleTo(transform, start, duration, ease);
        }

        public static IEnumerator ScaleFrom(this Transform transform, Vector3 target, float duration)
        {
            return ScaleFrom(transform, target, duration, Ease.Linear);
        }

        public static IEnumerator ScaleFrom(this Transform transform, Vector3 target, float duration, EaseType ease)
        {
            return ScaleFrom(transform, target, duration, Ease.FromType(ease));
        }

        public static IEnumerator RotateToGlobal(this Transform transform, Quaternion target, float duration, Easer ease)
        {
            float elapsed = 0;
            var start = transform.rotation;

            while (elapsed < duration)
            {
                elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
                transform.rotation = Quaternion.Lerp(start, target, ease(elapsed / duration));
                yield return 0;
            }

            transform.rotation = target;
        }

        public static IEnumerator RotateTo(this Transform transform, Quaternion target, float duration, Easer ease)
        {
            float elapsed = 0;
            var start = transform.localRotation;

            while (elapsed < duration)
            {
                elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
                transform.localRotation = Quaternion.Lerp(start, target, ease(elapsed / duration));
                yield return 0;
            }

            transform.localRotation = target;
        }

        public static IEnumerator RotateToGlobal(this Transform transform, Quaternion target, float duration, Easer ease, TurretAim source)
        {
            float elapsed = 0;
            var start = transform.rotation;

            while (elapsed < duration)
            {
                elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
                transform.rotation = Quaternion.Lerp(start, target, ease(elapsed / duration));
                yield return 0;
            }

            source.locked = true;
            transform.rotation = target;
        }

        public static IEnumerator RotateFrom(this Transform transform, Quaternion target, float duration, Easer ease)
        {
            var start = transform.localRotation;
            transform.localRotation = target;

            return RotateTo(transform, start, duration, ease);
        }

        public static IEnumerator RotateFrom(this Transform transform, Quaternion target, float duration)
        {
            return RotateFrom(transform, target, duration, Ease.Linear);
        }

        public static IEnumerator RotateFrom(this Transform transform, Quaternion target, float duration, EaseType ease)
        {
            return RotateFrom(transform, target, duration, Ease.FromType(ease));
        }

        public static IEnumerator CurveTo(this Transform transform, Vector3 control, Vector3 target, float duration, Easer ease)
        {
            float elapsed = 0;
            var start = transform.localPosition;
            Vector3 position;
            float t;

            while (elapsed < duration)
            {
                elapsed = Mathf.MoveTowards(elapsed, duration, Time.deltaTime);
                t = ease(elapsed / duration);
                position.x = start.x * (1 - t) * (1 - t) + control.x * 2 * (1 - t) * t + target.x * t * t;
                position.y = start.y * (1 - t) * (1 - t) + control.y * 2 * (1 - t) * t + target.y * t * t;
                position.z = start.z * (1 - t) * (1 - t) + control.z * 2 * (1 - t) * t + target.z * t * t;
                transform.localPosition = position;
                yield return 0;
            }

            transform.localPosition = target;
        }
        public static IEnumerator CurveTo(this Transform transform, Vector3 control, Vector3 target, float duration)
        {
            return CurveTo(transform, control, target, duration, Ease.Linear);
        }
        public static IEnumerator CurveTo(this Transform transform, Vector3 control, Vector3 target, float duration, EaseType ease)
        {
            return CurveTo(transform, control, target, duration, Ease.FromType(ease));
        }

        public static IEnumerator CurveFrom(this Transform transform, Vector3 control, Vector3 start, float duration, Easer ease)
        {
            var target = transform.localPosition;
            transform.localPosition = start;

            return CurveTo(transform, control, target, duration, ease);
        }
        public static IEnumerator CurveFrom(this Transform transform, Vector3 control, Vector3 start, float duration)
        {
            return CurveFrom(transform, control, start, duration, Ease.Linear);
        }
        public static IEnumerator CurveFrom(this Transform transform, Vector3 control, Vector3 start, float duration, EaseType ease)
        {
            return CurveFrom(transform, control, start, duration, Ease.FromType(ease));
        }

        public static IEnumerator Shake(this Transform transform, Vector3 amount, float duration)
        {
            var start = transform.localPosition;
            var shake = Vector3.zero;

            while (duration > 0)
            {
                duration -= Time.deltaTime;
                shake.Set(Random.Range(-amount.x, amount.x), Random.Range(-amount.y, amount.y), Random.Range(-amount.z, amount.z));
                transform.localPosition = start + shake;
                yield return 0;
            }

            transform.localPosition = start;
        }

        public static IEnumerator Shake(this Transform transform, float amount, float duration)
        {
            return Shake(transform, new Vector3(amount, amount, amount), duration);
        }
        #endregion

        #region Waiting Coroutines

        public static IEnumerator Wait(float duration)
        {
            while (duration > 0)
            {
                duration -= Time.deltaTime;
                yield return 0;
            }
        }

        public static IEnumerator WaitUntil(Predicate predicate)
        {
            while (!predicate())
                yield return 0;
        }
        #endregion

        #region Time-based motion

        public static float Loop(float duration, float from, float to, float offsetPercent)
        {
            var range = to - from;
            var total = (Time.time + duration * offsetPercent) * (Mathf.Abs(range) / duration);

            if (range > 0)
                return from + Time.time - (range * Mathf.FloorToInt((Time.time / range)));
            else
                return from - (Time.time - (Mathf.Abs(range) * Mathf.FloorToInt((total / Mathf.Abs(range)))));
        }

        public static float Loop(float duration, float from, float to)
        {
            return Loop(duration, from, to, 0);
        }

        public static Vector3 Loop(float duration, Vector3 from, Vector3 to, float offsetPercent)
        {
            return Vector3.Lerp(from, to, Loop(duration, 0, 1, offsetPercent));
        }

        public static Vector3 Loop(float duration, Vector3 from, Vector3 to)
        {
            return Vector3.Lerp(from, to, Loop(duration, 0, 1));
        }

        public static Quaternion Loop(float duration, Quaternion from, Quaternion to, float offsetPercent)
        {
            return Quaternion.Lerp(from, to, Loop(duration, 0, 1, offsetPercent));
        }

        public static Quaternion Loop(float duration, Quaternion from, Quaternion to)
        {
            return Quaternion.Lerp(from, to, Loop(duration, 0, 1));
        }

        public static float Wave(float duration, float from, float to, float offsetPercent)
        {
            var range = (to - from) / 2;

            return from + range + Mathf.Sin(((Time.time + duration * offsetPercent) / duration) * (Mathf.PI * 2)) * range;
        }

        public static float Wave(float duration, float from, float to)
        {
            return Wave(duration, from, to, 0);
        }

        public static Vector3 Wave(float duration, Vector3 from, Vector3 to, float offsetPercent)
        {
            return Vector3.Lerp(from, to, Wave(duration, 0, 1, offsetPercent));
        }

        public static Vector3 Wave(float duration, Vector3 from, Vector3 to)
        {
            return Vector3.Lerp(from, to, Wave(duration, 0, 1));
        }

        public static Quaternion Wave(float duration, Quaternion from, Quaternion to, float offsetPercent)
        {
            return Quaternion.Lerp(from, to, Wave(duration, 0, 1, offsetPercent));
        }

        public static Quaternion Wave(float duration, Quaternion from, Quaternion to)
        {
            return Quaternion.Lerp(from, to, Wave(duration, 0, 1));
        }
        #endregion
    }

    #region Easing Functions
    public enum EaseType { Linear, CustomCurve, QuadIn, QuadOut, QuadInOut, CubeIn, CubeOut, CubeInOut, BackIn, BackOut, BackInOut, ExpoIn, ExpoOut, ExpoInOut, SineIn, SineOut, SineInOut }

    //Easing operations class
    public static class Ease
    {
        public static readonly Easer Linear = (t, animCurve) => { return t; };
        public static readonly Easer CustomCurve = (t, animCurve) => { return animCurve != null ? animCurve.Evaluate(t) : t; };
        public static readonly Easer QuadIn = (t, animCurve) => { return t * t; };
        public static readonly Easer QuadOut = (t, animCurve) => { return 1f - QuadIn(1f - t); };
        public static readonly Easer QuadInOut = (t, animCurve) => { return (t <= 0.5f) ? QuadIn(t * 2f) * 0.5f : QuadOut(t * 2f - 1f) * 0.5f + 0.5f; };
        public static readonly Easer CubeIn = (t, animCurve) => { return t * t * t; };
        public static readonly Easer CubeOut = (t, animCurve) => { return 1f - CubeIn(1f - t); };
        public static readonly Easer CubeInOut = (t, animCurve) => { return (t <= 0.5f) ? CubeIn(t * 2f) * 0.5f : CubeOut(t * 2f - 1f) * 0.5f + 0.5f; };
        public static readonly Easer BackIn = (t, animCurve) => { return t * t * (2.70158f * t - 1.70158f); };
        public static readonly Easer BackOut = (t, animCurve) => { return 1f - BackIn(1f - t); };
        public static readonly Easer BackInOut = (t, animCurve) => { return (t <= 0.5f) ? BackIn(t * 2f) * 0.5f : BackOut(t * 2f - 1f) * 0.5f + 0.5f; };
        public static readonly Easer ExpoIn = (t, animCurve) => { return (float)Mathf.Pow(2f, 10f * (t - 1f)); };
        public static readonly Easer ExpoOut = (t, animCurve) => { return 1f - ExpoIn(1f - t); };
        public static readonly Easer ExpoInOut = (t, animCurve) => { return t < 0.5f ? ExpoIn(t * 2f) * 0.5f : ExpoOut(t * 2f - 1f) * 0.5f + 0.5f; };
        public static readonly Easer SineIn = (t, animCurve) => { return -Mathf.Cos(Mathf.PI * 0.5f * t) + 1f; };
        public static readonly Easer SineOut = (t, animCurve) => { return Mathf.Sin(Mathf.PI * 0.5f * t); };
        public static readonly Easer SineInOut = (t, animCurve) => { return -Mathf.Cos(Mathf.PI * t) * 0.5f + .5f; };

        public static Easer FromType(EaseType type)
        {
            switch (type)
            {
                case EaseType.Linear: return Linear;
                case EaseType.CustomCurve: return CustomCurve;
                case EaseType.QuadIn: return QuadIn;
                case EaseType.QuadOut: return QuadOut;
                case EaseType.QuadInOut: return QuadInOut;
                case EaseType.CubeIn: return CubeIn;
                case EaseType.CubeOut: return CubeOut;
                case EaseType.CubeInOut: return CubeInOut;
                case EaseType.BackIn: return BackIn;
                case EaseType.BackOut: return BackOut;
                case EaseType.BackInOut: return BackInOut;
                case EaseType.ExpoIn: return ExpoIn;
                case EaseType.ExpoOut: return ExpoOut;
                case EaseType.ExpoInOut: return ExpoInOut;
                case EaseType.SineIn: return SineIn;
                case EaseType.SineOut: return SineOut;
                case EaseType.SineInOut: return SineInOut;
            }
            return Linear;
        }
    }
    #endregion
}
