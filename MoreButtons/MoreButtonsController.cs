using HMUI;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace MoreButtons
{
    internal class MoreButtonsController : IAffinity
    {

        private static readonly int numberOffset = 256;
        private static readonly int stateSwitchStart = -10;
        private static readonly Dictionary<HMUI.UIKeyboard, int> keyboardState = new Dictionary<HMUI.UIKeyboard, int>();

        private static readonly List<List<char>> stateNumpads = new List<List<char>>()
        {
            new List<char>(){ '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' },
            new List<char>(){ '@', '&', '"', '\'', '!', ':', ';', '?', '.', ',' },
            new List<char>(){ '=', '+', '{', '}', '-', '[', ']', '*', '(', ')' },
            new List<char>(){ '$', '%', '~', '`', '#', '<', '>', '_', '/', '\\' }
        };
        private static readonly List<List<int>> stateSwitches = new List<List<int>>()
        {
            new List<int>(){ 1, 2 },
            new List<int>(){ 0, 3 },
            new List<int>(){ 0, 3 },
            new List<int>(){ 1, 2 }
        };
        private static readonly List<string> stateIcons = new List<string>()
        {
            "123",
            "?!",
            "+=",
            "/_"
        };


        [AffinityPatch(typeof(HMUI.UIKeyboard), nameof(HMUI.UIKeyboard.Awake))]
        [AffinityPrefix]
        public void UIKeyboard_Awake(HMUI.UIKeyboard __instance)
        {
            Transform numpad = __instance.transform.Find("Numpad");

            foreach (HMUI.UIKeyboardKey key in numpad.GetComponentsInChildren<HMUI.UIKeyboardKey>(true))
            {
                if ((int)key._keyCode >= numberOffset && (int)key._keyCode < numberOffset + 10)
                    key._keyCode = (UnityEngine.KeyCode)(-(int)key._keyCode + numberOffset);
            }

            Transform baseKey = numpad.Find("Row/0");
            var clone1 = UnityEngine.Object.Instantiate(baseKey);
            var clone2 = UnityEngine.Object.Instantiate(baseKey);

            Transform bottomRow = baseKey.parent;

            clone1.SetParent(bottomRow, false);
            clone2.SetParent(bottomRow, false);
            clone1.GetComponent<HMUI.UIKeyboardKey>()._keyCode = (KeyCode)stateSwitchStart;
            clone2.GetComponent<HMUI.UIKeyboardKey>()._keyCode = (KeyCode)(stateSwitchStart - 1);

            UnityEngine.UI.HorizontalLayoutGroup layout = bottomRow.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            layout.CalculateLayoutInputHorizontal();
            layout.SetLayoutHorizontal();

            keyboardState[__instance] = 0;
            UpdateKeyboardState(__instance);
        }


        [AffinityPatch(typeof(UIKeyboardManager), nameof(UIKeyboardManager.OpenKeyboardFor))]
        [AffinityPostfix]
        public void UIKeyboardManager_OpenKeyboardFor(UIKeyboardManager __instance, InputFieldView input)
        {
            keyboardState[__instance._uiKeyboard] = 0;
            UpdateKeyboardState(__instance._uiKeyboard);
        }

        [AffinityPatch(typeof(HMUI.UIKeyboard), nameof(HMUI.UIKeyboard.HandleKeyPress))]
        [AffinityPostfix]
        public void UIKeyboard_HandleKeyPress(HMUI.UIKeyboard __instance, KeyCode keyCode)
        {
            int castCode = (int)keyCode;
            if (castCode > 0 || !keyboardState.ContainsKey(__instance))
                return;

            if (castCode > stateSwitchStart)
                InvokeEvent(__instance, "keyWasPressedEvent", stateNumpads[keyboardState[__instance]][-castCode]);
            else
                SwitchKeyboardState(__instance, stateSwitchStart - castCode);
        }

        private void InvokeEvent(object instance, string eventName, object arg)
        {
            FieldInfo eventField = instance.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (eventField != null)
            {
                var eventDelegate = (MulticastDelegate)eventField.GetValue(instance);
                if (eventDelegate != null)
                {
                    foreach (var handler in eventDelegate.GetInvocationList())
                    {
                        handler.Method.Invoke(handler.Target, new object[] { arg });
                    }
                }
            }
        }

        public void SwitchKeyboardState(HMUI.UIKeyboard keyboard, int switchIdx)
        {
            int state = keyboardState[keyboard];
            keyboardState[keyboard] = stateSwitches[state][switchIdx];
            UpdateKeyboardState(keyboard);
        }


        void UpdateKeyboardState(HMUI.UIKeyboard keyboard)
        {
            Transform numpad = keyboard.transform.Find("Numpad");

            foreach (UIKeyboardKey key in numpad.GetComponentsInChildren<HMUI.UIKeyboardKey>(true))
            {
                int castCode = (int)key._keyCode;
                if (castCode > stateSwitchStart)
                {
                    string numpadString = new string(stateNumpads[keyboardState[keyboard]][-castCode], 1);
                    key._overrideText = numpadString;
                }
                else
                {
                    string icon = stateIcons[stateSwitches[keyboardState[keyboard]][stateSwitchStart - castCode]];
                    key._overrideText = icon;
                }
                key.Awake();
            }
        }
    }
}