using System.Collections;

namespace Controller.Helpers
{
    public class SlotList<T> : IEnumerable<T>
    {
        private readonly List<T?> _list; // Internal list to store elements
        private readonly int _capacity; // Fixed size for the slots

        public SlotList(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentException("Capacity must be greater than zero.");
            }

            _capacity = capacity;
            _list = new List<T?>(new T?[_capacity]); // Initialize with default values
        }

        /// <summary>
        /// Adds an item to a specific index.
        /// </summary>
        public void AddAt(int index, T item)
        {
            if (index < 0 || index >= _capacity)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of bounds.");
            }

            if (_list[index] is not null)
            {
                throw new InvalidOperationException($"Slot at index {index} is already filled.");
            }

            _list[index] = item;
        }

        /// <summary>
        /// Checks if all slots have been filled.
        /// </summary>
        public bool AreAllSlotsFilled()
        {
            foreach (var item in _list)
            {
                if (item is null)
                {
                    return false; // At least one slot is empty
                }
            }
            return true;
        }

        /// <summary>
        /// Custom iterator to return only non-null items.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _list)
            {
                if (item is not null) // Skip null slots
                {
                    yield return item!;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return GetEnumerator();
        }
    }
}
