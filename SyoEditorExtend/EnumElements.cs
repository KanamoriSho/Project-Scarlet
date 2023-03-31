using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

/* ネットの物を引っ張ってきて弄っただけなので理解しきれてないポイント多。
 * コメントが結構雑だけど許してほしい
 */

// Enumの要素に付けるAttribute
// PropertyAttributeではなくSystem.Attributeを継承する
[AttributeUsage(System.AttributeTargets.Field | AttributeTargets.Enum, AllowMultiple = false)]
public class EnumLabelAttribute : Attribute
{
    public string _displayName { get; private set; }        //表示されるEnumの要素名を格納する

    public string _labelName { get; private set; }          //表示されるEnumの名前を格納する

    public EnumLabelAttribute(string newDisplayName, string displayName)
    {
        _labelName = newDisplayName;    //引数からInspectorに表示する名前を受け取る

        _displayName = displayName;     //引数から要素名を受け取る
    }
}


[AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum, AllowMultiple = false)]
public class EnumElements : PropertyAttribute
{
    public Type _elementName { get; set; }     //呼び出し元スクリプト内のEnumの要素を取得

    public EnumElements(Type enumLabel)
    {
        _elementName = enumLabel;   //引数から要素名を取得
    }
}

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(EnumElements))]
public class EnumAttributeDrawer : PropertyDrawer
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
        EnumLabelAttribute elementAttribute = default;            //上記EnumElementAttribute格納用
        EnumElements enumLabelAttribute = attribute as EnumElements;      //上記EnumLabelを取得

        // FieldInfoから各要素のAttributeを取得し名前を得る
        var names = new List<string>();
        foreach (var fi in enumLabelAttribute._elementName.GetFields())
        {
            if (fi.IsSpecialName)
            {
                // SpecialNameは飛ばす
                continue;
            }

            //EnumElementAttributeを格納する
            elementAttribute = fi.GetCustomAttributes(typeof(EnumLabelAttribute), false).FirstOrDefault() as EnumLabelAttribute;

            //要素名を追加
            names.Add(elementAttribute == null ? fi.Name : elementAttribute._displayName);
        }

        string _labelName = elementAttribute._labelName;        //呼び出し元スクリプトからの引数を格納する。 Inspectorに表示する文字列。

        // 各要素の値はEnum.GetValues()で取得する
        IEnumerable<int> values = System.Enum.GetValues(enumLabelAttribute._elementName).Cast<int>();

        // 描画
        property.intValue = EditorGUI.IntPopup(position, _labelName, property.intValue, names.ToArray(), values.ToArray());
    }
}

#endif
