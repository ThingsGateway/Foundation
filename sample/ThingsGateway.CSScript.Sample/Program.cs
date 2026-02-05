using System.Runtime.Loader;
using Westwind.Scripting;

namespace ThingsGateway.Foundation.Sample
{

    public interface Test
    {
        public string Add(int num1, int num2);
        public string Multiply(int num1, int num2);

    }
    internal sealed class Program
    {
        private static async Task Main(string[] args)
        {
            var code = $@"

using System;
using ThingsGateway.Foundation.Sample;
using System.Net;
        public class Math{Random.Shared.Next()}:Test
	{{
		public string Add(int num1, int num2)
		{{
			// string templates
			var result = num1 + "" + "" + num2 + "" = "" + (num1 + num2);
			Console.WriteLine(result);
            var d=HttpWebRequest.DefaultMaximumErrorResponseLength;
            if(d>1000)
            {{
                Console.WriteLine(d);
            }}
			return result;
		}}
		
		public string Multiply(int num1, int num2)
		{{
			// string templates
			var result = $""{{num1}}  *  {{num2}} = {{ num1 * num2 }}"";
			Console.WriteLine(result);
			
			result = $""Take two: {{ result ?? ""No Result"" }}"";
			Console.WriteLine(result);
			
			return result;
		}}
	}}
    ";
            {
                var context = new AssemblyLoadContext("", true);
                var script = new CSharpScriptExecution();
                script.AlternateAssemblyLoadContext = context;

                var readWriteExpressions = script.CompileClassWithFile(code) as Test;
                Console.Write(readWriteExpressions.Add(5, 10));
            }
            {
                var context = new AssemblyLoadContext("", true);
                var script = new CSharpScriptExecution();
                script.AlternateAssemblyLoadContext = context;

                var readWriteExpressions = script.CompileClassWithFile(code) as Test;
                Console.Write(readWriteExpressions.Add(5, 10));
            }

            Console.ReadLine();
        }

    }
}
