using HybridEZS;
using System;
using System.Threading;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Events;
using DOTS_BoxCollider = Unity.Physics.BoxCollider;
using DOTS_Collider = Unity.Physics.Collider;
using DOTS_PhysicsMaterial = Unity.Physics.Material;
using DOTS_SphereCollider = Unity.Physics.SphereCollider;

[RequireComponent(typeof(PhysicsShapeAuthoring), typeof(EntityInjector))]
public class EntityTriggerEvents : ComponentDataAuthor, IEntityConverted, IStateRestorable
{
	[HideInInspector] public EntityInjector entityInjector;
	public Entity PrimaryEntity => entityInjector.PrimaryEntity;


	public UnityEvent onTriggerEnter;
	public UnityEvent<Entity> onTriggerEnterWithEntity;
	public bool oneShotEnter;
	public bool skipEnterEvents;

	public UnityEvent onTriggerExit;
	public UnityEvent<Entity> onTriggerExitWithEntity;
	public bool oneShotExit;
	public bool skipExitEvents;

	public bool SkipEnterEvents { get => skipEnterEvents; set => skipEnterEvents = value; }
	public bool SkipExitEvents { get => skipExitEvents; set => skipExitEvents = value; }

	public bool SkipEvents { get => skipEnterEvents || skipExitEvents; set => skipEnterEvents = (skipExitEvents = value); }

	public async UniTaskVoid OnEntityTriggerEnter(Entity other)
	{
		await UniTask.Yield();
		if (skipEnterEvents) return;
		if (oneShotEnter) skipEnterEvents = true;
		onTriggerEnter?.Invoke();
		onTriggerEnterWithEntity?.Invoke(other);
	}
	public async UniTaskVoid OnEntityTriggerExit(Entity other)
	{
		await UniTask.Yield();
		if (skipExitEvents) return;
		if (oneShotExit) skipExitEvents = true;
		onTriggerExit?.Invoke();
		onTriggerExitWithEntity?.Invoke(other);
	}

	PhysicsShapeAuthoring physicsShape;

	private bool HasInvalidOrientations()
	{
		if (!physicsShape)
			physicsShape = GetComponent<PhysicsShapeAuthoring>();

		switch (physicsShape.ShapeType)
		{
			case ShapeType.Capsule:
				return !physicsShape.GetCapsuleProperties().Orientation.Equals(quaternion.identity);
			case ShapeType.Sphere:
				physicsShape.GetSphereProperties(out quaternion quat);
				return !quat.Equals(quaternion.identity);
			case ShapeType.Cylinder:
				return !physicsShape.GetCylinderProperties().Orientation.Equals(quaternion.identity);
			case ShapeType.Plane:
				physicsShape.GetPlaneProperties(out _, out _, out quaternion q);
				return !q.Equals(quaternion.identity);
			case ShapeType.Box:
				return !physicsShape.GetBoxProperties().Orientation.Equals(quaternion.identity);
			default: Debug.LogError("Es recomendable no usar meshes para colliders", gameObject); return false;
		}
	}

	void Awake()
	{
		if (!entityInjector)
			Debug.LogError("This EntityTriggerEvents has not entityInjector, do a reset");

		if (HasInvalidOrientations())
			Debug.LogError("El collider de entity del objeto no tiene la rotacion en 0 0 0", gameObject);

	}
	new void Reset()
	{
		base.Reset();
		entityInjector = GetComponent<EntityInjector>();
		if (physicsShape == null)
		{
			physicsShape = GetComponent<PhysicsShapeAuthoring>();
			physicsShape.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;
			physicsShape.BelongsTo = new PhysicsCategoryTags { Category00 = true };
			physicsShape.CollidesWith = new PhysicsCategoryTags { Category01 = true };
		}
	}

	[ContextMenuItem("Fix")]
	public void Fix()
	{
		if (entityInjector == null)
			entityInjector = gameObject.GetOrCreateComponent<EntityInjector>();

		if (entityInjector.dataAuthors.Exists(x => x == this))
			return;

		entityInjector.dataAuthors.Add(this);
	}

