using HutongGames.PlayMaker;

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
			ModId = null!;
		}

		public override void OnEnter()
		{
			storeResult.SetValue(null);
			Finish();
		}
    }
}