using GenericVariableExtension;
using HutongGames.PlayMaker;
using WorldWeaver.Managers;

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
			ModId = null!;
		}

		public override void OnEnter()
		{
            if (!value.IsNone && WeaverDataManager.TryGetWorldWeaverPlugin(ModId.Value, out var plugin))
            {
                plugin.SetVariable(fieldName.Value, value.Value);
            }
            
			Finish();
		}
    }
}