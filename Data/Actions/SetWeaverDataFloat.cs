using HutongGames.PlayMaker;
using WorldWeaver.Editor;

namespace WorldWeaver.Data.Actions
{
	[ActionCategory("WorldWeaver")]
	[Tooltip("Sets a Float Variable from a WeaverDataHandler instance.")]
    public class SetWeaverDataFloat : FsmStateAction
    {
		[RequiredField]
		public FsmString fieldName = null!;
		public FsmFloat value = null!;
        
		[RequiredField]
        public FsmString ModId = null!;

		public override void Reset()
		{
			fieldName = null!;
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