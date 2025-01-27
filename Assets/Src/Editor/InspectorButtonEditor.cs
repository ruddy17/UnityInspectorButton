#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

namespace Antilatency {
    [CustomPropertyDrawer(typeof(InspectorButton))]
    public class InspectorButtonDrawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }

        private Action delayedAction;
        public void delay() {
            delayedAction();
        }

        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label) {

            SerializedObject O = prop.GetType().GetField("m_SerializedObject", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(prop) as SerializedObject;
            var TargetObjects = O.targetObjects;

            string MethodName = prop.name;
            char[] charsToTrim = { '_' };
            MethodName = MethodName.Trim(charsToTrim);
            MethodInfo TargetMethod = TargetObjects[0].GetType().GetMethods()
                .Where(x => x.Name == MethodName)
                .Where(x => x.ReturnType == typeof(void))
                .Where(x => x.GetParameters().Length == 0)
                .FirstOrDefault();

            if (TargetMethod == null) {
                GUI.color = Color.red;
                GUI.Label(pos, "Method " + MethodName + " not found.");
                GUI.color = Color.white;
                return;
            }

            SerializedProperty buttonColorProperty = O.FindProperty(prop.propertyPath + ".color");
            Color buttonColor = buttonColorProperty.colorValue;
            if (buttonColor == new Color(0, 0, 0, 0)) buttonColor = Color.white;
            GUI.color = buttonColor;

            if (GUI.Button(pos, ObjectNames.NicifyVariableName(MethodName))) {
                delayedAction = () => {
                    foreach (var o in TargetObjects) {
                        TargetMethod.Invoke(o, new object[0]);
                    }
                    SceneView.RepaintAll();
                };
                EditorApplication.delayCall += delay;
            }

            GUI.color = Color.white;
        }
    }
}
#endif
