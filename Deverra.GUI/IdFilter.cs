using System;
using System.Collections.Generic;
using System.Windows;

namespace Deverra.GUI
{
    internal class IdFilter : IEquatable<IdFilter>
    {
        private readonly Guid _id;
        public VM.Filters Filter { get; }
        public Visibility Visibility { get; }
        public int Ratio { get; set; }
        public string Tip { get; }

        private static readonly Dictionary<VM.Filters, string> SpecialFilters = new Dictionary<VM.Filters, string>
        {
            [VM.Filters.Contrast] = "From -100 to 100",
            [VM.Filters.Saturation] = "From -100 to 100",
            [VM.Filters.Hue] = "In degrees (0-360)",
            [VM.Filters.Log2] = "From 0 to 100",
            [VM.Filters.Wave] = "From 0 to 100",
            [VM.Filters.Shine] = "From 0 to 100"
        };


        public IdFilter(VM.Filters filter)
        {
            _id = Guid.NewGuid();
            Filter = filter;
            Visibility = SpecialFilters.TryGetValue(filter, out var result) ? Visibility.Visible : Visibility.Collapsed;
            Tip = result;
            Ratio = 0;
        }

        public static implicit operator VM.Filters(IdFilter idFilter)
        {
            return idFilter.Filter;
        }

        public override string ToString()
        {
            return Filter.ToString();
        }

        public bool Equals(IdFilter other)
        {
            return _id.Equals(other?._id);
        }

        public override bool Equals(object obj)
        {
            return obj is IdFilter other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }
    }
}
