namespace Collectibles
{
    public interface IBlockableCollectible
    {
        bool ShouldBlockDrop(CollectibleManager manager);
    }
}