using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotSpatial.Controls
{
    public static class CollectionExtensions
    {
        public static void InsertRange<T>(this BindingList<T> bindingList, int index, IEnumerable<T> array)
        {
            if (bindingList == null || array == null)
            {
                return;
            }
            for (int i = array.Count() - 1; i >= 0; i--)
            {
                bindingList.Insert(index, array.ElementAt(i));
            }
        }
        public static void AddRange<T>(this BindingList<T> bindingList, IEnumerable<T> array)
        {
            if (bindingList == null || array == null)
            {
                return;
            }
            foreach (var item in array)
            {
                bindingList.Add(item);
            }
        }

        public static void InsertRange<T>(this ObservableCollection<T> collection, int index, IEnumerable<T> array)
        {
            if (collection == null || array == null)
            {
                return;
            }
            for (int i = array.Count() - 1; i >= 0; i--)
            {
                collection.Insert(index, array.ElementAt(i));
            }
        }
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> array)
        {
            if (collection == null || array == null)
            {
                return;
            }
            foreach (var item in array)
            {
                collection.Add(item);
            }
        }
    }
}
