using GenericVariableExtension;
using HutongGames.PlayMaker;
using WorldWeaver.Managers;

namespace WorldWeaver.Data.Actions
{
	[ActionCategory("WorldWeaver")]
	[Tooltip("Gets a Boolean Variable from a WeaverDataHandler instance.")]
    public class GetWeaverDataBool : FsmStateAction
    {
		[RequiredField]
		public FsmString boolName = null!;
		public FsmBool storeResult = null!;
        
		[RequiredField]
        public FsmString ModId = null!;

		public override void Reset()
		{
			boolName = null!;
			storeResult = null!;
			ModId = null!;
		}

		public override void OnEnter()
		{
            if (!storeResult.IsNone && WeaverDataManager.TryGetWorldWeaverPlugin(ModId.Value, out var plugin))
            {
                storeResult.Value = plugin.GetVariable<bool>(boolName.Value);
            }
            
			Finish();
		}
    }
}