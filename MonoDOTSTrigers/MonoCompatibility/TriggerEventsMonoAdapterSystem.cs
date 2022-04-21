using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using static Unity.Mathematics.math;

[UpdateInGroup(typeof(TriggerProcessing)), UpdateBefore(typeof(TriggerEventsMonoAdapterSystem))]
public class TriggerEventsSorterSystem : SystemBase
{
	protected override void OnUpdate()
	{
		Entities.ForEach((ref DynamicBuffer<InTriggerEvent> evs, ref InTriggerEventGroups tg) =>
		{
			var e = evs.AsNativeArray();
			e.Sort();
			tg.stateCounts = 0;
			for (int i = 0; i < e.Length; i++)
			{
				evs[i] = e[i];
				tg.stateCounts[min(e[i].state, 3)]++;
			}
		}).WithBurst().ScheduleParallel();
	}

	//private static void Quick_Sort(DynamicBuffer<InTriggerEvent> arr, int left, int right)
	//{
	//      if (arr.Length == 0)
	//            return;

	//      if (left < right)
	//      {
	//            int pivot = arr[left].state;
	//            while (true)
	//            {
	//                  while (arr[left].state < pivot)
	//                        left++;

	//                  while (arr[right].state > pivot)
	//                        right--;

	//                  if (left < right)
	//                  {
	//                        if (arr[left].state == arr[right].state)
	//                              pivot = right;

	//                        var temp = arr[left];
	//                        arr[left] = arr[right];
	//                        arr[right] = temp;
	//                  }
	//                  else
	//                  {
	//                        pivot = right;
	//                        break;
	//                  }
	//            }

	//            if (pivot > 1)
	//                  Quick_Sort(arr, left, pivot - 1);
	//            if (pivot + 1 < right)
	//                  Quick_Sort(arr, pivot + 1, right);
	//      }
	//}
}






[UpdateInGroup(typeof(TriggerProcessing))]
public class TriggerEventsMonoAdapterSystem : SystemBase
{
	protected override void OnUpdate()
	{
		Entities/*.WithAll<ShouldUpdateTriggerEventsTag>()*/.ForEach((EntityTriggerEvents triggerMb, in DynamicBuffer<InTriggerEvent> evs) =>
		{
			int iterations = evs.Length;
			for (int i = 0; i < iterations; i++)
			{
				InTriggerEvent ev = evs[i];

				if (ev.state == 0)
					triggerMb.OnEntityTriggerEnter(ev.other).Forget();
				else if (ev.state == 2)
					triggerMb.OnEntityTriggerExit(ev.other).Forget();
			}

		}).WithoutBurst().Run();
	}
}

public struct ShouldUpdateTriggerEventsTag : IComponentData { }

