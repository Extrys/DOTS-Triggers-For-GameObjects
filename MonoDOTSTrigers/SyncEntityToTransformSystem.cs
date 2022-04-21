using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Jobs;

public struct SyncEntityToTransformTag : IComponentData { }

public class SyncEntityToTransformSystem : SystemBase
{
	EntityQuery query;
	protected override void OnCreate()
	{
		query = EntityManager.CreateEntityQuery(typeof(Transform), ComponentType.ReadOnly<SyncEntityToTransformTag>(), typeof(Translation), typeof(Rotation));
	}
	protected override void OnUpdate()
	{
		TransformAccessArray transformAccessArray = query.GetTransformAccessArray();
		Dependency = new SynchronizationJob
		{
			entities = query.ToEntityArray(Allocator.TempJob),
			translationAccessor = GetComponentDataFromEntity<Translation>(),
			rotationAccessor = GetComponentDataFromEntity<Rotation>(),
			transformAccessArrayToDealocate = transformAccessArray
		}.ScheduleReadOnly(transformAccessArray, 4, Dependency);
	}


	[BurstCompile]
	public struct SynchronizationJob : IJobParallelForTransform
	{
		[DeallocateOnJobCompletion, ReadOnly] public TransformAccessArray transformAccessArrayToDealocate;
		[DeallocateOnJobCompletion] public NativeArray<Entity> entities;
		[NativeDisableParallelForRestriction]
		public ComponentDataFromEntity<Translation> translationAccessor;
		[NativeDisableParallelForRestriction]
		public ComponentDataFromEntity<Rotation> rotationAccessor;
		public void Execute(int index, TransformAccess transform)
		{
			Entity e = entities[index];
			Translation translation = translationAccessor[e];
			Rotation rotation = rotationAccessor[e];
			translation.Value = transform.position;
			rotation.Value = transform.rotation;
			translationAccessor[e] = translation;
			rotationAccessor[e] = rotation;
		}
	}

}
