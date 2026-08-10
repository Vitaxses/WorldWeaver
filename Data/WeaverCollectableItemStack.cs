namespace WorldWeaver.Data
{
    [CreateAssetMenu(menuName = "WorldWeaver/Collectable Items/Collectable Item (Stack)")]
    public class WeaverCollectableItemStack : CollectableItemStack
    {
        [SerializeField]
        private Sprite tinyIcon;

        public override Sprite GetIcon(ReadSource readSource)
        {
            if (readSource == ReadSource.Tiny && tinyIcon != null)
                return tinyIcon;

            return base.GetIcon(readSource);
        }
    }
}