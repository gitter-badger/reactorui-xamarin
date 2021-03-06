﻿using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace XamarinReactorUI
{
    internal class RxPageHost<T> : VisualNode, IRxHostElement where T : RxComponent, new()
    {
        private RxComponent _component;
        private bool _sleeping;
        private Page _componentPage;
        private readonly Action<RxComponent> _componentInitializer;

        private RxPageHost(Action<RxComponent> componentInitializer)
        {
            _componentInitializer = componentInitializer;
        }

        public static Page CreatePage(Action<RxComponent> componentInitializer = null)
        {
            var host = new RxPageHost<T>(componentInitializer);
            host.Run();
            return host._componentPage;
        }

        protected sealed override void OnAddChild(VisualNode widget, Element nativeControl)
        {
            if (nativeControl is Page page)
            {
                _componentPage = page;
                _componentPage.Appearing += OnComponentPage_Appearing;
                _componentPage.Disappearing += OnComponentPage_Disappearing;
            }
            else
            {
                throw new NotSupportedException($"Invalid root component ({nativeControl.GetType()}): must be a page (i.e. RxContentPage, RxShell etc)");
            }
        }

        private void OnComponentPage_Appearing(object sender, EventArgs e)
        {
            _sleeping = false;
            OnLayoutCycleRequested();
        }

        private void OnComponentPage_Disappearing(object sender, EventArgs e)
        {
            _sleeping = true;
        }

        protected sealed override void OnRemoveChild(VisualNode widget, Element nativeControl)
        {
            if (_componentPage != null)
            {
                _componentPage.Appearing += OnComponentPage_Appearing;
                _componentPage.Disappearing += OnComponentPage_Disappearing;
            }

            _componentPage = null;
        }

        public void Run()
        {
            _component = _component ?? RxApplication.Instance.ComponentLoader.LoadComponent<T>();
            if (_component != null)
            {
                _componentInitializer?.Invoke((T)_component);
            }

            RxApplication.Instance.ComponentLoader.ComponentAssemblyChanged += OnComponentAssemblyChanged;

            OnLayout();

            if (_componentPage == null)
            {
                throw new InvalidOperationException($"Component {_component.GetType()} doesn't render a page as root");
            }
        }

        private void OnComponentAssemblyChanged(object sender, EventArgs e)
        {
            try
            {
                var newComponent = RxApplication.Instance.ComponentLoader.LoadComponent<T>();
                if (newComponent != null)
                {
                    _component = newComponent;
                    _componentInitializer?.Invoke(_component);

                    Invalidate();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Unable to hot relead component {typeof(T).FullName}: type not found in received assembly");
                }
            }
            catch (Exception ex)
            {
                RxApplication.Instance.FireUnhandledExpectionEvent(ex);
            }
        }

        public void Stop()
        {
            RxApplication.Instance.ComponentLoader.ComponentAssemblyChanged -= OnComponentAssemblyChanged;
            _sleeping = true;
        }

        protected internal override void OnLayoutCycleRequested()
        {
            if (!_sleeping)
            {
                Device.BeginInvokeOnMainThread(OnLayout);
            }

            base.OnLayoutCycleRequested();
        }

        //public event EventHandler<UnhandledExceptionEventArgs> UnhandledException;

        private void OnLayout()
        {
            try
            {
                Layout();
            }
            catch (Exception ex)
            {
                RxApplication.Instance?.FireUnhandledExpectionEvent(ex);
                //UnhandledException?.Invoke(this, new UnhandledExceptionEventArgs(ex, false));
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        protected override IEnumerable<VisualNode> RenderChildren()
        {
            yield return _component;
        }
    }
}
