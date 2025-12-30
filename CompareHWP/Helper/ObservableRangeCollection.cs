using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace CompareHWP.Helper
{
    public class ObservableRangeCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// List 추가 시 마지막에 노티파이 이벤트 발생
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(IEnumerable<T> items)
        {
            if (items == null) return;

            foreach (var item in items)
                Items.Add(item);

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
