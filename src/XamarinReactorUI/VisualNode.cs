﻿using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace XamarinReactorUI
{
    public abstract class VisualNode
    {
        protected VisualNode()
        {
            //System.Diagnostics.Debug.WriteLine($"{this}->Created()");
        }

        public object Key { get; set; }
        public int ChildIndex { get; private set; }
        internal VisualNode Parent { get; private set; }

        private bool _invalidated = false;
        protected void Invalidate()
        {
            _invalidated = true;
             
            RequireLayoutCycle();
            //System.Diagnostics.Debug.WriteLine($"{this}->Invalidated()");
        }

        internal bool IsLayoutCycleRequired { get; set; } = true;
        private void RequireLayoutCycle()
        {
            if (IsLayoutCycleRequired)
                return;

            IsLayoutCycleRequired = true;
            Parent?.RequireLayoutCycle();
            OnLayoutCycleRequested();
        }

        internal protected virtual void OnLayoutCycleRequested()
        { 
        
        }

        //internal event EventHandler LayoutCycleRequest;

        private IReadOnlyList<VisualNode> _children = null;
        internal IReadOnlyList<VisualNode> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = new List<VisualNode>(RenderChildren().Where(_ => _ != null));
                    for (int i = 0; i < _children.Count; i++)
                    {
                        _children[i].ChildIndex = i;
                        _children[i].Parent = this;
                    }
                }
                return _children;
            }
        }

        internal virtual void MergeWith(VisualNode newNode)
        {
            if (newNode == this)
                return;

            for (int i = 0; i < Children.Count; i++)
            {
                if (newNode.Children.Count > i)
                {
                    Children[i].MergeWith(newNode.Children[i]);
                }
            }

            for (int i = newNode.Children.Count; i < Children.Count; i++)
            {
                Children[i].Unmount();
                Children[i].Parent = null;
            }

            Parent = null;
        }

        internal virtual void MergeChildrenFrom(IReadOnlyList<VisualNode> oldChildren)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                if (oldChildren.Count > i)
                {
                    oldChildren[i].MergeWith(Children[i]);
                }
            }

            for (int i = Children.Count; i < oldChildren.Count; i++)
            {
                oldChildren[i].Unmount();
                oldChildren[i].Parent = null;
            }
        }

        protected abstract IEnumerable<VisualNode> RenderChildren();

        protected bool _isMounted = false;
        protected bool _stateChanged = true;

        internal void Layout()
        {
            if (!IsLayoutCycleRequired)
                return;

            IsLayoutCycleRequired = false;

            if (_invalidated)
            {
                //System.Diagnostics.Debug.WriteLine($"{this}->Layout(Invalidated)");
                var oldChildren = Children;
                _children = null;
                MergeChildrenFrom(oldChildren);
                _invalidated = false;
            }

            if (!_isMounted && Parent != null)
                OnMount();

            if (_stateChanged)
                OnUpdate();

            foreach (var child in Children)
                child.Layout();

        }

        protected virtual void OnMount()
        {
            _isMounted = true;
        }

        internal void Unmount()
        {
            OnUnmount();
        }

        protected virtual void OnUnmount()
        {
            _isMounted = false;
        }

        protected virtual void OnUpdate()
        {
            _stateChanged = false;
        }


        internal void AddChild(VisualNode widget, Element childNativeControl)
        {
            if (widget is null)
            {
                throw new ArgumentNullException(nameof(widget));
            }

            if (childNativeControl is null)
            {
                throw new ArgumentNullException(nameof(childNativeControl));
            }

            OnAddChild(widget, childNativeControl);
        }

        protected virtual void OnAddChild(VisualNode widget, Element childNativeControl)
        {

        }

        internal void RemoveChild(VisualNode widget, Element childNativeControl)
        {
            if (widget is null)
            {
                throw new ArgumentNullException(nameof(widget));
            }

            if (childNativeControl is null)
            {
                throw new ArgumentNullException(nameof(childNativeControl));
            }

            OnRemoveChild(widget, childNativeControl);
        }

        protected virtual void OnRemoveChild(VisualNode widget, Element childNativeControl)
        {

        }

        private readonly Dictionary<string, object> _metadata = new Dictionary<string, object>();
        public void SetMetadata<T>(string key, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("can'be null or empty", nameof(key));
            }

            _metadata[key] = value;
        }

        public void SetMetadata<T>(T value)
        {
            _metadata[typeof(T).FullName] = value;
        }

        public T GetMetadata<T>(string key, T defaultValue = default)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("can'be null or empty", nameof(key));
            }

            if (_metadata.TryGetValue(key, out var value))
                return (T)value;

            return defaultValue;
        }

        public T GetMetadata<T>(T defaultValue = default) 
            => GetMetadata(typeof(T).FullName, defaultValue);


    }

    public static class VisualNodeExtensions
    {
        public static T WithMetadata<T>(this T node, string key, object value) where T : VisualNode
        {
            node.SetMetadata(key, value);
            return node;
        }

        public static T WithMetadata<T>(this T node, object value) where T : VisualNode
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            node.SetMetadata(value.GetType().FullName, value);
            return node;
        }

        public static T WithKey<T>(this T node, object key) where T : VisualNode
        {
            node.Key = key;
            return node;
        }

    }
}
