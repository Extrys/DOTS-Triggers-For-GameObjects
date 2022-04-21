using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;

[UpdateInGroup(typeof(CustomTriggerEventGroup)), UpdateAfter(typeof(InTriggerEventProcessSystem))]
public class TriggerEventsFinalizerSystem : SystemBase
{
	protected override void OnUpdate()
	{
		Entities.ForEach((ref DynamicBuffer<InTriggerEvent> evs, in InTriggerEventGroups eg) =>
		{
			int iterations = evs.Length;

			int deletes = eg.stateCounts.w; // the last element of the int4 is the deletes count

			if (deletes > 0)
			{
				iterations -= deletes;
				evs.RemoveRange(iterations, deletes);
			}

			for (int i = 0; i < iterations; i++)
			{
				InTriggerEvent ev = evs[i];
				++ev.state;
				//ev.state = math.min(ev.state, 3);
				evs[i] = ev;
			}

		}).Schedule();
	}
}


