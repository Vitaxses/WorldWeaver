using HutongGames.PlayMaker;
using WorldWeaver.Editor;

namespace WorldWeaver.Data.Actions
{
	[ActionCategory("WorldWeaver")]
	[Tooltip("Gets a Boolean Variable from a WeaverDataHandler instance.")]
    public class GetWeaverDataBool : FsmStateAction
    {
		[RequiredField]
		public FsmString boolName = null!;
		public FsmBool storeResult = null!;
        
		[RequiredField]
        public FsmString ModId = null!;

		public override void Reset()
		{
			boolName = null!;
			storeResult = null!;
            if (string.IsNullOrEmpty(ModId?.Value))
                ModId = WorldWeaverSettings.Instance.ModIdDefault;
		}

		public override void OnEnter()
		{
			storeResult.Value = true;
			Finish();
		}
    }
}