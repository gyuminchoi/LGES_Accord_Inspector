using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Pattern
{
    public class AutoDeleteObservableCollection<T> : ObservableCollection<T>
    {

        /// <summary>
        /// Default 20
        /// </summary>
        public int MaxIndex { get; set; } = 20;
        public AutoDeleteObservableCollection() { }
        public AutoDeleteObservableCollection(int maxIndex) => MaxIndex = maxIndex;

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            if (Count > MaxIndex) RemoveAt(0);
        }
    }
}
