﻿
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace XamarinReactorUI
{
    public interface IRxContentPage
    {
    }

    public sealed class RxContentPage : RxPage<Xamarin.Forms.ContentPage>, IRxContentPage, IEnumerable<VisualNode>
    {
        private List<VisualNode> _contents = new List<VisualNode>();
        private readonly ContentPage _contentPage;

        public RxContentPage()
        {

        }

        public RxContentPage(VisualNode content)
        {
            _contents.Add(content ?? throw new ArgumentNullException());
        }

        public RxContentPage(ContentPage contentPage)
        {
            _contentPage = contentPage;
        }

        public RxContentPage(Action<ContentPage> componentRefAction)
            : base(componentRefAction)
        {
        }

        protected override void OnMount()
        {
            _nativeControl = _contentPage;
            base.OnMount();
        }

        public void Add(VisualNode child)
        {
            _contents.Add(child ?? throw new ArgumentNullException());
        }

        public IEnumerator<VisualNode> GetEnumerator()
        {
            return _contents.GetEnumerator();
        }

        protected override void OnAddChild(VisualNode widget, Element childControl)
        {
            if (childControl is View view)
                NativeControl.Content = view;
            else if (childControl is ToolbarItem toolbarItem)
                NativeControl.ToolbarItems.Add(toolbarItem);
            else
            {
                throw new InvalidOperationException($"Type '{childControl.GetType()}' not supported under '{GetType()}'");
            }

            base.OnAddChild(widget, childControl);
        }

        protected override void OnRemoveChild(VisualNode widget, Element childControl)
        {
            if (childControl is View)
                NativeControl.Content = null;
            else if (childControl is ToolbarItem toolbarItem)
                NativeControl.ToolbarItems.Remove(toolbarItem);

            base.OnRemoveChild(widget, childControl);
        }

        protected override IEnumerable<VisualNode> RenderChildren()
        {
            return _contents;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public static class RxContentPageExtensions
    {

    }

}