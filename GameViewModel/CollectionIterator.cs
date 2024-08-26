namespace KFC.ViewModel {
    public class CollectionIterator
    {
        IList<int> collection;
        private int index;

        public CollectionIterator(IList<int> collection)
        {
            this.collection = collection;
            index = 0;
        }
        public bool GetNextItem(out int item)
        {
            item = -1;
            if(index >= collection.Count)
            {
                return false;
            }
            item = collection[index++];
            return true;
        }
    }
}
