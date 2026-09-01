using HutongGames.PlayMaker;
using WorldWeaver.Editor;

namespace WorldWeaver.Data.Actions
{
	[ActionCategory("WorldWeaver")]
	[Tooltip("Sets a Variable from a WeaverDataHandler instance.")]
    public class SetWeaverDataVariable : FsmStateAction
    {
		[RequiredField]
		public FsmString variableName = null!;
		public FsmVar value = null!;
        
		[RequiredField]
        public FsmString ModId = null!;

		public override void Reset()
		{
			variableName = null!;
			value = null!;
            if (string.IsNullOrEmpty(ModId?.Value))
                ModId = WorldWeaverSettings.Instance.ModIdDefault;
		}

		public override void OnEnter()
		{
			Finish();
		}
    }
}