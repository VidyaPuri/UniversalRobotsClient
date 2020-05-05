using Caliburn.Micro;
using System.Windows;
using RobotClient.ViewModels;
using System;
using System.Collections.Generic;
using RobotClient.Networking;
using RobotClient.Move;
using RobotInterface.ViewModels;

namespace RobotClient
{
    public class Bootstrapper : BootstrapperBase
    {
        private SimpleContainer _container = new SimpleContainer();

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            _container.Instance(_container);
            _container.Singleton<ShellViewModel>();
            _container.Singleton<NetworkingViewModel>();
            //_container.Singleton<TimelineViewModel>();

            _container.PerRequest<SocketClient>();
            _container.PerRequest<RobotCommand>();
            _container.PerRequest<ControllerClass>();
            _container
                .Singleton<IWindowManager, WindowManager>()
                .Singleton<IEventAggregator, EventAggregator>();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }
    }
}
