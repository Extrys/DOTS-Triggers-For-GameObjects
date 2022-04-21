using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

[BurstCompile]
public struct CustomCollectTriggerEventsJob : ITriggerEventsJob
{
	[NativeDisableParallelForRestriction]
	public BufferFromEntity<InTriggerEvent> entitiesInside;

	public void Execute(TriggerEvent triggerEvent)
	{
		bool aIsTrigger = entitiesInside.HasComponent(triggerEvent.EntityA);
		if (aIsTrigger == entitiesInside.HasComponent(triggerEvent.EntityB))
			return;

		Entity trigger = aIsTrigger ? triggerEvent.EntityA : triggerEvent.EntityB;
		Entity other = aIsTrigger ? triggerEvent.EntityB : triggerEvent.EntityA;


		var entitiesInTrigger = entitiesInside[trigger];
		int iterations = entitiesInTrigger.Length;
		bool otherIsInTrigger = false;
		for (int i = 0; i < iterations; i++)
		{
			var entityDataInTrigger = entitiesInTrigger[i];
			entityDataInTrigger.state = 1;
			entitiesInTrigger[i] = entityDataInTrigger;
			otherIsInTrigger |= (entityDataInTrigger.other == other);
		}
		if (!otherIsInTrigger)
			entitiesInside[trigger].Add(new InTriggerEvent { other = other, state = 0 });
	}
}