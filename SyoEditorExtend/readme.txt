ちょっとしたエディタ拡張

・Label
概要 : Inspector上での変数名の表示を日本語にできます。省スペース版Headerみたいな感じ。
Headerと組み合わせて使うと変数のカテゴライズとかに便利かも

[使い方]

[SerializeField, Label("スピード")]
private int _speed = 5;

と書くだけ。


・EnumLabel
概要 : Inspector上でのEnumの表示を日本語にできます。現状Enum専用。
若干実装雑なのでちょっと不便。

[使い方]

public enum ShotMoveType
{
	[EnumLabel("弾の動き方", "直線")]
	Straight,
	[EnumLabel("弾の動き方", "カーブ")]
	Curve,

}

[EnumElements(typeof(ShotMoveType))]
public ShotMoveType _shotMoveType;

と書く。

弾の動き方	▼直線
		　カーブ

と表示される。