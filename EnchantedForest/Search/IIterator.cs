namespace EnchantedForest.Search
{
    public interface IIterator<T>
    {
        bool HasNext();
        T GetNext();
        void Expand();
    }
}