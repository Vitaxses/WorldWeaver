using GenericVariableExtension;
using HutongGames.PlayMaker;
using WorldWeaver.Managers;

namespace WorldWeaver.Data.Actions
{
	[ActionCategory("WorldWeaver")]
	[Tooltip("Gets a Float Variable from a WeaverDataHandler instance.")]
    public class GetWeaverDataFloat : FsmStateAction
    {
		[RequiredField]
		public FsmString fieldName = null!;
		public FsmFloat storeResult = null!;
        
		[RequiredField]
        public FsmString ModId = null!;

		public override void Reset()
		{
			fieldName = null!;
			storeResult = null!;
			ModId = null!;
		}

		public override void OnEnter()
		{
            if (WeaverDataManager.TryGetWorldWeaverPlugin(ModId.Value, out var plugin))
            {
                storeResult.Value = plugin.GetVariable<float>(fieldName.Value);
            }
            
			Finish();
		}
    }
}