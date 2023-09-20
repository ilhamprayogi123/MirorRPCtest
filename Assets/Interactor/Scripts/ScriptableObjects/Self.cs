using UnityEngine;

namespace razz
{
    [CreateAssetMenu(fileName = "SelfSettings", menuName = "Interactor/SelfSettings")]
    public class Self : InteractionTypeSettings
    {
        [HideInInspector] public bool selfActive;
        [HideInInspector] public PathMover[] pathMovers;

        public override void Init(InteractorObject interactorObject)
        {
            base.Init(interactorObject);
            AssignPathMovers();
            //Max priority to keep self interaction on top of interaction list order.
            _intObj.priority = int.MaxValue;
            selfActive = false;
        }

        private void AssignPathMovers()
        {
            pathMovers = _intObj.GetComponentsInChildren<PathMover>();

            bool checkPathMovers = false;
            for (int i = 0; i < _intObj.childTargets.Length; i++)
            {
                if (pathMovers[i] == null)
                {
                    checkPathMovers = true;
                    Debug.Log(_intObj.childTargets[i].name + " is SelfInteraction target without Path Mover.");
                }
            }

            if (checkPathMovers == true)
                _intObj.gameObject.SetActive(false);
        }
        //Gets "True" probability in every "pathMovers.odd" seconds
        public bool CheckOdds(int i)
        {
            return Random.Range(0, pathMovers[i].odd * 1000) < 1000 * Time.fixedDeltaTime;
        }
    }
}
