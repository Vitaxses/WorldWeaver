using HutongGames.PlayMaker;
using WorldWeaver.Editor;

namespace WorldWeaver.Data.Actions
{
	[ActionCategory("WorldWeaver")]
	[Tooltip("Gets a String Variable from a WeaverDataHandler instance.")]
    public class GetWeaverDataString : FsmStateAction
    {
		[RequiredField]
		public FsmString stringName = null!;
		public FsmString storeResult = null!;
        
		[RequiredField]
        public FsmString ModId = null!;

		public override void Reset()
		{
			stringName = null!;
			storeResult = null!;
            if (string.IsNullOrEmpty(ModId?.Value))
                ModId = WorldWeaverSettings.Instance.ModIdDefault;
		}

		public override void OnEnter()
		{
			storeResult.Value = string.Empty;
			Finish();
		}
    }
}