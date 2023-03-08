using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Label : PropertyAttribute
{
    public readonly string _label;      //Inspectorに表示される文字列を格納する(読み取り専用)

    /// <summary>
    /// <para>Label</para>
    /// <para>呼び出された際の引数を_labelに代入する</para>
    /// </summary>
    /// <param name="label">Inspectorに表示される文字列 [Label("ここ")]</param>
    public Label(string label)
    {
        _label = label;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Label))]
public class LabelAttributeDrawer : PropertyDrawer
{

    /// <summary>
    /// <para>OnGUI</para>
    /// <para>UnityのGUI処理。 継承して追加の処理を入れて上書きする</para>
    /// </summary>
    /// <param name="position">Inspector上に表示される要素の画面上座標</param>
    /// <param name="property">Inspectorの要素 intの入力欄とか[SerializeField]で表示される奴。要するにInspectorに出てるやつ全部</param>
    /// <param name="label">Inspectorに表示する文字列</param>
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Label newLabel = attribute as Label;                        //上記Labelクラスを取得
        label.text = newLabel._label;                               //上記Labelクラスに引数のstringを渡す
        EditorGUI.PropertyField(position, property, label, true);   //Inspector上での表示を上書きする
                                                                    //(Inspector上の座標, 上書きする要素, 表示名, 表示の有無)
    }

    /// <summary>
    /// <para>GetPropertyHeight</para>
    /// <para>UnityのGUI処理。 ここでは変数の表示位置を返す処理に上書き</para>
    /// </summary>
    /// <param name="property">Inspectorの要素</param>
    /// <param name="label">Inspectorに表示する文字列</param>
    /// <returns></returns>
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, true);         //呼び出した変数のInspector座標を返す
    }
}
#endif
