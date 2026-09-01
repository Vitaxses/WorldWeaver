using HutongGames.PlayMaker;
using WorldWeaver.Editor;

namespace WorldWeaver.Data.Actions
{
	[ActionCategory("WorldWeaver")]
	[Tooltip("Gets a Variable from a WeaverDataHandler instance.")]
    public class GetWeaverDataVariable : FsmStateAction
    {
		[RequiredField]
		public FsmString variableName = null!;
		public FsmVar storeResult = null!;
        
		[RequiredField]
        public FsmString ModId = null!;

		public override void Reset()
		{
			variableName = null!;
			storeResult = null!;
            if (string.IsNullOrEmpty(ModId?.Value))
                ModId = WorldWeaverSettings.Instance.ModIdDefault;
		}

		public override void OnEnter()
		{
			storeResult.SetValue(null);
			Finish();
		}
    }
}