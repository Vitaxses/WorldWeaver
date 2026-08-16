using HutongGames.PlayMaker;

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
			ModId = null!;
		}

		public override void OnEnter()
		{
			storeResult.Value = string.Empty;
			Finish();
		}
    }
}