using HutongGames.PlayMaker;
using WorldWeaver.Editor;

namespace WorldWeaver.Data.Actions
{
	[ActionCategory("WorldWeaver")]
	[Tooltip("Gets a Float Variable from a WeaverDataHandler instance.")]
    public class GetWeaverDataFloat : FsmStateAction
    {
		[RequiredField]
		public FsmString fieldName = null!;
		public FsmFloat storeResult = null!;
        
		[RequiredField]
        public FsmString ModId = null!;

		public override void Reset()
		{
			fieldName = null!;
			storeResult = null!;
            if (string.IsNullOrEmpty(ModId?.Value))
                ModId = WorldWeaverSettings.Instance.ModIdDefault;
		}

		public override void OnEnter()
		{
			storeResult.Value = 0f;
			Finish();
		}
    }
}