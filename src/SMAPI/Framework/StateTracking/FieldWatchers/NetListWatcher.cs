using System.Collections.Generic;
using Netcode;

namespace StardewModdingAPI.Framework.StateTracking.FieldWatchers
{
    /// <summary>A watcher which detects changes to a net list field.</summary>
    /// <typeparam name="TValue">The list value type.</typeparam>
    internal class NetListWatcher<TValue> : BaseDisposableWatcher, ICollectionWatcher<TValue>
        where TValue : class, INetObject<INetSerializable>
    {
        /*********
        ** Fields
        *********/
        /// <summary>The field being watched.</summary>
        private readonly NetList<TValue, NetRef<TValue>> Field;

        /// <summary>The pairs added since the last reset.</summary>
        private readonly IList<TValue> AddedImpl = new List<TValue>();

        /// <summary>The pairs removed since the last reset.</summary>
        private readonly IList<TValue> RemovedImpl = new List<TValue>();


        /*********
        ** Accessors
        *********/
        /// <summary>Whether the collection changed since the last reset.</summary>
        public bool IsChanged => this.AddedImpl.Count > 0 || this.RemovedImpl.Count > 0;

        /// <summary>The values added since the last reset.</summary>
        public IEnumerable<TValue> Added => this.AddedImpl;

        /// <summary>The values removed since the last reset.</summary>
        public IEnumerable<TValue> Removed => this.RemovedImpl;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="field">The field to watch.</param>
        public NetListWatcher(NetList<TValue, NetRef<TValue>> field)
        {
            this.Field = field;
            field.OnElementChanged += this.OnElementChanged;
            field.OnArrayReplaced += this.OnArrayReplaced;
        }

        /// <summary>Set the current value as the baseline.</summary>
        public void Reset()
        {
            this.AddedImpl.Clear();
            this.RemovedImpl.Clear();
        }

        /// <summary>Update the current value if needed.</summary>
        public void Update()
        {
            this.AssertNotDisposed();
        }

        /// <summary>Stop watching the field and release all references.</summary>
        public override void Dispose()
        {
            if (!this.IsDisposed)
            {
                this.Field.OnElementChanged -= this.OnElementChanged;
                this.Field.OnArrayReplaced -= this.OnArrayReplaced;
            }

            base.Dispose();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>A callback invoked when the value list is replaced.</summary>
        /// <param name="list">The net field whose values changed.</param>
        /// <param name="oldValues">The previous list of values.</param>
        /// <param name="newValues">The new list of values.</param>
        private void OnArrayReplaced(NetList<TValue, NetRef<TValue>> list, IList<TValue> oldValues, IList<TValue> newValues)
        {
            this.AddedImpl.Clear();
            this.RemovedImpl.Clear();

            foreach (TValue value in newValues)
                this.AddedImpl.Add(value);

            foreach (TValue value in oldValues)
                this.RemovedImpl.Add(value);
        }

        /// <summary>A callback invoked when an entry is replaced.</summary>
        /// <param name="list">The net field whose values changed.</param>
        /// <param name="index">The list index which changed.</param>
        /// <param name="oldValue">The previous value.</param>
        /// <param name="newValue">The new value.</param>
        private void OnElementChanged(NetList<TValue, NetRef<TValue>> list, int index, TValue oldValue, TValue newValue)
        {
            if (newValue != null)
                this.AddedImpl.Add(newValue);

            if (oldValue != null)
                this.RemovedImpl.Add(oldValue);
        }
    }
}
