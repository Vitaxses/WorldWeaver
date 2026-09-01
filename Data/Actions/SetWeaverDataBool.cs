using HutongGames.PlayMaker;
using WorldWeaver.Editor;

namespace WorldWeaver.Data.Actions
{
	[ActionCategory("WorldWeaver")]
	[Tooltip("Sets a Boolean Variable from a WeaverDataHandler instance.")]
    public class SetWeaverDataBool : FsmStateAction
    {
		[RequiredField]
		public FsmString boolName = null!;
		public FsmBool value = null!;
        
		[RequiredField]
        public FsmString ModId = null!;

		public override void Reset()
		{
			boolName = null!;
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