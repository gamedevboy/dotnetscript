using DotNetScript.Runtime;
using DotNetScript.Types;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DotNetScript.Types.Attributes;
using HostLibrary;

namespace DotNetScriptConsole
{
    internal static class Program
    {
        [ScriptHostWarpper(typeof(Foo<>))]
        private class FooWarpper<T> : Foo<T>
        {
            private ScriptObject _scriptObject;
            private ScriptObject ScriptObject => _scriptObject ?? (_scriptObject = ScriptObject.FromHostObject(this));

            public override void VirtualTest(string name)
            {
                var ret = ScriptObject.Invoke("VirtualTest", name);
                if( !ret.Item1 )
                    base.VirtualTest(name);
            }
        }

        private static void Main(string[] args)
        {
            using (var fs = File.OpenRead("../../../ScriptLibrary/bin/Debug/ScriptLibrary.dll"))
            {
                var asm = new ScriptAssembly(fs);
                ScriptContext.Load(asm);

                var type = asm.TypeSystem.GetType("ScriptLibrary.MyFoo");
                var myFoo = type.CreateInstance();
                var foo = myFoo.GetHostInstance<Foo<int>>();
                foo.VirtualTest("Abc");
                var method = type.GetMethod("Test2");
                var ret = method.Invoke(myFoo, 10);
                //ret = type.GetMethod("Test3").Invoke(null, 100);
            }
        }
    }
}
