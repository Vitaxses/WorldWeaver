using GenericVariableExtension;
using HutongGames.PlayMaker;
using WorldWeaver.Managers;

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
			ModId = null!;
		}

		public override void OnEnter()
		{
            if (!value.IsNone && WeaverDataManager.TryGetWorldWeaverPlugin(ModId.Value, out var plugin))
            {
                plugin.SetVariable(stringName.Value, value.Value);
            }
            
			Finish();
		}
    }
}