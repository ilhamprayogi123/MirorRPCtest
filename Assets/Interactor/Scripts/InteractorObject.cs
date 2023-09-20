using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace razz
{
    #region Interaction & Look Types
    //You can add new tags here with category name and interaction name, then add it in InteractionTypes too
    public static class Tags
    {
        [TagField(categoryName = "")] public const int Unselected = 0;
        [TagField(categoryName = "Default")] public const int Animated = 10;
        [TagField(categoryName = "Manual")] public const int Button = 20;
        [TagField(categoryName = "Manual")] public const int Switch = 21;
        [TagField(categoryName = "Manual")] public const int Rotator = 22;
        [TagField(categoryName = "Manual")] public const int Hit = 23;
        [TagField(categoryName = "Manual")] public const int Force = 24;
        [TagField(categoryName = "Touch")] public const int Vertical = 30;
        [TagField(categoryName = "Touch")] public const int HorizontalUp = 31;
        [TagField(categoryName = "Touch")] public const int HorizontalDown = 32;
        [TagField(categoryName = "Touch")] public const int Still = 33;
        [TagField(categoryName = "Distance")] public const int CrossHair = 40;
        [TagField(categoryName = "Climbable")] public const int Ladder = 50;
        [TagField(categoryName = "Multiple")] public const int Cockpit = 60;
        [TagField(categoryName = "Self")] public const int Itch = 70;
        [TagField(categoryName = "Pickable")] public const int OneHand = 80;
        [TagField(categoryName = "Pickable")] public const int TwoHands = 81;
        [TagField(categoryName = "Push - Pull")] public const int Push = 90;
        [TagField(categoryName = "Cover")] public const int Stand = 100;
        [TagField(categoryName = "Cover")] public const int Crouch = 101;
        [TagField(categoryName = "Greeting")] public const int Greet = 110;
    }

    public enum InteractionTypes
    {
        DefaultAnimated = Tags.Animated,
        ManualButton = Tags.Button,
        ManualSwitch = Tags.Switch,
        ManualRotator = Tags.Rotator,
        ManualHit = Tags.Hit,
        ManualForce = Tags.Force,
        TouchVertical = Tags.Vertical,
        TouchHorizontalUp = Tags.HorizontalUp,
        TouchHorizontalDown = Tags.HorizontalDown,
        TouchStill = Tags.Still,
        DistanceCrosshair = Tags.CrossHair,
        ClimbableLadder = Tags.Ladder,
        MultipleCockpit = Tags.Cockpit,
        SelfItch = Tags.Itch,
        PickableOne = Tags.OneHand,
        PickableTwo = Tags.TwoHands,
        Push = Tags.Push,
        CoverStand = Tags.Stand,
        CoverCrouch = Tags.Crouch,
        GreetTest = Tags.Greet
    };

    public enum Look
    {
        Never = 0,
        OnSelection = (1 << 0),
        Before = (1 << 1),
        OnPause = (1 << 2),
        After = (1 << 3),
        Always = ~0
    }
    #endregion

    [HelpURL("https://negengames.com/interactor/components.html#interactorobjectcs")]
    [DisallowMultipleComponent]
    public class InteractorObject : MonoBehaviour
    {
        #region InteractorObject Variables
        //TagFilter is custom attribute, it will give drop down menu with categories filled with Tags class
        //And return assigned int. You can remove it and use regular enum  property on inspector.
        [Header("Type Specific Settings")]
        [TagFilter(typeof(Tags))] public int interaction;
        [Space(20f)]
        [ConditionalSO(Condition.Show, nameof(DefaultEnabled))] 
        public Default defaultSettings;
        [ConditionalSO(Condition.Show, nameof(ManualEnabled))]
        public Manual manualSettings;
        [ConditionalSO(Condition.Show, nameof(TouchEnabled))]
        public Touch touchSettings;
        [ConditionalSO(Condition.Show, nameof(DistanceEnabled))]
        public Distance distanceSettings;
        [ConditionalSO(Condition.Show, nameof(ClimbableEnabled))]
        public Climbable climbableSettings;
        [ConditionalSO(Condition.Show, nameof(MultipleEnabled))]
        public Multiple multipleSettings;
        [ConditionalSO(Condition.Show, nameof(SelfEnabled))]
        public Self selfSettings;
        [ConditionalSO(Condition.Show, nameof(PickableEnabled))]
        public Pickable pickableSettings;
        [ConditionalSO(Condition.Show, nameof(PushEnabled))]
        public Push pushSettings;
        [ConditionalSO(Condition.Show, nameof(CoverEnabled))]
        public Cover coverSettings;
        [ConditionalSO(Condition.Show, nameof(GreetEnabled))]
        public Greet greetSettings;

        [Foldout("Interaction Settings", true)]
        [Tooltip("Prevents this object from leaving Interactor's list with OnTriggerExit. If enabled, this InteractorObject will only exit with Interactor.RemoveInteractionManual() call.")]
        public bool preventExit;
        [Tooltip("Interactable objects list gets ordered with this priority. Higher priority interaction will be upper on the selection list and also will take look target priority")]
        [Range(0, 200)]
        public int priority = 100;
        [Tooltip("Enable if this object needs more than one effector to interact.")]
        public bool multipleConnections = false;
        [Tooltip("If this interaction needs to pause on half.")] 
        public bool pauseOnInteraction;
        [Tooltip("If this interaction interruptible.")] 
        public bool interruptible;
        [Tooltip("If this object interactable without Y axis checks on effectors.")]
        public bool zOnly;
        [Tooltip("If there is pivot object between target and parent, assign here to rotate itself to effector while interacting.")]
        public GameObject pivot;
        [Tooltip("Rotate pivot on X axis")]
        public bool pivotX = true;
        [Tooltip("Rotate pivot on Y axis")]
        public bool pivotY = true;
        [Tooltip("Rotate pivot on Z axis")]
        public bool pivotZ = true;
        [Tooltip("Resets pivot rotation to back when interaction is over.")]
        public bool resetPivot = false;
        [Tooltip("Extra raycast to check if object has any obstacles between target and effector.")]
        public bool obstacleRaycast;
        [Tooltip("Exclude colliders to be checked for obstacleRaycast. You can exclude this object's and child colliders for checking obstacles for example.")]
        public Collider[] excludeColliders;
        [Tooltip("If all targets arent child of this InteractorObject, select a parent who has all targets.")]
        public GameObject otherTargetsRoot;
        [Tooltip("If other InteractorObject targets exist in this children, exlclude them from this interactions' targets.")]
        public InteractorTarget[] excludeTargets;

        [Foldout("Speed Settings", true)]
        [Tooltip("The time needs to pass to reach the target which is half of interaction.")]
        public float targetDuration = 1f;
        [Tooltip("The time needs to pass to go back to default position from target which is other half of interaction.")]
        public float backDuration = 0.35f;
        [Tooltip("Interaction animation easing. Select Custom for Animation Curve editing.")]
        public EaseType easeType = EaseType.QuadIn;
        [Tooltip("Animation curve for custom speed needs at least 3 keyframes at (0,0), (1,1) and (0,2). You can add more keyframes between those. So they'll be between 0 and 2 (horizontal values). 0 to 1 is for targetPath, 1 to 2 is for backPath.")]
        [Conditional(Condition.Show, nameof(ShowAnimationCurve))]
        public AnimationCurve speedCurve;
        [TextArea]
        public string ps = "Before editing the animation curve, select an InteractorTarget (one of this InteractorObject's), then select back this object (also make sure Speed debug is enabled (Speed button on InteractorTarget)). This way it can visualize the speed values in SceneView and also it will check your animation curve to prevent editing mistakes.";

        [Foldout("Animation Assist Settings", true)]
        [Tooltip("Enable if you wish to use animation clips with this interaction. Interactor player needs to have AnimAssist component to use this.")]
        public bool animAssistEnabled;
        [Conditional(Condition.Show, "animAssistEnabled")] 
        public string clipName = "CaSe-SEnsitiVe";
        [Tooltip("Lower this weight if you wish to blend the clip with your default Animator Controller layer animation.")]
        [Range(0f, 1f)] public float clipWeight = 1f;
        [Tooltip("Adjust clip speed from here, not on state settings on Animator Controller. Start Time and Clip Offset will be adjusted accordingly.")]
        [Range(0.05f, 10f)] public float clipSpeed = 1f;
        [Tooltip("If you wish to skip first part of your clip, adjust this normalized value. It is between 0 and 1 so it will work as percentage. Start Time calculation will start after this cut. And if you wish to cut last part of the clip, use InteractorLayer state settings at Animator Controller by selecting exit transitions and adjusting their Exit Times.")]
        [Range(0f, 1f)] public float clipOffset = 0f;
        [Tooltip("Adjust this value to determine the starting point of the clip where the interaction will begin. You can select the clip and make your decision while previewing it.")]
        [Range(0f, 1f)] public float startTime = 0.5f;
        [Tooltip("Enable this option if you wish to use the same animation clip for leaving the InteractorObject. This is applicable to InteractorObjects that have two uses, such as using the same object to drop.")]
        public bool secondUse = false;
        [Tooltip("If you use Second Use, you can set different starting time for the same clip. If you wish to use same, set same value as Start Time.")]
        [Range(0f, 1f)] public float secondStartTime = 0.5f;

        [Foldout("AI Settings", true)]
        [Tooltip("Set any transform to enable AI movement for this interaction. Player will move to its position and rotation then will start interaction by itself. But this InteractorObject needs to be in a PathGrid.")]
        public Transform aiTarget;
        [Tooltip("InteractorObjects are exclusively added to Interactors by the PathGrid. Players will initiate this interaction only when they have reached their designated aiTarget. If disabled, interactions can be used as normal too when player is in a good position.")]
        public bool aiOnly;
        [Tooltip("Interactor initiates the interaction before reaching the aiTarget, resulting in a more natural movement. (in meters)")]
        public float startEarly = 0;
        private bool _reached;

        public bool Reached
        {
            get { return _reached; }
            set { _reached = value; }
        }

        [Foldout("Look Settings", true)]
        [Tooltip("Enable when you want to look at this. You can select multiple states.")]
        [EnumFlags] public Look lookAtThis = Look.Never;
        [Tooltip("If enabled, it will look at active InteractorTarget of this InteractorObject instead of this transform.")]
        public bool lookAtChildren = false;
        [Tooltip("If assigned, it will look at this transform.")]
        public Transform alternateLookTarget;
        [Tooltip("Seconds to pass before starting to look")]
        public float waitTimeToLook = 0f;
        [Tooltip("Seconds to end total ongoing look. 0 for disable.")]
        public float lookTimeout = 0f;
        [Tooltip("You can lower weight for close targets.")]
        [Range(0.05f, 1f)]public float lookWeight = 1f;
        [Tooltip("The time needs to pass to rotate the head to the target.")]
        public float rotationDurationTarget = 1f;
        [Tooltip("The time needs to pass to rotate the head to its default rotation.")]
        public float rotationDurationBack = 1f;

        [Foldout("Other", true)]
        [Tooltip("If highlighted material is another object, assign here. If it is this object leave empty.")]
        public MeshRenderer outlineMat;
        [Tooltip("If one hand drop back enabled and has drop transforms, it will check every transform if they're good to drop for this effector and will drop on first possible one.")]
        public Transform[] dropLocations;
        [Tooltip("Example scene info texts to toggle on/off.")]
        public GameObject info;

        [Foldout("Events", true)]
        [Space(10)]
        public UnityEvent unityEvent;
        public UnityEvent unityEndEvent;

        [HideInInspector] public InteractionTypes interactionType;
        [HideInInspector] public InteractorTarget[] childTargets;
        [HideInInspector] public bool ready;
        [HideInInspector] public bool used;
        [HideInInspector] public Interactor currentInteractor;

        private bool[] _useableEffectors;
        private InteractionTypeSettings _activeSettings;

        //Rigidbody, raycast or pivot operations
        [HideInInspector] public Collider col;
        [HideInInspector] public Rigidbody rigid;
        [HideInInspector] public bool hasRigid;
        [HideInInspector] public bool rotating;
        [HideInInspector] public Vector3 rotateTo;
        private Quaternion _pivotDirection;
        private Vector3 _tempRotation;
        //ManualHit
        private HitHandler _hitHandler;
        [HideInInspector] public bool hitObjUseable;
        [HideInInspector] public bool hitDone;
        //Outline Material
        private int _propertyIdFirstOutline;
        private int _propertyIdSecondOutline;
        private Color _firstOutline;
        private Color _secondOutline;
        private bool _hasOutlineMat;
        [HideInInspector] public Material thisMat;
        //AutoMovers
        [HideInInspector] public AutoMover[] autoMovers;
        [HideInInspector] public bool hasAutomover;
        //Switches & Rotators
        [HideInInspector] public InteractiveSwitch[] interactiveSwitches;
        [HideInInspector] public bool hasInteractiveSwitch;
        [HideInInspector] public InteractiveRotator[] interactiveRotators;
        [HideInInspector] public bool hasInteractiveRotator;
        //Vehicle Parts
        //Gets true automatically by VehiclePartControls if it has animation on Vehicle Animator
        [HideInInspector] public bool isVehiclePartwithAnimation;
        //Gets its hash id automatically by VehiclePartControls if it has animation on Vehicle Animator
        [HideInInspector] public int vehiclePartId;
        #endregion

        public void SendUnityEvent()
        {
            if (unityEvent != null) unityEvent.Invoke();
        }

        public void SendUnityEndEvent()
        {
            if (unityEndEvent != null) unityEndEvent.Invoke();
        }

        private void Awake()
        {
            if (easeType == EaseType.CustomCurve)
            {
                if (speedCurve == null || speedCurve.keys.Length < 3)
                {
                    Debug.LogWarning("SpeedCurve is not correct. \"" + this.name + "\" easing type set as QuadIn. Please set Custom Curve of InteractorObject.", this);
                    easeType = EaseType.QuadIn;
                }
            }
            //Unselected InteractionType
            if (interaction == 0)
            {
                Debug.LogWarning(this.name + " has InteractorObject with unselected Interaction Type! GameObject disabled.", this);
                gameObject.SetActive(false);
                return;
            }
            else
            {
                //Set all settings files null except the one for selected type
                if (!DefaultEnabled()) defaultSettings = null;
                else if (defaultSettings != null) _activeSettings = defaultSettings;
                if (!ManualEnabled()) manualSettings = null;
                else if (manualSettings != null) _activeSettings = manualSettings;
                if (!TouchEnabled()) touchSettings = null;
                else if (touchSettings != null) _activeSettings = touchSettings;
                if (!DistanceEnabled()) distanceSettings = null;
                else if (distanceSettings != null) _activeSettings = distanceSettings;
                if (!ClimbableEnabled()) climbableSettings = null;
                else if (climbableSettings != null) _activeSettings = climbableSettings;
                if (!MultipleEnabled()) multipleSettings = null;
                else if (multipleSettings != null)
                {
                    multipleSettings = Instantiate(multipleSettings);
                    _activeSettings = multipleSettings;
                }
                if (!SelfEnabled()) selfSettings = null;
                else if (selfSettings != null) _activeSettings = selfSettings;
                if (!PickableEnabled()) pickableSettings = null;
                else if (pickableSettings != null)
                {
                    pickableSettings = Instantiate(pickableSettings);
                    _activeSettings = pickableSettings;
                }
                if (!PushEnabled()) pushSettings = null;
                else if (pushSettings != null)
                {
                    pushSettings = Instantiate(pushSettings);
                    _activeSettings = pushSettings;
                }
                if (!CoverEnabled()) coverSettings = null;
                else if (coverSettings != null) _activeSettings = coverSettings;
                if (!GreetEnabled()) greetSettings = null;
                else if (greetSettings != null) _activeSettings = greetSettings;

                if (!_activeSettings)
                {
                    Debug.LogWarning(this.name + "<b><color=red> has no settings file!</color></b>", this);
                    gameObject.SetActive(false);
                    return;
                }
            }

            interactionType = (InteractionTypes)interaction;
            _useableEffectors = new bool[9];

            //Outline
            if (outlineMat)
            {
                thisMat = outlineMat.material;
                if (thisMat.HasProperty("_FirstOutlineColor"))
                {
                    //Instead of strings, we cache ids, much faster.
                    _propertyIdFirstOutline = Shader.PropertyToID("_FirstOutlineColor");
                    _firstOutline = thisMat.GetColor(_propertyIdFirstOutline);

                    _propertyIdSecondOutline = Shader.PropertyToID("_SecondOutlineColor");
                    _secondOutline = thisMat.GetColor(_propertyIdSecondOutline);

                    _hasOutlineMat = true;

                    _firstOutline.a = 0;
                    _secondOutline.a = 0;
                    thisMat.SetColor(_propertyIdFirstOutline, _firstOutline);
                    thisMat.SetColor(_propertyIdSecondOutline, _secondOutline);
                }
            }
            else if (GetComponentInParent<MeshRenderer>())
            {
                thisMat = GetComponentInParent<MeshRenderer>().material;
                if (thisMat.HasProperty("_FirstOutlineColor"))
                {
                    //Instead of strings, we cache ids, much faster.
                    _propertyIdFirstOutline = Shader.PropertyToID("_FirstOutlineColor");
                    _firstOutline = thisMat.GetColor(_propertyIdFirstOutline);

                    _propertyIdSecondOutline = Shader.PropertyToID("_SecondOutlineColor");
                    _secondOutline = thisMat.GetColor(_propertyIdSecondOutline);

                    _hasOutlineMat = true;

                    _firstOutline.a = 0;
                    _secondOutline.a = 0;
                    thisMat.SetColor(_propertyIdFirstOutline, _firstOutline);
                    thisMat.SetColor(_propertyIdSecondOutline, _secondOutline);
                }
            }

            col = GetComponent<Collider>();
            if (rigid = GetComponent<Rigidbody>())
                hasRigid = true;

            //Get all targets on children
            if (otherTargetsRoot != null)
            {
                childTargets = otherTargetsRoot.GetComponentsInChildren<InteractorTarget>();
                if (excludeTargets.Length > 0)
                    childTargets = ExcludedTargets(childTargets);
                for (int i = 0; i < childTargets.Length; i++)
                {
                    childTargets[i].intObj = this;
                }

                autoMovers = otherTargetsRoot.GetComponentsInChildren<AutoMover>();
                if (autoMovers != null && autoMovers.Length > 0)
                {
                    hasAutomover = true;
                }
            }
            else
            {
                childTargets = GetComponentsInChildren<InteractorTarget>();
                if (excludeTargets.Length > 0)
                    childTargets = ExcludedTargets(childTargets);
                for (int i = 0; i < childTargets.Length; i++)
                {
                    childTargets[i].intObj = this;
                }

                autoMovers = GetComponentsInChildren<AutoMover>();
                if (autoMovers != null && autoMovers.Length > 0)
                {
                    hasAutomover = true;
                }
            }

            interactiveSwitches = GetComponentsInChildren<InteractiveSwitch>();
            if (interactiveSwitches != null && interactiveSwitches.Length > 0)
            {
                hasInteractiveSwitch = true;
            }

            interactiveRotators = GetComponentsInChildren<InteractiveRotator>();
            if (interactiveRotators != null && interactiveRotators.Length > 0)
            {
                hasInteractiveRotator = true;
            }

            //ManualHit
            _hitHandler = GetComponentInChildren<HitHandler>();

            //If there is a info for this object, assign it as GameObject, deactivating it here for to be activated with interactions.
            Info(false);
        }

        public InteractorTarget[] ExcludedTargets(InteractorTarget[] interactorTargets)
        {
            List<InteractorTarget> newTargets = new List<InteractorTarget>(interactorTargets);
            for (int i = 0; i < excludeTargets.Length; i++)
            {
                int index = newTargets.IndexOf(excludeTargets[i]);
                if (index >= 0)
                {
                    newTargets.RemoveAt(index);
                }
            }
            return newTargets.ToArray();
        }

        private void Start()
        {
            //Needs to be on start for InstantiateRandomAreaPool because it instantiates its children on awake. For ManualForce Truck Example.
            if (interactionType == InteractionTypes.ManualForce)
            {
                //If there is a InstantiateRandomAreaPool component, add its prefabs to childTargets array, since they arent parented.
                InstantiateRandomAreaPool _pool;
                if ((_pool = GetComponent<InstantiateRandomAreaPool>()))
                {
                    //If there are already child targets as children, add spawned prefabs with copying arrays.
                    if (childTargets.Length != 0)
                    {
                        InteractorTarget[] childTargetsCopy = new InteractorTarget[childTargets.Length + _pool.maxPrefabCount];

                        for (int i = 0; i < childTargets.Length; i++)
                        {
                            childTargetsCopy[i] = childTargets[i];
                        }

                        for (int j = 0; j < _pool.maxPrefabCount; j++)
                        {
                            childTargetsCopy[j + childTargets.Length] = _pool._prefabList[j].GetComponent<InteractorTarget>();
                        }

                        childTargets = childTargetsCopy;
                    }
                    else
                    {
                        InteractorTarget[] childTargetsCopy = new InteractorTarget[_pool.maxPrefabCount];

                        for (int j = 0; j < _pool.maxPrefabCount; j++)
                        {
                            childTargetsCopy[j] = _pool._prefabList[j].GetComponent<InteractorTarget>();
                        }

                        childTargets = childTargetsCopy;
                    }
                }
            }
            if (pivot && resetPivot) _pivotDirection = pivot.transform.rotation;
            _activeSettings.Init(this);
        }

        //Assigns the Interactor which tries to use this so InteractorObject can know which Interactor is going to use now
        public void AssignInteractor(Interactor interactor)
        {
            currentInteractor = interactor;
        }
        //Used is true when used by effector, called by Interactor directly for turning off the outline material. 
        //Otherwise, it is only called by AddEffectorToUseables and RemoveEffectorFromUseables
        public void Prepare(bool On)
        {
            if (!ready && On)
            {
                Useable();
                ready = true;
            }
            else if(ready && On) return;

            if (ready && !On)
            {
                NotUseable();
                ready = false;
            }
        }
        //Outline and info texts
        public void Useable()
        {
            if (_hasOutlineMat)
            {
                _firstOutline.a = 0.6f;
                _secondOutline.a = 0.4f;
                thisMat.SetColor(_propertyIdFirstOutline, _firstOutline);
                thisMat.SetColor(_propertyIdSecondOutline, _secondOutline);
            }
            Info(true);
        }
        //Outline and info texts
        public void NotUseable()
        {
            if (_hasOutlineMat)
            {
                _firstOutline.a = 0;
                _secondOutline.a = 0;
                thisMat.SetColor(_propertyIdFirstOutline, _firstOutline);
                thisMat.SetColor(_propertyIdSecondOutline, _secondOutline);
            }
            Info(false);
            Reached = false;
        }
        public void Used(bool used)
        {
            this.used = used;
        }
        //Check if effector switched its toggle to useable for this object
        public bool CanEffectorUse(int effector)
        {
            return _useableEffectors[effector];
        }
        //Returns first useable effector other than input
        public int GetOtherUseableEffector(int effector)
        {
            for (int i = 0; i < _useableEffectors.Length; i++)
            {
                if (i == effector) continue;

                if (_useableEffectors[i])
                {
                    return i;
                }
            }
            return -1;
        }
        //Activates object prepares InteractorObject
        //Adds or removes effectors depending on their useability for this object
        public void AddEffectorToUseables(int effector)
        {
            if (effector < 0) return;

            _useableEffectors[effector] = true;
            Prepare(true);
        }
        //Adds effector, enables object if count amount of effectors in use.
        public void AddEffectorToUseables(int effector, int count)
        {
            if (effector < 0) return;

            _useableEffectors[effector] = true;
            if (UseableEffectorCount() >= count)
                Prepare(true);
        }
        public void RemoveEffectorFromUseables(int effector)
        {
            if (effector < 0)
            {
                ResetUseableEffectors();
                Prepare(false);
                return;
            }

            _useableEffectors[effector] = false;
            if (UseableEffectorCount() == 0)
            {
                Prepare(false);
                if (pivot && resetPivot) pivot.transform.rotation = _pivotDirection;
            }
        }
        public void RemoveEffectorFromUseables(int effector, int count)
        {
            if (effector < 0)
            {
                ResetUseableEffectors();
                Prepare(false);
                return;
            }

            _useableEffectors[effector] = false;
            if (UseableEffectorCount() < count)
                Prepare(false);
        }
        //Returns how many effectors is in use position for this object
        public int UseableEffectorCount()
        {
            int count = 0;
            for (int i = 0; i < _useableEffectors.Length; i++)
            {
                if (_useableEffectors[i])
                {
                    count++;
                }
            }
            return count;
        }
        public void ResetUseableEffectors()
        {
            for (int i = 0; i < _useableEffectors.Length; i++)
            {
                _useableEffectors[i] = false;
            }
        }
        //Returns all target transforms in list for given effector type
        public List<Transform> GetTargetTransformsForEffectorType(int effectorType)
        {
            List<Transform> targetTransforms = new List<Transform>();
            for (int i = 0; i < childTargets.Length; i++)
            {
                if ((int)childTargets[i].effectorType == effectorType)
                {
                    targetTransforms.Add(childTargets[i].transform);
                }
            }
            return targetTransforms;
        }
        //Returns first InteractorTarget for given type
        public InteractorTarget GetTargetForEffectorType(int effectorType)
        {
            for (int i = 0; i < childTargets.Length; i++)
            {
                if ((int)childTargets[i].effectorType == effectorType)
                {
                    return childTargets[i];
                }
            }
            return null;
        }
        //Returns all targets for given effector type
        public InteractorTarget[] GetTargetsForEffectorType(int effectorType)
        {
            List<InteractorTarget> targets = new List<InteractorTarget>();
            for (int i = 0; i < childTargets.Length; i++)
            {
                if ((int)childTargets[i].effectorType == effectorType)
                {
                    targets.Add(childTargets[i]);
                }
            }
            return targets.ToArray();
        }
        //Checks if this InteractorObject has this effector type in children
        public bool HasEffectorTypeInTargets(int effectorType)
        {
            for (int i = 0; i < childTargets.Length; i++)
            {
                if ((int)childTargets[i].effectorType == effectorType)
                {
                    return true;
                }
            }
            return false;
        }
        public void RemoveTargetFromChildTargets(InteractorTarget removeTarget)
        {
            List<InteractorTarget> newChildTargets = new List<InteractorTarget>();
            for (int i = 0; i < childTargets.Length; i++)
            {
                if (childTargets[i] == removeTarget) continue;

                newChildTargets.Add(childTargets[i]);
            }
            childTargets = newChildTargets.ToArray();
        }

        //Called by Interactor to rotate pivot if assigned any
        public void RotatePivot(Vector3 rotateTo)
        {
            if (pivot != null)
            {
                //If has a rigidbody and moving, we need to rotate target pivot until pick is done
                if (hasRigid)
                {
                    if (!rigid.IsSleeping())
                    {
                        //Bool for LateUpdate loop to rotate continuously and cache target to rotate
                        rotating = true;
                        this.rotateTo = rotateTo;
                    }
                }
                Rotate(rotateTo);
            }
        }
        public void Rotate(Vector3 target)
        {
            _tempRotation = (transform.position - target).normalized;
            Quaternion xRot = Quaternion.FromToRotation(_tempRotation, Vector3.up);
            Vector3 _tempRotUp = xRot * _tempRotation;
            _tempRotation = Quaternion.LookRotation(_tempRotation, _tempRotUp).eulerAngles;

            //If any axis not selected, get its own value, so dont rotate on that axis.
            if (!pivotX) _tempRotation.x = pivot.transform.eulerAngles.x;
            if (!pivotY) _tempRotation.y = pivot.transform.eulerAngles.y;
            if (!pivotZ) _tempRotation.z = pivot.transform.eulerAngles.z;

            //This rotates its InteractorTarget(with pivot) to the effector which started interaction
            //If there is a pivot, this is called at least one time or more if rigidbody is moving
            pivot.transform.eulerAngles = _tempRotation;
        }

        //Only used by Hit interaction, explained in Interactor.EffectorLink Update
        public void Hit(Transform interactionTarget, Vector3 effectorPos, Interactor interactor)
        {
            if (hitObjUseable)
            {
                _hitHandler.HitHandlerRotate(interactor);
            }

            if (used && !_hitHandler.moveOnce)
            {
                _hitHandler.moveOnce = true;
                hitDone = false;
                _hitHandler.HitPosMove(interactionTarget, effectorPos);
            }
        }
        //Only used by Hit interaction
        public void HitPosDefault(Transform interactionTarget, Vector3 effectorPos)
        {
            if (!_hitHandler.hitDone)
            {
                _hitHandler.HitPosDefault(interactionTarget, effectorPos);
            }
            else
            {
                hitDone = true;
            }
        }

        //Touch Method Entries for Passing this collider.
        public bool RaycastVertical(Transform playerTransform, out RaycastHit hit, bool left)
        {
            if (_activeSettings != touchSettings) 
            {
                Debug.LogWarning("This interaction is not set as Touch.", this);
                hit = new RaycastHit();
                return false;
            }
            return touchSettings.RaycastVertical(playerTransform, out hit, col, left);
        }
        public bool RaycastHorizontalUp(Transform playerTransform, out RaycastHit hit)
        {
            if (_activeSettings != touchSettings)
            {
                Debug.LogWarning("This interaction is not set as Touch.", this);
                hit = new RaycastHit();
                return false;
            }
            return touchSettings.RaycastHorizontalUp(playerTransform, out hit, col);
        }

        //You can delete this with its three references, just for info texts.
        private void Info(bool on)
        {
            if (info != null)
            {
                if (on)
                    info.SetActive(true);
                else
                    info.SetActive(false);
            }
        }

        private void LateUpdate()
        {
            if (!currentInteractor) return;

            _activeSettings.UpdateSettings();
        }

        #region Variable and Interaction Conditions
        public bool ShowAnimationCurve()
        {
            if (easeType == EaseType.CustomCurve) return true;
            else return false;
        }
        public bool DefaultEnabled()
        {
            if (interaction >= 10 && interaction < 20) return true;
            else return false;
        }
        public bool ManualEnabled()
        {
            if (interaction >= 20 && interaction < 30) return true;
            else return false;
        }
        public bool TouchEnabled()
        {
            if (interaction >= 30 && interaction < 40) return true;
            else return false;
        }
        public bool DistanceEnabled()
        {
            if (interaction >= 40 && interaction < 50) return true;
            else return false;
        }
        public bool ClimbableEnabled()
        {
            if (interaction >= 50 && interaction < 60) return true;
            else return false;
        }
        public bool MultipleEnabled()
        {
            if (interaction >= 60 && interaction < 70) return true;
            else return false;
        }
        public bool SelfEnabled()
        {
            if (interaction >= 70 && interaction < 80) return true;
            else return false;
        }
        public bool PickableEnabled()
        {
            if (interaction >= 80 && interaction < 90) return true;
            else return false;
        }
        public bool PushEnabled()
        {
            if (interaction >= 90 && interaction < 100) return true;
            else return false;
        }
        public bool CoverEnabled()
        {
            if (interaction >= 100 && interaction < 110) return true;
            else return false;
        }

        public bool GreetEnabled()
        {
            if (interaction >= 110 && interaction < 120) return true;
            else return false;
        }
        #endregion
    }
}
