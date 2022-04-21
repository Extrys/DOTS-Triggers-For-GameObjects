using Unity.Entities;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup)), UpdateAfter(typeof(StepPhysicsWorld)), UpdateBefore(typeof(EndFramePhysicsSystem))]
public class CustomTriggerEventGroup : ComponentSystemGroup { }

[UpdateInGroup(typeof(CustomTriggerEventGroup)), UpdateAfter(typeof(InTriggerEventProcessSystem)), UpdateBefore(typeof(TriggerEventsFinalizerSystem))]
public class TriggerProcessing : ComponentSystemGroup { }
