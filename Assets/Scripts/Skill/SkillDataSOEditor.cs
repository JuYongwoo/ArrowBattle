using UnityEditor;
using UnityEngine;


//SO를 다룰 때 깔끔하게 보이기 위한 커스텀 에디터 스크립트
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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillProjectile"), new GUIContent("Skill Projectile"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillIcon"), new GUIContent("Skill Icon"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillSound"), new GUIContent("Skill Sound"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillDamage"), new GUIContent("Skill Damage"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillCoolTime"), new GUIContent("Skill CoolTime"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillCastingTime"), new GUIContent("Skill CastingTime"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("projectileSpeed"), new GUIContent("Projectile Speed"));

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Skill Motion", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("skillProjectileMovingType"), new GUIContent("Skill Projectile Moving Type"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("projectileCount"), new GUIContent("Projectile Count"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isGuided"), new GUIContent("Is Guided"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isShowPortrait"), new GUIContent("Is Show Portrait"));

        serializedObject.ApplyModifiedProperties();
    }
}
