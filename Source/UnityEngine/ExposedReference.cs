namespace UnityEngine;

[Serializable]
public struct ExposedReference<T> where T : Object
{
	public PropertyName exposedName;
	public T? defaultValue;
}
