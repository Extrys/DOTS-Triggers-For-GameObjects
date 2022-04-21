using HybridEZS;

public class SyncEntityToTransformAuthoring : ComponentDataAuthor<SyncEntityToTransformTag>
{
	public override SyncEntityToTransformTag GetAuthoredComponentData() => new SyncEntityToTransformTag();
}
