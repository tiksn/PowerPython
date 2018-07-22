using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace TIKSN.PowerPython.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, "Python")]
    public class InvokePythonCommand : Command
    {
        [Parameter]
        public Dictionary<string, string> Arguments { get; set; }

        [Parameter(Mandatory = true)]
        public string ScriptFile { get; set; }

        protected override Task ProcessRecordAsync(CancellationToken cancellationToken)
        {
            var engine = Python.CreateEngine();

            var scope = engine.CreateScope();

            scope.SetVariable("params", Arguments);

            ScriptSource source = engine.CreateScriptSourceFromFile(ScriptFile);

            object result = source.Execute(scope);

            var variables = scope.GetVariableNames().Where(name => scope.GetVariable(name)).ToArray();

            WriteObject(variables, true);

            return Task.CompletedTask;
        }
    }
}
