using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace StardewModdingAPI.Framework.Reflection
{
    /// <summary>Generates proxy classes to access mod APIs through an arbitrary interface.</summary>
    internal class InterfaceProxyBuilder
    {
        /*********
        ** Properties
        *********/
        /// <summary>The CLR module in which to create proxy classes.</summary>
        private readonly ModuleBuilder ModuleBuilder;

        /// <summary>The generated proxy types.</summary>
        private readonly IDictionary<string, Type> GeneratedTypes = new Dictionary<string, Type>();


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        public InterfaceProxyBuilder()
        {
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName($"StardewModdingAPI.Proxies, Version={this.GetType().Assembly.GetName().Version}, Culture=neutral"), AssemblyBuilderAccess.Run);
            this.ModuleBuilder = assemblyBuilder.DefineDynamicModule("StardewModdingAPI.Proxies");
        }

        /// <summary>Create an API proxy.</summary>
        /// <typeparam name="TInterface">The interface through which to access the API.</typeparam>
        /// <param name="instance">The API instance to access.</param>
        /// <param name="sourceModID">The unique ID of the mod consuming the API.</param>
        /// <param name="targetModID">The unique ID of the mod providing the API.</param>
        public TInterface CreateProxy<TInterface>(object instance, string sourceModID, string targetModID)
            where TInterface : class
        {
            // validate
            if (instance == null)
                throw new InvalidOperationException("Can't proxy access to a null API.");
            if (!typeof(TInterface).IsInterface)
                throw new InvalidOperationException("The proxy type must be an interface, not a class.");

            // get proxy type
            Type targetType = instance.GetType();
            string proxyTypeName = $"StardewModdingAPI.Proxies.From<{sourceModID}_{typeof(TInterface).FullName}>_To<{targetModID}_{targetType.FullName}>";
            if (!this.GeneratedTypes.TryGetValue(proxyTypeName, out Type type))
            {
                type = this.CreateProxyType(proxyTypeName, typeof(TInterface), targetType);
                this.GeneratedTypes[proxyTypeName] = type;
            }

            // create instance
            ConstructorInfo constructor = type.GetConstructor(new[] { targetType });
            if (constructor == null)
                throw new InvalidOperationException($"Couldn't find the constructor for generated proxy type '{proxyTypeName}'."); // should never happen
            return (TInterface)constructor.Invoke(new[] { instance });
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Define a class which proxies access to a target type through an interface.</summary>
        /// <param name="proxyTypeName">The name of the proxy type to generate.</param>
        /// <param name="interfaceType">The interface type through which to access the target.</param>
        /// <param name="targetType">The target type to access.</param>
        private Type CreateProxyType(string proxyTypeName, Type interfaceType, Type targetType)
        {
            // define proxy type
            TypeBuilder proxyBuilder = this.ModuleBuilder.DefineType(proxyTypeName, TypeAttributes.Public | TypeAttributes.Class);
            proxyBuilder.AddInterfaceImplementation(interfaceType);

            // create field to store target instance
            FieldBuilder field = proxyBuilder.DefineField("__Target", targetType, FieldAttributes.Private);

            // create constructor which accepts target instance
            {
                ConstructorBuilder constructor = proxyBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard | CallingConventions.HasThis, new[] { targetType });
                ILGenerator il = constructor.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0); // this
                // ReSharper disable once AssignNullToNotNullAttribute -- never null
                il.Emit(OpCodes.Call, typeof(object).GetConstructor(new Type[0])); // call base constructor
                il.Emit(OpCodes.Ldarg_0);      // this
                il.Emit(OpCodes.Ldarg_1);      // load argument
                il.Emit(OpCodes.Stfld, field); // set field to loaded argument
                il.Emit(OpCodes.Ret);
            }

            // proxy methods
            foreach (MethodInfo proxyMethod in interfaceType.GetMethods())
            {
                var targetMethod = targetType.GetMethod(proxyMethod.Name, proxyMethod.GetParameters().Select(a => a.ParameterType).ToArray());
                if (targetMethod == null)
                    throw new InvalidOperationException($"The {interfaceType.FullName} interface defines method {proxyMethod.Name} which doesn't exist in the API.");

                this.ProxyMethod(proxyBuilder, targetMethod, field);
            }

            // create type
            return proxyBuilder.CreateType();
        }

        /// <summary>Define a method which proxies access to a method on the target.</summary>
        /// <param name="proxyBuilder">The proxy type being generated.</param>
        /// <param name="target">The target method.</param>
        /// <param name="instanceField">The proxy field containing the API instance.</param>
        private void ProxyMethod(TypeBuilder proxyBuilder, MethodInfo target, FieldBuilder instanceField)
        {
            Type[] argTypes = target.GetParameters().Select(a => a.ParameterType).ToArray();

            // create method
            MethodBuilder methodBuilder = proxyBuilder.DefineMethod(target.Name, MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual);
            methodBuilder.SetParameters(argTypes);
            methodBuilder.SetReturnType(target.ReturnType);

            // create method body
            {
                ILGenerator il = methodBuilder.GetILGenerator();

                // load target instance
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, instanceField);

                // invoke target method on instance
                for (int i = 0; i < argTypes.Length; i++)
                    il.Emit(OpCodes.Ldarg, i + 1);
                il.Emit(OpCodes.Call, target);

                // return result
                il.Emit(OpCodes.Ret);
            }
        }
    }
}
