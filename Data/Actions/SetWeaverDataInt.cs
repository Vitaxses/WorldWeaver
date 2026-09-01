using HutongGames.PlayMaker;
using WorldWeaver.Editor;

namespace WorldWeaver.Data.Actions
{
	[ActionCategory("WorldWeaver")]
	[Tooltip("Sets an Int Variable from a WeaverDataHandler instance.")]
    public class SetWeaverDataInt : FsmStateAction
    {
		[RequiredField]
		public FsmString fieldName = null!;
		public FsmInt value = null!;

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