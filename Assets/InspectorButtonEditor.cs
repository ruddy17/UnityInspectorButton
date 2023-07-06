using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Antilatency.UnityPrototypingTools {

    [CustomPropertyDrawer(typeof(InspectorButton))]
    public class InspectorButtonDrawer : PropertyDrawer {

        public override bool CanCacheInspectorGUI(SerializedProperty property) {
            return true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var targets = property.serializedObject.targetObjects;
            if (targets == null || targets.Length == 0)
                return;

            var methodName = property.name.TrimStart('_');
            var declaringType = this.fieldInfo.DeclaringType;
            var targetMethods = new List<MethodInfo>(targets.Length);
            foreach (var target in targets) {
                var targetMethod = declaringType.GetMethod(methodName,
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if (targetMethod != null)
                    targetMethods.Add(targetMethod);
            }

            if (targetMethods.Count != targets.Length) {
                var msg = string.Format("Method '{0}' not found", methodName);
                EditorGUI.HelpBox(position, msg, MessageType.Error);
                return;
            }

            position.xMin += EditorGUIUtility.labelWidth;

            var nicerName = ObjectNames.NicifyVariableName(methodName);
            if (GUI.Button(position, nicerName)) {
                for (int i = 0; i < targets.Length; i++) {
                    targetMethods[i].Invoke(targets[i], null);
                }

                // In most use cases it's handy to repaint gizmos after
                // the button clicked.
                SceneView.RepaintAll();
            }
        }
    }
}
