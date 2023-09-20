using UnityEngine;
using System.Collections;

namespace razz
{
    [HelpURL("https://negengames.com/interactor/components.html#animassistcs")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Interactor))]
    public class AnimAssist : MonoBehaviour
    {
        [Tooltip("Assign the Interactor manually for the best practice.")]
        public Interactor interactor;
        [Tooltip("Assign the Animator manually for the best practice.")]
        public Animator animator;
        [Tooltip("Assign all the clips you wish to use in your interactions. They will be added to this Animator Controller.")]
        public AnimationClip[] animationClips;
        [Tooltip("Prevent multiple uses of the same interaction before it starts. If disabled, the player will be able to use the same interaction multiple times, which can cause problems like replaying the same clip over and over again.")]
        public bool preventSpam = true;

        private bool _initiated;
        private bool _clipPlaying;
        public bool clipPlaying
        {
            get { return _clipPlaying; }
            set { _clipPlaying = value; }
        }
        private int _clipIndex = -1;
        private string _interactorLayerName = "InteractorLayer";
        public string interactorLayerName
        {
            get { return _interactorLayerName; }
            set { _interactorLayerName = value; }
        }
        private int _interactorLayerIndex = -1;
        public int interactorLayerIndex
        {
            get { return _interactorLayerIndex; }
            set { _interactorLayerIndex = value; }
        }
        private string _speedParam = "animAssistSpeed";
        public string speedParam
        {
            get { return _speedParam; }
            set { _speedParam = value; }
        }

        private void Start()
        {
            Init();
        }
        private void Init()
        {
            if (!interactor)
            {
                interactor = GetComponent<Interactor>();
                Debug.LogWarning("Please assign Interactor to this AnimAssist for best practice.", this);
            }
            if (!animator)
            {
                animator = GetComponentInChildren<Animator>();
                Debug.LogWarning("Please assign Animator to this AnimAssist for best practice.", this);
            }
            if (!interactor || !animator)
            {
                _initiated = false;
                Debug.LogWarning("Interactor or Animator could not be found, AnimAssist disabled.", this);
                return;
            }
            interactor.animAssist = this;
            _initiated = CheckAnimator();
        }
        private bool CheckAnimator()
        {
            interactorLayerIndex = -1;
            interactorLayerIndex = animator.GetLayerIndex(interactorLayerName);
            if (interactorLayerIndex < 0)
            {
                Debug.LogWarning("Interactor Layer could not be found for AnimAssist! Please add Interactor Layer after stopping the play mode.", this);
                return false;
            }

            int stateId;
            for (int i = 0; i < animationClips.Length; i++)
            {
                if (!animationClips[i])
                {
                    Debug.LogWarning(i + ". clip is null on AnimAssist component! Please re-add Interactor layer to update, after stopping the play mode.", this);
                    continue;
                }

                stateId = Animator.StringToHash(_interactorLayerName + "." + animationClips[i].name);
                if (!animator.HasState(interactorLayerIndex, stateId))
                {
                    Debug.LogWarning(animationClips[i].name + " in Interactor Layer(Animator Controller) could not be found for AnimAssist! Please add Interactor layer after stopping the play mode.", this);
                }
            }
            return true;
        }

        public bool CheckAnimAssist(string clipName)
        {//Check if this returns true before continuing on SetAnimAssist
            _clipIndex = -1;
            if (!_initiated)
            {
                Debug.LogWarning("AnimAssist could not initiated.");
                return false;
            }
            _clipIndex = GetClipIndex(clipName);
            if (_clipIndex < 0)
            {
                Debug.LogWarning("AnimAssist clip name " + clipName + " could not be found on AnimAssist clips. Please check the name, interaction is starting without AnimAssist.");
                return false;
            }
            if (animationClips[_clipIndex] == null)
            {
                Debug.LogWarning("Animation clip is missing for AnimAssist. Try re-adding by Add Interactor Layer.");
                return false;
            }
            return true;
        }
        public void SetAnimAssist(InteractorObject interactorObject)
        {//Start given animation and coroutine with time parameters
            if (_clipPlaying && preventSpam) return;
            if (!interactorObject.used && !interactorObject.ready) return;
            if (_clipIndex < 0)
            {
                Debug.LogWarning("Wrong clip index for AnimAssist!");
                return;
            }

            float offsetInSeconds = Mathf.Lerp(0, animationClips[_clipIndex].length, interactorObject.clipOffset);

            if (preventSpam) StartCoroutine(AssistTimer(interactorObject, offsetInSeconds));

            if (interactorObject.secondUse && interactorObject.used)
                StartCoroutine(AnimCoroutine(interactorObject, interactorObject.secondStartTime, offsetInSeconds));
            else StartCoroutine(AnimCoroutine(interactorObject, interactorObject.startTime, offsetInSeconds));

            animator.SetLayerWeight(interactorLayerIndex, interactorObject.clipWeight);
            animator.SetFloat(_speedParam, interactorObject.clipSpeed);
            animator.CrossFadeInFixedTime(animationClips[_clipIndex].name, 0.25f, interactorLayerIndex, offsetInSeconds);
        }
        private int GetClipIndex(string clipName)
        {
            for (int i = 0; i < animationClips.Length; i++)
                if (animationClips[i].name == clipName) return i;
            return -1;
        }
        private IEnumerator AnimCoroutine(InteractorObject intObj, float startTime, float offset)
        {
            yield return new WaitForSeconds((startTime * (1 / intObj.clipSpeed) * animationClips[_clipIndex].length) - intObj.clipOffset);
            interactor.StartStopInteractionAnim(intObj);
            yield return null;
        }
        private IEnumerator AssistTimer(InteractorObject intObj, float offset)
        {//Locks AnimAssist AnimCoroutine until current clip is over
            _clipPlaying = true;
            yield return new WaitForSeconds(((1 / intObj.clipSpeed) * animationClips[_clipIndex].length) - intObj.clipOffset);
            _clipPlaying = false;
            yield return null;
        }
        private void AddAnimEvent(InteractorObject intObj)
        {//Alternative to coroutine is animation event but it seems they're less accurate in timing
            animationClips[_clipIndex].events = new AnimationEvent[0];
            AnimationEvent animEvent = new AnimationEvent();
            animEvent.functionName = "StartInteractionEvent";
            animEvent.time = (intObj.startTime * (1 / intObj.clipSpeed) * animationClips[_clipIndex].length) - intObj.clipOffset;
            animEvent.objectReferenceParameter = intObj;
            animationClips[_clipIndex].AddEvent(animEvent);
        }
        private void StartInteractionEvent(InteractorObject intObj)
        {
            interactor.StartStopInteractionAnim(intObj);
            animationClips[_clipIndex].events = new AnimationEvent[0];
        }
    }
}
