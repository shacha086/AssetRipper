using AssetRipper.Primitives;

namespace AssetRipper.SerializationLogic.Tests;

public class SerializedFieldLayoutTests
{
	private class DerivedObject : UnityEngine.Object
	{
	}

	private class BehaviourWithExposedReferences : UnityEngine.MonoBehaviour
	{
		public UnityEngine.ExposedReference<UnityEngine.Object> exposedObject;
		public UnityEngine.ExposedReference<DerivedObject> exposedDerivedObject;
		public int followingField;
	}

	[TestCase("2018.4")]
	[TestCase("2019.4")]
	[TestCase("2020.1")]
	[TestCase("2021.3")]
	public void ExposedReferencesPreserveTheirFieldsBeforeAndAfterGenericSerializationSupport(string version)
	{
		SerializableType type = SerializableTypes.Create<BehaviourWithExposedReferences>(UnityVersion.Parse(version));
		Assert.That(type.Fields.Select(field => field.Name), Is.EqualTo(new[]
		{
			nameof(BehaviourWithExposedReferences.exposedObject),
			nameof(BehaviourWithExposedReferences.exposedDerivedObject),
			nameof(BehaviourWithExposedReferences.followingField),
		}));

		foreach (SerializableType.Field field in type.Fields.Take(2))
		{
			Assert.That(field.ArrayDepth, Is.Zero);
			Assert.That(field.Type.Fields.Select(child => child.Name), Is.EqualTo(new[] { "exposedName", "defaultValue" }));
			Assert.That(field.Type.Fields[0].Type.FullName, Is.EqualTo("UnityEngine.PropertyName"));
			Assert.That(field.Type.Fields[0].Type.IsEngineStruct(), Is.True);
			Assert.That(field.Type.Fields[1].Type, Is.SameAs(SerializablePointerType.Shared));
		}
		Assert.That(type.Fields[2].Type.Type, Is.EqualTo(PrimitiveType.Int));
	}

	[Serializable]
	private class ArrayWrapper
	{
		public DerivedObject[]? objects;
	}

	private class BehaviourWithCollections : UnityEngine.MonoBehaviour
	{
		public DerivedObject[][]? jaggedObjects;
		public List<DerivedObject[]>? listsOfArrays;
		public List<DerivedObject>[]? arraysOfLists;
		public List<List<DerivedObject>>? nestedLists;
		public DerivedObject[,]? multidimensionalObjects;
		public List<DerivedObject[,]>? listsOfMultidimensionalArrays;
		public DerivedObject[]? objects;
		public List<DerivedObject>? objectList;
		public List<ArrayWrapper>? wrappers;
		public int followingField;
	}

	[TestCase("2018.4")]
	[TestCase("2021.3")]
	public void UnsupportedContainersDoNotShiftTheFollowingFields(string version)
	{
		SerializableType type = SerializableTypes.Create<BehaviourWithCollections>(UnityVersion.Parse(version));
		Assert.That(type.Fields.Select(field => field.Name), Is.EqualTo(new[]
		{
			nameof(BehaviourWithCollections.objects),
			nameof(BehaviourWithCollections.objectList),
			nameof(BehaviourWithCollections.wrappers),
			nameof(BehaviourWithCollections.followingField),
		}));

		foreach (SerializableType.Field field in type.Fields.Take(2))
		{
			Assert.That(field.ArrayDepth, Is.EqualTo(1));
			Assert.That(field.Type, Is.SameAs(SerializablePointerType.Shared));
		}
		SerializableType.Field wrapper = type.Fields[2];
		Assert.That(wrapper.ArrayDepth, Is.EqualTo(1));
		Assert.That(wrapper.Type.Fields, Has.Count.EqualTo(1));
		Assert.That(wrapper.Type.Fields[0].ArrayDepth, Is.EqualTo(1));
		Assert.That(wrapper.Type.Fields[0].Type, Is.SameAs(SerializablePointerType.Shared));
		Assert.That(type.Fields[3].Type.Type, Is.EqualTo(PrimitiveType.Int));
	}
}
