using UnityEngine;
using System.Collections.Generic;

namespace razz
{
    [CreateAssetMenu(fileName = "ClimbableSettings", menuName = "Interactor/ClimbableSettings")]
    public class Climbable : InteractionTypeSettings
    {
        [Tooltip("How much player will be pushed forward on top. Other adjustments can be made on PlayerController script which handles the climbing.")]
        public float topPush = 0.4f;

        //Gets the closest target for same object to given effector
        private Transform ClosestTargetSameEffector(InteractorObject intObj, Interactor.FullBodyBipedEffector effectorType, Vector3 effectorWorldSpace)
        {
            List<Transform> targets = intObj.GetTargetTransformsForEffectorType((int)effectorType);
            if (targets.Count == 0) return null;

            float shortestSqrDist = 25f;
            int targetPointer = -1;
            float distanceSqr;

            for (int i = 0; i < targets.Count; i++)
            {
                distanceSqr = (effectorWorldSpace - targets[i].position).sqrMagnitude;

                if (distanceSqr < shortestSqrDist)
                {
                    shortestSqrDist = distanceSqr;
                    targetPointer = i;
                }
            }

            if (targetPointer < 0) return null;
            else return targets[targetPointer];
        }

        //Gets the farthest target for same object to given effector
        private Transform FarthestTargetSameEffector(InteractorObject intObj, Interactor.FullBodyBipedEffector effectorType, Vector3 effectorWorldSpace)
        {
            List<Transform> targets = intObj.GetTargetTransformsForEffectorType((int)effectorType);
            if (targets.Count == 0) return null;

            float farthestSqrDist = 0;
            int targetPointer = -1;
            float distanceSqr;

            for (int i = 0; i < targets.Count; i++)
            {
                distanceSqr = (effectorWorldSpace - targets[i].position).sqrMagnitude;

                if (distanceSqr > farthestSqrDist)
                {
                    farthestSqrDist = distanceSqr;
                    targetPointer = i;
                }
            }

            if (targetPointer < 0) return null;
            else return targets[targetPointer];
        }

        //Moves player to center and rotates full forward when starting to climb at bottom
        public void ReposClimbingPlayerBottom(InteractorObject intObj, Vector3 effectorWorldSpace)
        {
            Transform closeLeftHand = ClosestTargetSameEffector(intObj, Interactor.FullBodyBipedEffector.LeftHand, effectorWorldSpace);
            Transform closeRightHand = ClosestTargetSameEffector(intObj, Interactor.FullBodyBipedEffector.RightHand, effectorWorldSpace);
            if (!closeLeftHand || !closeRightHand) return;

            Vector3 lh = closeLeftHand.position;
            Vector3 rh = closeRightHand.position;

            Vector3 handsDir = rh - lh;
            Vector3 handsMiddle = (lh + rh) * 0.5f;
            handsMiddle.y = intObj.currentInteractor.playerTransform.position.y;
            Vector3 perpendicular = Vector3.Cross(Vector3.up * 0.5f, handsDir);

            Debug.DrawRay(handsMiddle, perpendicular, Color.red, 10f);

            intObj.currentInteractor.interactionStates.targetPosition = handsMiddle + perpendicular;
            intObj.currentInteractor.interactionStates.targetRotation = Quaternion.LookRotation(-perpendicular, Vector3.up);
            intObj.currentInteractor.interactionStates.rePos = true;
        }

        //Moves player a little forward when ending climb at top
        public void ReposClimbingPlayerTop(InteractorObject intObj, Vector3 effectorWorldSpace)
        {
            Transform farLeftHand = FarthestTargetSameEffector(intObj, Interactor.FullBodyBipedEffector.LeftHand, effectorWorldSpace);
            Transform farRightHand = FarthestTargetSameEffector(intObj, Interactor.FullBodyBipedEffector.RightHand, effectorWorldSpace);
            if (!farLeftHand || !farRightHand) return;

            //Since we're starting to climb at bottom, farthest targets are top
            Vector3 lh = farLeftHand.position;
            Vector3 rh = farRightHand.position;

            Vector3 handsDir = rh - lh;
            //Top middle point based on left and right hand targets
            Vector3 handsMiddle = (lh + rh) * 0.5f;
            //Perpendicular angle which is player forward direction and its amount
            Vector3 perpendicular = Vector3.Cross(Vector3.up * -topPush, handsDir);

            Debug.DrawRay(handsMiddle, perpendicular, Color.red, 10f);
            //Sets top posiiton at player state so PlayerController gets from there
            intObj.currentInteractor.interactionStates.targetTopPosition = handsMiddle + perpendicular;
            intObj.currentInteractor.interactionStates.targetTopRotation = Quaternion.LookRotation(perpendicular, Vector3.up);
        }
    }
}
