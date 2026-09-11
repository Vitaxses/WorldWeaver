using HutongGames.PlayMaker;
using WorldWeaver.Data.MonoBehaviours;

namespace WorldWeaver.Data.Actions
{
	[ActionCategory("WorldWeaver")]
	public class WeaverSetShopItemPurchased : FsmStateAction
	{
		public FsmOwnerDefault Target = null!;

		public FsmInt SubItemIndex = null!;

		[UIHint(UIHint.Variable)]
		public FsmBool IsWaitingBool = null!;

		public override void Reset()
		{
			Target = null!;
			SubItemIndex = null!;
			IsWaitingBool = null!;
		}

		void OnComplete()
		{
			IsWaitingBool.Value = false;
			GameCameras.instance.HUDIn();
		}

		public override void OnEnter()
		{
			IsWaitingBool.Value = false;
			GameObject safe = Target.GetSafe(this);
			
			if (safe == null)
			{
				Finish();
				return;
			}

			if (safe.TryGetComponent<WeaverShopItemStats>(out var weaverStats)) 
			{
				IsWaitingBool.Value = true;
				weaverStats.SetPurchased(OnComplete, SubItemIndex.Value);
			} 
			
			else if (safe.TryGetComponent<ShopItemStats>(out var stats))
			{
				IsWaitingBool.Value = true;
				stats.SetPurchased(OnComplete, SubItemIndex.Value);
			}

			Finish();
		}
	}
}
