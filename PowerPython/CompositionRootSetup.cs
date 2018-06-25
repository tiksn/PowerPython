using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Management.Automation;
using TIKSN.DependencyInjection;

namespace TIKSN.PowerPython
{
    public class CompositionRootSetup : AutofacPlatformCompositionRootSetupBase
    {
        private readonly Cmdlet _cmdlet;

        public CompositionRootSetup(IConfigurationRoot configurationRoot, Cmdlet cmdlet) : base(configurationRoot)
        {
            _cmdlet = cmdlet ?? throw new ArgumentNullException(nameof(cmdlet));
        }

        protected override void ConfigureContainerBuilder(ContainerBuilder builder)
        {
            builder.RegisterInstance(_cmdlet);
        }

        protected override void ConfigureOptions(IServiceCollection services, IConfigurationRoot configuration)
        {
        }
    }
}