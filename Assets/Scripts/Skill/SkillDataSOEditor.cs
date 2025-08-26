using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkillDataSO))]
public class SkillDataSOEditor : Editor
{
    private void OnEnable()
    {
        serializedObject.Update();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Skill Base Info", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillName"), new GUIContent("Skill Name"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillDamage"), new GUIContent("Skill Damage"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillCooldown"), new GUIContent("Skill Cooldown"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillSound"), new GUIContent("Skill Sound"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillProjectile"), new GUIContent("Skill Projectile"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillIcon"), new GUIContent("Skill Icon"));

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Skill Motion", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("projectileSpeed"), new GUIContent("Projectile Speed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isShowPortrait"), new GUIContent("Is Show Portrait"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isGuided"), new GUIContent("Is Guided"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillProjectileMovingType"), new GUIContent("Skill Projectile Moving Type"));

        serializedObject.ApplyModifiedProperties();
    }
}
