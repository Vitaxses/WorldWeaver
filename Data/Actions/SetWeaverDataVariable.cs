using GenericVariableExtension;
using HutongGames.PlayMaker;
using WorldWeaver.Managers;

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
			ModId = null!;
		}

		public override void OnEnter()
		{
            if (!value.IsNone && WeaverDataManager.TryGetWorldWeaverPlugin(ModId.Value, out var plugin))
            {
                plugin.SetVariable(variableName.Value, value.GetValue());
            }
            
			Finish();
		}
    }
}