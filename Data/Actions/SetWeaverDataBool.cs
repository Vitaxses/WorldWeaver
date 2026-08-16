using GenericVariableExtension;
using HutongGames.PlayMaker;
using WorldWeaver.Managers;

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
			ModId = null!;
		}

		public override void OnEnter()
		{
            if (!value.IsNone && WeaverDataManager.TryGetWorldWeaverPlugin(ModId.Value, out var plugin))
            {
                plugin.SetVariable(boolName.Value, value.Value);
            }
            
			Finish();
		}
    }
}