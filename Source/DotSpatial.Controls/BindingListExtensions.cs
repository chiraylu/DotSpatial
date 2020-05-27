using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotSpatial.Controls
{
    public static class BindingListExtensions
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
    }
}
