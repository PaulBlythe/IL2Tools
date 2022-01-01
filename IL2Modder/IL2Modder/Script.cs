using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom.Compiler;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using IL2Modder.IL2;

namespace IL2Modder
{
    public class Script
    {
        Assembly assembly;
        AircraftInterface scriptObject;

        

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

            assembly =  result.CompiledAssembly;
            return result;
        }

        public void PrepareScript()
        {
            // Now that we have a compiled script, lets run them
            foreach (Type type in assembly.GetExportedTypes())
            {
                foreach (Type iface in type.GetInterfaces())
                {
                    if (iface == typeof(AircraftInterface))
                    {
                        // yay, we found a script interface, lets create it and run it!

                        // Get the constructor for the current type
                        // you can also specify what creation parameter types you want to pass to it,
                        // so you could possibly pass in data it might need, or a class that it can use to query the host application
                        ConstructorInfo constructor = type.GetConstructor(System.Type.EmptyTypes);
                        if (constructor != null && constructor.IsPublic)
                        {
                            
                            // we specified that we wanted a constructor that doesn't take parameters, so don't pass parameters
                            scriptObject = constructor.Invoke(null) as AircraftInterface;
                            scriptObject.onLoad();
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

        public void moveGear(float f)
        {
            scriptObject.moveGear(f);
        }
        public void moveRudder(float f)
        {
            scriptObject.moveRudder(f);
        }
        public void doMurderPilot(int i)
        {
            scriptObject.doMurderPilot(i);
        }
        public void moveFlaps(float f)
        {
            scriptObject.moveFlap(f);
        }
        public void moveBayDoor(float f)
        {
            scriptObject.moveBayDoor(f);
        }
        public void moveWingFold(float f)
        {
            scriptObject.moveWingFold(f);
        }
        public void moveCockpitDoor(float f)
        {
            scriptObject.moveCockpitDoor(f);
        }
        public List<AircraftActions> GetResults()
        {
            return scriptObject.GetResults();
        }
        public void moveAirBrake(float f)
        {
            scriptObject.moveAirBrake(f);
        }
        public bool turretAngles(int paramInt, float[] paramArrayOfFloat)
        {
            return scriptObject.turretAngles(paramInt, paramArrayOfFloat);
        }
        public void moveElevator(float f)
        {
            scriptObject.moveElevator(f);
        }
        public void moveAileron(float f)
        {
            scriptObject.moveAileron(f);
        }
        public void moveArrestorHook(float f)
        {
            scriptObject.moveArrestorHook(f);
        }
        public void update()
        {
            KeyboardState ks = Keyboard.GetState();
            Keys[] pressed = ks.GetPressedKeys();
            List<String> c = new List<String>();
            foreach (Keys k in pressed)
            {
                c.Add(k.ToString());
            }
            scriptObject.update(c.ToArray());
        }
        public void moveFan(float f)
        {
            scriptObject.moveFan(f);
        }
    }
}

