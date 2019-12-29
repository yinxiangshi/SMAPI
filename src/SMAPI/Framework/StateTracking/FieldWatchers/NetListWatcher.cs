using System;
using System.Collections.Generic;
using Netcode;

namespace StardewModdingAPI.Framework.StateTracking.FieldWatchers
{
    internal class NetListWatcher<TKey, TValue> : BaseDisposableWatcher, ICollectionWatcher<TValue>
        where TValue : class, INetObject<INetSerializable>
    {


        /*********
        ** Fields
        *********/
        /// <summary>The field being watched.</summary>
        private readonly NetList<TValue, Netcode.NetRef<TValue>> Field;

        public TKey Key { get; }

        /// <summary>The pairs added since the last reset.</summary>
        private readonly IList<TValue> AddedImpl = new List<TValue>();

        /// <summary>The pairs removed since the last reset.</summary>
        private readonly IList<TValue> RemovedImpl = new List<TValue>();

        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="field">The field to watch.</param>
        public NetListWatcher(NetList<TValue, Netcode.NetRef<TValue>> field, TKey key)
        {
            this.Field = field;
            this.Key = key;
            field.OnElementChanged += this.OnElementChanged;
            field.OnArrayReplaced += this.OnArrayReplaced;
        }

        public bool IsChanged => this.AddedImpl.Count > 0 || this.RemovedImpl.Count > 0;

        public IEnumerable<TValue> Added => this.AddedImpl;

        public IEnumerable<TValue> Removed => this.RemovedImpl;

        public void Dispose()
        {
            if (!this.IsDisposed)
            {
                this.Field.OnElementChanged -= this.OnElementChanged;
                this.Field.OnArrayReplaced -= this.OnArrayReplaced;
            }

            base.Dispose();
        }

        public void Reset()
        {
            this.AddedImpl.Clear();
            this.RemovedImpl.Clear();
        }

        public void Update()
        {
            this.AssertNotDisposed();
        }

        /*********
        ** Private methods
        *********/
        private void OnArrayReplaced(NetList<TValue, Netcode.NetRef<TValue>> list, IList<TValue> before, IList<TValue> after)
        {
            this.AddedImpl.Clear();
            this.RemovedImpl.Clear();

            foreach(var obj in after)
                this.AddedImpl.Add(obj);

            foreach(var obj in before)
                this.RemovedImpl.Add(obj);
        }

        private void OnElementChanged(NetList<TValue, Netcode.NetRef<TValue>> list, int index, TValue oldValue, TValue newValue)
        {

            /* checks for stack addition / subtraction changing stacks does not fire off an element changed event
            if ((oldValue != null && newValue != null) && oldValue.CompareTo(newValue) < 0)
                this.AddedImpl.Add(newValue);
            //Stack Removed from
            if ((oldValue != null && newValue != null) && oldValue.CompareTo(newValue) > 0)
                this.RemovedImpl.Add(newValue);
                */

            if(newValue!=null)
                this.AddedImpl.Add(newValue);

            if(oldValue!=null)
                this.RemovedImpl.Add(oldValue);
        }
    }
}