	CancellationTokenSource cancellation = new CancellationTokenSource();
	bool settingPhysics, finalPhysicsValue;
	async UniTaskVoid DelayedSetPhysicsState(bool state, CancellationToken cancellationToken)
	{
		finalPhysicsValue = state;
		if (settingPhysics)
			return;
		settingPhysics = true;
		await UniTask.Yield(cancellationToken);
		if (cancellation.IsCancellationRequested)
			return;

		if (finalPhysicsValue)
			PrimaryEntity.TryRemoveComponentData<PhysicsExclude>();
		else
		{
			if (World.DefaultGameObjectInjectionWorld != null)
				PrimaryEntity.TryAddComponentData<PhysicsExclude>();
		}
		settingPhysics = false;
	}
	void OnEnable()
	{
		DelayedSetPhysicsState(true, cancellation.Token).Forget();
	}
	void OnDisable()
	{
		if (World.DefaultGameObjectInjectionWorld != null)
			DelayedSetPhysicsState(false, cancellation.Token).Forget();
	}
	void OnDestroy()
	{
		cancellation.Cancel();
		var world = World.DefaultGameObjectInjectionWorld;
		if (world == null)
			return;

		EntityCommandBuffer ecb = world.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer();
		ecb.DestroyEntity(PrimaryEntity);
		//World.DefaultGameObjectInjectionWorld?.EntityManager.DestroyEntity(PrimaryEntity);
	}


	public override void InsertAuthoredComponentToEntity(Entity entity)
	{
		physicsShape = GetComponent<PhysicsShapeAuthoring>();

		EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;

		manager.AddBuffer<InTriggerEvent>(entity);
		manager.AddComponentData(entity, new InTriggerEventGroups());
		manager.AddComponentData(entity, new Translation() { Value = transform.position });
		manager.AddComponentData(entity, new Rotation() { Value = transform.rotation });
		manager.AddComponentData(entity, new PhysicsVelocity { Angular = 0, Linear = 0 });
		manager.AddComponentData(entity, new PhysicsCollider { Value = CreateColliderFromPhysicsShape(physicsShape) });

		manager.AddComponentObject(entity, this);
		manager.AddComponentObject(entity, transform);

#if UNITY_EDITOR
		manager.SetName(PrimaryEntity, name);
#endif

	}



	BlobAssetReference<DOTS_Collider> CreateColliderFromPhysicsShape(PhysicsShapeAuthoring shape)
	{
		DOTS_PhysicsMaterial mat = new DOTS_PhysicsMaterial { CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents };
		var collisionFilter = new CollisionFilter
		{
			BelongsTo = shape.BelongsTo.Value,
			CollidesWith = shape.CollidesWith.Value,
		};

		BlobAssetReference<DOTS_Collider> collider;
		BoxGeometry boxGeometry = shape.GetBoxProperties();
		boxGeometry.Center *= transform.lossyScale;
		boxGeometry.Size *= transform.lossyScale;
		boxGeometry.Orientation = quaternion.identity;

		collider = DOTS_BoxCollider.Create(boxGeometry, collisionFilter, mat);


		switch (shape.ShapeType)
		{
			case ShapeType.Sphere:
				SphereGeometry sphereGeometry = shape.GetSphereProperties(out _);
				sphereGeometry.Radius *= math.min(transform.localScale.x, math.min(transform.localScale.y, transform.localScale.z));
				collider = DOTS_SphereCollider.Create(sphereGeometry, collisionFilter, mat);
				break;
			case ShapeType.Cylinder:
			case ShapeType.Plane:
			case ShapeType.Capsule:
				Debug.LogError("Trigger geometry type is not supported yet", gameObject);
				break;
		}
		return collider;
	}




	Data data;
	public bool Disabled => false;
	public IStateRestorable.IData RestorableData { get => data; set => data = (Data)value; }
	public void RestoreSavedData() { skipEnterEvents = data.skipEnterEvents; skipExitEvents = data.skipExitEvents; }
	public void StoreCurrentStateData() { data.skipEnterEvents = skipEnterEvents; data.skipEnterEvents = skipEnterEvents; }



	public struct Data : IStateRestorable.IData
	{
		public bool skipEnterEvents;
		public bool skipExitEvents;
	}
}


public struct InTriggerEvent : IBufferElementData, IComparable<InTriggerEvent>
{
	public Entity other;
	public int state;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int CompareTo(InTriggerEvent other) => math.clamp(other.state - state, -1, 1);
}


public struct InTriggerEventGroups : IComponentData
{
	//0 = enters, 1 = stays, 2 = exits, 3 = delete
	public int4 stateCounts;
}

