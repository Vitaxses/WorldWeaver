using HutongGames.PlayMaker;
using WorldWeaver.Editor;

namespace WorldWeaver.Data.Actions
{
	[ActionCategory("WorldWeaver")]
	[Tooltip("Gets an Int Variable from a WeaverDataHandler instance.")]
    public class GetWeaverDataInt : FsmStateAction
    {
		[RequiredField]
		public FsmString fieldName = null!;
		public FsmInt storeResult = null!;

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
			storeResult.Value = 0;
			Finish();
		}
    }
}