using System;
using System.Collections;
using System.Collections.Generic;

namespace SimpleCircuit.Components.Variants
{
    /// <summary>
    /// Describes a set of variants.
    /// </summary>
    public class VariantSet : IEnumerable<string>
    {
        private readonly HashSet<string> _set = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _checked = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Event that is called when a variant has been added.
        /// </summary>
        public event EventHandler Changed;

        /// <summary>
        /// Gets the number of variants and branching variants.
        /// </summary>
        public int BranchCount
        {
            get
            {
                int count = _set.Count;
                foreach (string n in _checked)
                {
                    if (!_set.Contains(n))
                        count++;
                }
                return count;
            }
        }

        /// <summary>
        /// Gets the number of items in the variant set.
        /// </summary>
        public int Count => _set.Count;

        /// <summary>
        /// Gets all the potential variants that can be used from this point on.
        /// </summary>
        public IEnumerable<string> Branches
        {
            get
            {
                foreach (string n in _checked)
                {
                    if (!_set.Contains(n))
                        yield return n;
                }
            }
        }

        /// <summary>
        /// Clears the set of any variants.
        /// </summary>
        public void Clear()
        {
            _checked.Clear();
            _set.Clear();
            Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Clears the checked variants.
        /// </summary>
        public void Reset() => _checked.Clear();

        /// <inheritdoc />
        public bool Add(string variant)
        {
            if (_set.Add(variant))
            {
                Changed?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public bool Remove(string variant)
        {
            if (_set.Remove(variant))
            {
                Changed?.Invoke(this, EventArgs.Empty);
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public bool Contains(string variant)
        {
            if (_set.Contains(variant))
                return true;
            else
            {
                _checked.Add(variant);
                return false;
            }
        }

        /// <summary>
        /// Find the index of the specified variant that is selected. Allows using variants inside a switch statement.
        /// </summary>
        /// <param name="variantNames">The list of variants that need to be checked.</param>
        /// <returns>Returns the index of the variant, -1 if no variants were selected, and -2 if multiple variants were selected.</returns>
        public int Select(params string[] variantNames)
        {
            int index = -1;
            for (int i = 0; i < variantNames.Length; i++)
            {
                if (_set.Contains(variantNames[i]))
                {
                    if (index == -1)
                        index = i;
                    else
                        index = -2;
                }
            }
            if (index == -1)
            {
                foreach (string n in variantNames)
                    _checked.Add(n);
            }
            return index;
        }

        /// <inheritdoc />
        public virtual IEnumerator<string> GetEnumerator() => _set.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
