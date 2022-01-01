using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using System.Reflection;
using IL2Modder.IL2;

namespace IL2Modder
{
    public class CockpitScript
    {
        Assembly assembly;
        CockpitInterface scriptObject;

        public CompilerResults Compile(String code)
        {
            Microsoft.CSharp.CSharpCodeProvider csProvider = new Microsoft.CSharp.CSharpCodeProvider();
            CompilerParameters options = new CompilerParameters();
            options.GenerateExecutable = false;
            options.GenerateInMemory = true;
            options.ReferencedAssemblies.Add(Assembly.GetExecutingAssembly().Location);
            CompilerResults result;
            result = csProvider.CompileAssemblyFromSource(options, code);

            if (result.Errors.HasErrors)
            {
                assembly = null;
                return result;
            }

            assembly = result.CompiledAssembly;
            return result;
        }
        public void PrepareScript()
        {
            // Now that we have a compiled script, lets run them
            foreach (Type type in assembly.GetExportedTypes())
            {
                foreach (Type iface in type.GetInterfaces())
                {
                    if (iface == typeof(CockpitInterface))
                    {
                        // yay, we found a script interface, lets create it and run it!

                        // Get the constructor for the current type
                        // you can also specify what creation parameter types you want to pass to it,
                        // so you could possibly pass in data it might need, or a class that it can use to query the host application
                        ConstructorInfo constructor = type.GetConstructor(System.Type.EmptyTypes);
                        if (constructor != null && constructor.IsPublic)
                        {

                            // we specified that we wanted a constructor that doesn't take parameters, so don't pass parameters
                            scriptObject = constructor.Invoke(null) as CockpitInterface;
                        }
                        else
                        {
                            // and even more friendly and explain that there was no valid constructor
                            // found and thats why this script object wasn't run

                        }
                    }
                }
            }
        }

        public void reflectWorldToInstruments(float f, FlightVariables fv)
        {
            scriptObject.reflectWorldToInstruments(f, fv);
        }
        public List<AircraftActions> GetResults()
        {
            return scriptObject.GetResults();
        }
    }
}
