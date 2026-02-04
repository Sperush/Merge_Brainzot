using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using GIE;

[CustomEditor(typeof(GetItemEffect))]
public class GetItemEffectEditor : Editor
{
    private ReorderableList reorderablelist;
    GetItemEffect getItem;

    // Biến trạng thái cho Foldout
    bool showProperties_explosion;
    bool showProperties_jump;
    bool showProperties_fly;

    private void OnEnable()
    {
        getItem = (GetItemEffect)target;
        SerializedProperty prop = serializedObject.FindProperty("mGetItem");

        reorderablelist = new ReorderableList(serializedObject, prop, true, true, true, true);

        reorderablelist.drawHeaderCallback = (Rect rect) =>
        {
            GUI.Label(rect, "Items Configuration");
        };

        // Chiều cao item phải khớp với GetPropertyHeight trong Drawer
        reorderablelist.elementHeight = (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4 + 15;

        reorderablelist.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            SerializedProperty item = reorderablelist.serializedProperty.GetArrayElementAtIndex(index);
            // Dùng PropertyField để gọi xuống CustomPropertyDrawer bên trên
            EditorGUI.PropertyField(rect, item, new GUIContent("Item " + index), true);
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 1. Vẽ List Items
        reorderablelist.DoLayoutList();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Effect Settings", EditorStyles.boldLabel);

        // 2. Vẽ Explosion Settings
        // Lưu ý: Đã sửa tên biến từ Explostion -> Explosion
        showProperties_explosion = EditorGUILayout.BeginFoldoutHeaderGroup(showProperties_explosion, "Explosion");
        if (showProperties_explosion)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                // Sử dụng PropertyField thay vì vẽ tay để hỗ trợ Undo/Redo tốt hơn
                EditorGUILayout.PropertyField(serializedObject.FindProperty("mExplosionRadius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("mExplosionSpeed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("mExplosionFlySpeed"));
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // 3. Vẽ Jump Settings
        showProperties_jump = EditorGUILayout.BeginFoldoutHeaderGroup(showProperties_jump, "Jump");
        if (showProperties_jump)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("mJumpRadius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("mJumpHeight"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("mJumpToFlyDuration"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("mJumpSpeed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("mJumpFlySpeed"));
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        // 4. Vẽ Fly Settings
        showProperties_fly = EditorGUILayout.BeginFoldoutHeaderGroup(showProperties_fly, "Fly");
        if (showProperties_fly)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("mFlyRadius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("mFlySpeed"));
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        serializedObject.ApplyModifiedProperties();
    }
}