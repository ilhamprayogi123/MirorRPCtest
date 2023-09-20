using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace razz
{
    [CustomEditor(typeof(AnimAssist))]
    public class AnimAssistEditor : Editor
    {
        private AnimAssist _script;
        private Animator _animator;
        private AnimatorController _animatorController;
        private int _interactorLayerIndex;
        private int _paramIndex;
        private AnimatorControllerLayer _interactorLayer;
        private AnimatorStateMachine _stateMachine;

        private void OnEnable()
        {
            if (!_script) _script = (AnimAssist)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Space(10f);

            if (!Application.isPlaying)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Add InteractorLayer", "This adds all animation clips to the Animator Controller as a new InteractorLayer but deletes the previous InteractorLayer if it exist. Also adds a parameter named as animAssistSpeed to control clip speeds from InteractorObject settings."))) AddInteractorLayer();
                if (GUILayout.Button(new GUIContent("Remove InteractorLayer","Removes InteractorLayer and animAssistSpeed parameter."))) RemoveInteractorLayer();
                GUILayout.EndHorizontal();
            }
        }
        private bool Init()
        {
            if (!_animator) _animator = _script.animator;
            if (_animator == null)
            {
                Debug.LogWarning("Animator is not assigned on AnimAssist component!", _script);
                return false;
            }

            if (!_animatorController) _animatorController = _animator.runtimeAnimatorController as AnimatorController;
            if (_animator == null)
            {
                Debug.LogWarning("Animator Controller is not assigned on Animator component!", _script);
                return false;
            }

            return true;
        }
        private bool CheckAnimAssistParameter()
        {
            _paramIndex = -1;
            AnimatorControllerParameter[] parameters = _animatorController.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].name == _script.speedParam)
                {
                    _paramIndex = i;
                    return true;
                }
            }
            return false;
        }
        private bool CheckInteractorLayer()
        {
            _interactorLayerIndex = -1;
            for (int i = 0; i < _animatorController.layers.Length; i++)
            {
                if (_animatorController.layers[i].name == _script.interactorLayerName)
                {
                    _interactorLayerIndex = i;
                    break;
                }
            }
            return (_interactorLayerIndex >= 0);
        }
        private void AddInteractorLayer()
        {
            if (!Init()) return;
            if (_script.animationClips.Length == 0)
            {
                Debug.Log("There is no AnimClips on AnimAssist component.", _script);
                return;
            }

            AddLayer();
            AddAnimClips();
        }
        private void AddLayer()
        {
            RemoveInteractorLayer();
            if (!CheckAnimAssistParameter()) AddParameter();

            _animatorController.AddLayer(_script.interactorLayerName);
            _interactorLayerIndex = _animatorController.layers.Length - 1;
            _script.interactorLayerIndex = _interactorLayerIndex;
            _interactorLayer = _animatorController.layers[_interactorLayerIndex];
            _stateMachine = _interactorLayer.stateMachine;
            Debug.Log("Interactor Layer is added to Animator Controller.");

            EditorUtility.SetDirty(_animatorController);
            AssetDatabase.SaveAssets();
        }
        private void AddParameter()
        {
            AnimatorControllerParameter parameter = new AnimatorControllerParameter
            {
                name = _script.speedParam,
                type = AnimatorControllerParameterType.Float
            };
            parameter.defaultFloat = 1f;
            _animatorController.AddParameter(parameter);
        }
        private void AddAnimClips()
        {
            Vector3 layerOffset = new Vector3(250f, 125f, 0);
            AnimatorState newDefaultState = _stateMachine.AddState("Wait for Interaction", layerOffset);
            _stateMachine.defaultState = newDefaultState;
            if (_script.animationClips == null)
            {
                Debug.LogWarning("Animation Clips is null.", this);
                return;
            }

            RemoveNullClips();
            for (int i = 0; i < _script.animationClips.Length; i++)
            {
                AnimatorState addedState = _stateMachine.AddState(_script.animationClips[i].name, layerOffset + new Vector3(275, i * 50, 0f));
                addedState.motion = _script.animationClips[i];
                addedState.speedParameterActive = true;
                addedState.speedParameter = _script.speedParam;
                addedState.AddExitTransition(true);
            }
            if (_script.animationClips.Length > 0) Debug.Log("All animation clips are added as states to Interactor Layer on Animator Controller.");

            EditorUtility.SetDirty(_animatorController);
            AssetDatabase.SaveAssets();
        }
        private void RemoveNullClips()
        {
            int valid = 0;
            for (int i = 0; i < _script.animationClips.Length; i++)
            {
                if (_script.animationClips[i] != null)
                {
                    if (i != valid) _script.animationClips[valid] = _script.animationClips[i];
                    valid++;
                }
            }
            if (valid < _script.animationClips.Length)
            {
                Array.Resize(ref _script.animationClips, valid);
                Debug.Log("Removed null clips from AnimAssist.");
            }
        }
        private void RemoveInteractorLayer()
        {
            if (!Init()) return;

            if (CheckInteractorLayer())
            {
                _animatorController.RemoveLayer(_interactorLayerIndex);
                _script.interactorLayerIndex = -1;
                Debug.Log("Interactor Layer is removed from Animator Controller.");
            }
            if (CheckAnimAssistParameter())
            {
                _animatorController.RemoveParameter(_paramIndex);
            }

            EditorUtility.SetDirty(_animatorController);
            AssetDatabase.SaveAssets();
        }
    }
}
