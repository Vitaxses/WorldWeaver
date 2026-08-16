using GenericVariableExtension;
using HutongGames.PlayMaker;
using WorldWeaver.Managers;

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