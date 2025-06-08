using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Scripts.Systems {
    public static class InputActionStack {
        /// <summary>
        ///     Set of objects that are currently masking input, preventing any action
        ///     maps from being enabled.
        /// </summary>
        private static readonly HashSet<object> _inputMasks = new();

        /// <summary>
        ///     Stack used to manage the hierarchy of input control requests, storing
        ///     both the requested map type and the requester.
        /// </summary>
        private static readonly Stack<(InputActionMap map, object requester)> _controlStack = new();

        /// <summary>
        ///     A set of objects that request to mask (disable) the EventSystem.
        /// </summary>
        private static readonly HashSet<object> _eventSystemMasks = new();

        private static EventSystem _maskedEventSystem;

        /// <summary>
        ///     Gets the current active input map type, defaulting to StaticOnly if the
        ///     stack is empty.
        /// </summary>
        public static InputActionMap CurrentInputMap =>
            _controlStack.Count > 0 ? _controlStack.Peek().map : null;

        /// <summary>
        ///     Pushes a new input map onto the stack, making it the current active map.
        /// </summary>
        /// <param name="map">Type of the input map to push.</param>
        /// <param name="requester">
        ///     The object requesting the push, used for
        ///     ensuring proper pop operations.
        /// </param>
        public static void PushInputMap(InputActionMap map, object requester) {
            // Disable the current map if any
            if (_controlStack.Count > 0) CurrentInputMap?.Disable();
            _controlStack.Push((map, requester));
            if (_inputMasks.Count == 0) CurrentInputMap?.Enable();
        }

        /// <summary>
        ///     Pops the top input map from the stack if the requester matches,
        ///     reverting to the previous input map.
        /// </summary>
        /// <param name="requester">
        ///     The object that must match the current top of
        ///     the stack to allow the pop.
        /// </param>
        public static void PopInputMap(object requester) {
            if (_controlStack.Count == 0)
                throw new InvalidOperationException(
                    "[MyInputActions] Cannot pop. Control stack is empty.");

            if (_controlStack.Peek().requester != requester) {
                Debug.LogError($"[MyInputActions] Stack top {_controlStack.Peek()} was not pushed by {requester}.");
            } else {
                // Disable previous map
                _controlStack.Pop().map?.Disable();
                if (_controlStack.Count == 0) {
                    Debug.LogWarning("[MyInputActions] Control stack empty. All input maps disabled.");
                } else {
                    // Enable new map if no mask is in place
                    if (_inputMasks.Count == 0) CurrentInputMap?.Enable();
                }
            }
        }

        /// <summary>
        ///     Masks input, disabling the current active input map and preventing new
        ///     ones from being enabled.
        /// </summary>
        /// <param name="masker">The object requesting the mask.</param>
        public static void MaskInput(object masker) {
            _inputMasks.Add(masker);
            CurrentInputMap?.Disable();
        }

        /// <summary>
        ///     Unmasks input, possibly re-enabling the current active input map if no other masks are present.
        /// </summary>
        /// <param name="masker">The object that requested the mask to be removed.</param>
        public static void UnmaskInput(object masker) {
            _inputMasks.Remove(masker);
            if (_inputMasks.Count == 0) CurrentInputMap?.Enable();
        }

        /// <summary>
        ///     Requests to disable the EventSystem.
        /// </summary>
        /// <param name="masker">The object requesting the mask.</param>
        public static void MaskEventSystem(object masker) {
            // Add the masker to the set
            _eventSystemMasks.Add(masker);

            // Check if this is the first masker added, disable the EventSystem if so
            if (_eventSystemMasks.Count == 1) {
                _maskedEventSystem = EventSystem.current;
                EventSystem.current.enabled = false;
            }
        }

        /// <summary>
        ///     Requests to re-enable the EventSystem. Only re-enables it if no other masks are present.
        /// </summary>
        /// <param name="masker">The object requesting to remove its mask.</param>
        public static void UnmaskEventSystem(object masker) {
            // Remove the masker from the set
            _eventSystemMasks.Remove(masker);

            // Check if there are no more maskers, enable the EventSystem if so
            if (_eventSystemMasks.Count == 0 && _maskedEventSystem != null) _maskedEventSystem.enabled = true;
        }
    }
}