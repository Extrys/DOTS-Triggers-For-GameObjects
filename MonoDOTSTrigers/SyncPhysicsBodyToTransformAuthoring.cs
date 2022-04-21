using HybridEZS;
using System;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Transforms;
using DOTS_CapsuleCollider = Unity.Physics.CapsuleCollider;
using DOTS_Collider = Unity.Physics.Collider;
using DOTS_SphereCollider = Unity.Physics.SphereCollider;
using DOTS_BoxCollider = Unity.Physics.BoxCollider;

public class SyncPhysicsBodyToTransformAuthoring : SyncEntityToTransformAuthoring
{
	public EntityInjector entityInjector;
	public PhysicsShapeAuthoring physicsShape;

	new private void Reset()
	{
		base.Reset();
		physicsShape = gameObject.GetOrCreateComponent<PhysicsShapeAuthoring>();
		entityInjector = gameObject.GetComponent<EntityInjector>();
		if (entityInjector.objects != null && !entityInjector.objects.Contains(transform))
		{
			var list = entityInjector.objects.ToList();
			list.Add(transform);
			entityInjector.objects = list.ToArray();
		}
		else if (entityInjector.objects == null)
			entityInjector.objects = new UnityEngine.Object[] { transform };
	}

	public override SyncEntityToTransformTag GetAuthoredComponentData() => new SyncEntityToTransformTag();

	public override void InsertAuthoredComponentToEntity(Entity entity)
	{
		base.InsertAuthoredComponentToEntity(entity);


		var collisionFilter = new CollisionFilter
		{
			BelongsTo = physicsShape.BelongsTo.Value,
			CollidesWith = physicsShape.CollidesWith.Value,
			GroupIndex = 1
		};

		BlobAssetReference<DOTS_Collider> collider = default;
		if(physicsShape.ShapeType == ShapeType .Sphere)
			collider = DOTS_SphereCollider.Create(physicsShape.GetSphereProperties(out _), collisionFilter);
		if (physicsShape.ShapeType == ShapeType.Capsule)
			collider = DOTS_CapsuleCollider.Create(physicsShape.GetCapsuleProperties().ToRuntime(), collisionFilter);
		if (physicsShape.ShapeType == ShapeType.Box)
			collider = DOTS_BoxCollider.Create(physicsShape.GetBoxProperties(), collisionFilter);


		entityInjector.Manager.AddComponentData(entity, new PhysicsCollider { Value = collider });
		entityInjector.Manager.AddComponentData(entity, new PhysicsVelocity { Angular = 0, Linear = 0 });
		entityInjector.Manager.AddComponentData(entity, new Translation() { Value = transform.position});
		entityInjector.Manager.AddComponentData(entity, new Rotation() { Value = transform.rotation });
	}

}
