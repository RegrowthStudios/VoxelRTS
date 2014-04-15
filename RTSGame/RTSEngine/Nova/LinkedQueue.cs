using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections.Generic
{
    public class LinkedQueue<T> : IEnumerable<T>
    {
        protected Node first, last;
        public Node First { get { return first.Next; } }
        public Node Last { get { return last; } }
        private int count;
        public int Count { get { return count; } }

        public LinkedQueue()
        {
            count = 0;
            last = new Node(default(T));
            first = last;
        }
        public LinkedQueue(IEnumerable<T> o)
            : this()
        {
            foreach (T data in o)
            {
                enqueue(data);
            }
        }
        public LinkedQueue(params T[] o)
            : this()
        {
            if (o != null || o.Length > 0)
            {
                foreach (T data in o)
                {
                    enqueue(data);
                }
            }
        }

        public void enqueue(LinkedQueue<T> queue)
        {
            last.Next = queue.first.Next;
            count += queue.count;
        }
        public void enqueue(Node node)
        {
            last.Next = node;
            last = node;
            count++;
        }
        public void enqueue(T data)
        {
            last = new Node(data, last);
            count++;
        }
        public T dequeue()
        {
            if (count == 0)
            {
                return default(T);
            }
            else
            {
                count--;
                first = first.Next;
                return first.Data;
            }
        }

        public void clear()
        {
            count = 0;
            last = new Node(default(T));
            first = last;
        }

        #region Enumeration
        public IEnumerator<T> GetEnumerator()
        {
            Node n = first;
            while (n != null)
            {
                yield return n.Data;
                n = n.Next;
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            Node n = first;
            while (n != null)
            {
                yield return n.Data;
                n = n.Next;
            }
        }
        #endregion

        public class Node
        {
            public Node Next;
            public T Data;

            public Node(T o)
            {
                Data = o;
            }
            public Node(T o, Node parent)
                : this(o)
            {
                Data = o;
                if (parent != null)
                {
                    parent.Next = this;
                }
            }
        }
    }
}
