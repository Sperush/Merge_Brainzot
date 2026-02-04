using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using GIE;

// --- PROPERTY DRAWER CHO CLASS ITEM ---
[CustomPropertyDrawer(typeof(Item))]
public class GetItemEffectDrawer : PropertyDrawer
{
    // Xác định chiều cao của mỗi phần tử trong list
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // 4 dòng property + khoảng cách padding
        return (EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing) * 4 + 10;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        using (new EditorGUI.PropertyScope(position, label, property))
        {
            // Tìm các property con
            var nameProp = property.FindPropertyRelative("mItemName");
            var templateProp = property.FindPropertyRelative("mItemTemplate");
            var toWhereProp = property.FindPropertyRelative("mItemToWhere");
            var numberProp = property.FindPropertyRelative("mCacheNumber");

            // Tính toán vị trí
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float currentY = position.y + 5; // Padding top

            Rect rect = new Rect(position.x, currentY, position.width, lineHeight);

            // Vẽ Name
            EditorGUI.PropertyField(rect, nameProp);

            // Vẽ Template
            rect.y += lineHeight + spacing;
            EditorGUI.PropertyField(rect, templateProp);

            // Vẽ To Where
            rect.y += lineHeight + spacing;
            EditorGUI.PropertyField(rect, toWhereProp);

            // Vẽ Cache Number (Dùng IntSlider cho trực quan)
            rect.y += lineHeight + spacing;
            numberProp.intValue = EditorGUI.IntSlider(rect, new GUIContent(numberProp.displayName), numberProp.intValue, 0, 100);
        }
    }
}