using HutongGames.PlayMaker;
using WorldWeaver.Editor;

namespace WorldWeaver.Data.Actions
{
	[ActionCategory("WorldWeaver")]
	[Tooltip("Sends Events based on the value of a Boolean Variable.")]
    public class WeaverDataBoolTest : FsmStateAction
    {
		[RequiredField]
		public FsmString boolName = null!;

		[Tooltip("Event to send if the Bool variable is True.")]
		public FsmEvent isTrue = null!;

		[Tooltip("Event to send if the Bool variable is False.")]
		public FsmEvent isFalse = null!;
        
		[RequiredField]
        public FsmString ModId = null!;

		public override void Reset()
		{
			boolName = null!;
			isTrue = null!;
			isFalse = null!;
            if (string.IsNullOrEmpty(ModId?.Value))
                ModId = WorldWeaverSettings.Instance.ModIdDefault;
		}

		public override void OnEnter()
		{
			Fsm.Event(isTrue);
			Finish();
		}
    }
}