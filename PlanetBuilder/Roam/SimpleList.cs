namespace PlanetBuilder.Roam
{
    public class SimpleList<T> where T : SimpleList<T>
    {
        public T NextNode;
        public T PrevNode;

        public SimpleList()
        {
            NextNode = (T)this;
            PrevNode = (T)this;
        }

        /// Insert this node before the given node.
        public void InsertBefore(SimpleList<T> node)
        {
            // remove from old list
            PrevNode.NextNode = NextNode;
            NextNode.PrevNode = PrevNode;

            // insert into new list
            PrevNode = node.PrevNode;
            NextNode = (T)node;
            PrevNode.NextNode = (T)this;
            node.PrevNode = (T)this;
        }

        /// Insert this node after the given node.
        public void InsertAfter(SimpleList<T> node)
        {
            // remove from old list
            PrevNode.NextNode = NextNode;
            NextNode.PrevNode = PrevNode;

            // insert into new list
            PrevNode = (T)node;
            NextNode = node.NextNode;
            NextNode.PrevNode = (T)this;
            node.NextNode = (T)this;
        }

        public void Remove()
        {
            // remove from old list
            PrevNode.NextNode = NextNode;
            NextNode.PrevNode = PrevNode;

            NextNode = (T)this;
            PrevNode = (T)this;
        }

        /// Count will not include the head node
        public int Count()
        {
            int count = 0;
            for (var node = NextNode; node != this; node = node.NextNode)
                count++;
            return count;
        }
    }
}
