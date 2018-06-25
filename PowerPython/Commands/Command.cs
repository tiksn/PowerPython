using System;
using TIKSN.PowerShell;

namespace TIKSN.PowerPython.Commands
{
    public abstract class Command : CommandBase
    {
        protected override IServiceProvider CreateServiceProvider()
        {
            var configurationRootSetup = new ConfigurationRootSetup();
            var configurationRoot = configurationRootSetup.GetConfigurationRoot();
            var compositionRootSetup = new CompositionRootSetup(configurationRoot, this);
            return compositionRootSetup.CreateServiceProvider();
        }
    }
}