using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

//TagDrawer呼び出し用のPropertyAttribute継承クラス
public class TagFieldDrawer : PropertyAttribute
{
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(TagFieldDrawer))]
public class TagDrawer : PropertyDrawer
{
    //Unity既存のGUI表示メソッド
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        property.stringValue = EditorGUI.TagField(position, label, property.stringValue);

        EditorGUI.EndProperty();
    }

}


#endif
