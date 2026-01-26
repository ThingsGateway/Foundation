using CSScripting;
using CSScriptLib;
using System.Text.Json;

using ThingsGateway.Foundation.Common;
using ThingsGateway.Foundation.Common.Log;
using ThingsGateway.Gateway.Application.Extensions;

namespace ThingsGateway.Foundation.Sample
{
    internal sealed class Program
    {
        private static async Task Main(string[] args)
        {
            Console.ReadLine();
            for (int i = 0; i < 1; i++)
            {
                Test();
            }
            Console.ReadLine();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            Console.ReadLine();
        }

        private static ReadWriteExpressions Test()
        {
            ReadWriteExpressions? runScript = CSScript.RoslynEvaluator.With(eval => eval.IsAssemblyUnloadingEnabled = true).LoadCode<ReadWriteExpressions>(
   $@"
        using System;
        using System.Linq;
        using System.Collections.Generic;
        using Newtonsoft.Json;
        using Newtonsoft.Json.Linq;
        using ThingsGateway.Gateway.Application;
        using ThingsGateway.Foundation.Common.StringExtension;
        using ThingsGateway.Foundation.Common;
        using ThingsGateway.Foundation.Common.Extension;
        using ThingsGateway.Foundation.Common.Json.Extension;
        using ThingsGateway.Gateway.Application.Extensions;
        public class Script:ReadWriteExpressions
        {{
            public override object GetNewValue(object raw)
            {{
                   return 1;
            }}
        }}
    ");

            runScript.GetNewValue(1);

            //runScript.GetType().Assembly.Unload();
            return runScript;
        }
    }
}
