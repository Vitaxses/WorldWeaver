using HutongGames.PlayMaker;
using WorldWeaver.Editor;

namespace WorldWeaver.Data.Actions
{
	[ActionCategory("WorldWeaver")]
	[Tooltip("Sets a String Variable from a WeaverDataHandler instance.")]
    public class SetWeaverDataString : FsmStateAction
    {
		[RequiredField]
		public FsmString stringName = null!;
		public FsmString value = null!;
        
		[RequiredField]
        public FsmString ModId = null!;

		public override void Reset()
		{
			stringName = null!;
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