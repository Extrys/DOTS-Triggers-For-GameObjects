using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;



[UpdateInGroup(typeof(CustomTriggerEventGroup))]
public class InTriggerEventProcessSystem : SystemBase
{
	private StepPhysicsWorld m_StepPhysicsWorld = default;
	private BuildPhysicsWorld m_BuildPhysicsWorld = default;
	protected override void OnCreate()
	{
		m_StepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
		m_BuildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
	}

	protected override void OnUpdate()
	{
		var physicsWorld = m_BuildPhysicsWorld.PhysicsWorld;
		Dependency = new CustomCollectTriggerEventsJob
		{
			entitiesInside = GetBufferFromEntity<InTriggerEvent>()
		}.Schedule(m_StepPhysicsWorld.Simulation, ref physicsWorld, Dependency);
	}
}

//[UpdateInGroup(typeof(CustomTriggerEventGroup)),UpdateAfter(typeof(InTriggerEventProcessSystem))]
//public class TriggerUpdateCheckEnabler: SystemBase
//{
//	public EndFixedStepSimulationEntityCommandBufferSystem ecbSystem;
//	protected override void OnCreate()
//	{
//		ecbSystem = World.GetOrCreateSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
//	}
//	protected override void OnUpdate()
//	{
//		EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer();
//		Entities.WithNone<ShouldUpdateTriggerEventsTag>().ForEach((Entity e, DynamicBuffer<InTriggerEvent> inTriggers) =>
//		{
//			if (inTriggers.Length > 0)
//				ecb.AddComponent<ShouldUpdateTriggerEventsTag>(e);
//		}).WithBurst().ScheduleParallel();
//	}
//}

//[UpdateInGroup(typeof(CustomTriggerEventGroup)), UpdateAfter(typeof(InTriggerEventProcessSystem))]
//public class TriggerUpdateCheckDisabler : SystemBase
//{
//	public EndFixedStepSimulationEntityCommandBufferSystem ecbSystem;
//	protected override void OnCreate()
//	{
//		ecbSystem = World.GetOrCreateSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
//	}
//	protected override void OnUpdate()
//	{
//		EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer();
//		Entities.WithAll<ShouldUpdateTriggerEventsTag>().ForEach((Entity e, DynamicBuffer<InTriggerEvent> inTriggers) =>
//		{
//			if (inTriggers.Length == 0)
//				ecb.RemoveComponent<ShouldUpdateTriggerEventsTag>(e);
//		}).WithBurst().ScheduleParallel();
//	}
//}

//[UpdateInGroup(typeof(CustomTriggerEventGroup)), UpdateAfter(typeof(InTriggerEventProcessSystem)), UpdateBefore(typeof(CustomTriggerEventGroup))]
//public class TriggerUpdateCheckEnabler : SystemBase
//{
//	public EndFixedStepSimulationEntityCommandBufferSystem ecbSystem;
//	protected override void OnCreate()
//	{
//		ecbSystem = World.GetOrCreateSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
//	}
//	protected override void OnUpdate()
//	{
//		EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer();
//		Dependency = Entities.WithNone<ShouldUpdateTriggerEventsTag>().ForEach((Entity e, in DynamicBuffer<InTriggerEvent> inTriggers) =>
//		{
//			if (inTriggers.Length > 0)
//				ecb.AddComponent<ShouldUpdateTriggerEventsTag>(e);
//		}).WithDisposeOnCompletion(ecb).WithBurst().Schedule(Dependency);
//		Dependency.Complete();
//	}
//}
//[UpdateInGroup(typeof(CustomTriggerEventGroup)), UpdateAfter(typeof(InTriggerEventProcessSystem)), UpdateBefore(typeof(CustomTriggerEventGroup))]
//public class TriggerUpdateCheckDisabler : SystemBase
//{
//	public EndFixedStepSimulationEntityCommandBufferSystem ecbSystem;
//	protected override void OnCreate()
//	{
//		ecbSystem = World.GetOrCreateSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
//	}
//	protected override void OnUpdate()
//	{

//		EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer();
//		Dependency = Entities.WithAll<ShouldUpdateTriggerEventsTag>().ForEach((Entity e, in DynamicBuffer<InTriggerEvent> inTriggers) =>
//		{
//			if (inTriggers.Length == 0)
//				ecb.RemoveComponent<ShouldUpdateTriggerEventsTag>(e);
//		}).WithDisposeOnCompletion(ecb).WithBurst().Schedule(Dependency);
//		Dependency.Complete();
//	}
//}
