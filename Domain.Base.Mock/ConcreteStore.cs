using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Base.Mock
{
    public class ConcreteStore<TKey, TElem>
    {
        private readonly Dictionary<TKey, LinkedList<TElem>> _localStore = new Dictionary<TKey, LinkedList<TElem>>(20);

        public TElem[] Select(TKey key, Func<TElem,bool> criteria) => _localStore.ContainsKey(key) ? _localStore[key].Where(criteria).ToArray() : null;

        public void Insert(TKey key, TElem elem)
        {
            if (!_localStore.ContainsKey(key))
            {
                _localStore[key] = new LinkedList<TElem>();
            }
            _localStore[key].AddLast(elem);
        }

        public void Insert(TElem elem, Func<TElem, TKey> KeyRetriever) => Insert(KeyRetriever(elem), elem);

        public void Delete(TElem elem, Func<TElem, TKey> KeyRetriever)
        {
            if (_localStore.ContainsKey(KeyRetriever(elem)))
            {
                _localStore[KeyRetriever(elem)].Remove(elem);
            }
        }

        public void Delete(TElem elem, Func<TElem, TKey> KeyRetriever, Func<TElem, bool> criteria)
        {
            if (_localStore.ContainsKey(KeyRetriever(elem)))
            {
                Delete(_localStore[KeyRetriever(elem)].Where(criteria).ToArray(), KeyRetriever);
            }
        }

        public void Delete(TElem[] elems, Func<TElem, TKey> KeyRetriever)
        {
            for (int i = 0; i < elems.Length; i++)
            {
                Delete(elems[i], KeyRetriever);
            }
        }

        public void Delete(IEnumerable<TElem> elems, Func<TElem, TKey> KeyRetriever)
        {
            foreach (var elem in elems)
            {
                Delete(elem, KeyRetriever);
            }
        }
    }
}
