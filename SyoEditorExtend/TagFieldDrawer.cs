using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

//TagDrawer�Ăяo���p��PropertyAttribute�p���N���X
public class TagFieldDrawer : PropertyAttribute
{
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TagFieldDrawer))]
public class TagDrawer : PropertyDrawer
{
    //Unity������GUI�\�����\�b�h
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        property.stringValue = EditorGUI.TagField(position, label, property.stringValue);

        EditorGUI.EndProperty();
    }

}


#endif
