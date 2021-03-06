﻿//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using Xamarin.Forms;

//namespace XamarinReactorUI
//{
//    public class RxHotReloadHostElement : VisualNode, IRxHostElement
//    {
//        private readonly HotReloadServer _server;
//        private RxComponent _rootComponent;
//        //private bool _pendingStop = false;
//        private byte[] _latestAssemblyRaw;
//        private byte[] _latestAssemblySymbolStoreRaw;

//        //private VisualTree _visualTree;

//        public RxHotReloadHostElement(RxComponent rootComponent)
//            : this(rootComponent, 45820)
//        {
//        }

//        public RxHotReloadHostElement(RxComponent rootComponent, int serverPort, params object[] args)
//        {
//            _rootComponent = rootComponent ?? throw new ArgumentNullException(nameof(rootComponent));
//            _server = new HotReloadServer(serverPort);
//            _server.ReceivedAssembly += ReceivedAssemblyFromHost;
//            //_visualTree = new VisualTree(this);
//        }

//        public event EventHandler<UnhandledExceptionEventArgs> UnhandledException;

//        protected sealed override void OnAddChild(VisualNode widget, Xamarin.Forms.Element nativeControl)
//        {
//        }

//        protected sealed override void OnRemoveChild(VisualNode widget, Xamarin.Forms.Element nativeControl)
//        {
//        }

//        public void Run()
//        {
//            //_pendingStop = false;
//            //Device.BeginInvokeOnMainThread(OnLayout);
//            OnLayoutCycleRequested();

//            //_visualTree.LayoutCycleRequest += VisualTree_LayoutCycleRequest;
//            _server.Run();
//        }

//        //private void VisualTree_LayoutCycleRequest(object sender, EventArgs e)
//        //{
//        //    Device.BeginInvokeOnMainThread(OnLayout);
//        //}

//        protected internal override void OnLayoutCycleRequested()
//        {
//            Device.BeginInvokeOnMainThread(OnLayout); 
//            base.OnLayoutCycleRequested();
//        }

//        private void ReceivedAssemblyFromHost(object sender, ReceivedAssemblyEventArgs e)
//        {
//            lock (this)
//            {
//                _latestAssemblyRaw = e.AssemblyRaw;
//                _latestAssemblySymbolStoreRaw = e.AssemblySymbolStoreRaw;
//            }
//            Device.BeginInvokeOnMainThread(OnLayout);
//        }

//        public void Stop()
//        {
//            //_visualTree.LayoutCycleRequest -= VisualTree_LayoutCycleRequest;

//            _server.Stop();
//            //_pendingStop = true;
//        }

//        private void OnLayout()
//        {
//            //if (_pendingStop)
//            //    return;

//            if (_latestAssemblyRaw != null)
//            {
//                LoadLatestAssembly();
//            }

//            try
//            {
//                Layout();
//            }
//            catch (Exception ex)
//            {
//                UnhandledException?.Invoke(this, new UnhandledExceptionEventArgs(ex, false));
//            }

//            //Device.BeginInvokeOnMainThread(OnLayout);
//        }

//        private void LoadLatestAssembly()
//        {
//            byte[] latestAssemblyRaw;
//            byte[] latestAssemblySymbolStoreRaw;

//            lock (this)
//            {
//                latestAssemblyRaw = _latestAssemblyRaw;
//                latestAssemblySymbolStoreRaw = _latestAssemblySymbolStoreRaw;
//                _latestAssemblyRaw = null;
//                _latestAssemblySymbolStoreRaw = null;
//            }

//            try
//            {
//                var assembly = _latestAssemblySymbolStoreRaw == null ? Assembly.Load(latestAssemblyRaw) : Assembly.Load(latestAssemblyRaw, latestAssemblySymbolStoreRaw);
//                var type = assembly.GetType(_rootComponent.GetType().FullName);
//                var context = _rootComponent.Context;
//                _rootComponent = (RxComponent)Activator.CreateInstance(type);

//                //_rootComponent.Context = context;
//                Invalidate();
//            }
//            catch (Exception ex)
//            {
//                UnhandledException?.Invoke(this, new UnhandledExceptionEventArgs(ex, false));
//            }
//        }

//        protected override IEnumerable<VisualNode> RenderChildren()
//        {
//            yield return _rootComponent;
//        }
//    }
//}