using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace StardewModdingAPI.Framework.Reflection
{
    /// <summary>Generates a proxy class to access a mod API through an arbitrary interface.</summary>
    internal class InterfaceProxyBuilder
    {
        /*********
        ** Consts
        *********/
        private static readonly string TargetFieldName = "__Target";
        private static readonly string GlueFieldName = "__Glue";
        private static readonly MethodInfo CreateInstanceForProxyTypeNameMethod = typeof(InterfaceProxyGlue).GetMethod(nameof(InterfaceProxyGlue.CreateInstanceForProxyTypeName), new Type[] { typeof(string), typeof(object) });

        /*********
        ** Fields
        *********/
        /// <summary>The target class type.</summary>
        private readonly Type TargetType;

        /// <summary>The full name of the generated proxy type.</summary>
        private readonly string ProxyTypeName;

        /// <summary>The generated proxy type.</summary>
        private Type ProxyType;


        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="targetType">The target type.</param>
        /// <param name="proxyTypeName">The type name to generate.</param>
        public InterfaceProxyBuilder(Type targetType, string proxyTypeName)
        {
            // validate
            this.TargetType = targetType ?? throw new ArgumentNullException(nameof(targetType));
            this.ProxyTypeName = proxyTypeName ?? throw new ArgumentNullException(nameof(proxyTypeName));
        }


        /// <summary>Creates and sets up the proxy type.</summary>
        /// <param name="factory">The <see cref="InterfaceProxyFactory"/> that requested to build a proxy.</param>
        /// <param name="moduleBuilder">The CLR module in which to create proxy classes.</param>
        /// <param name="interfaceType">The interface type to implement.</param>
        /// <param name="sourceModID">The unique ID of the mod consuming the API.</param>
        /// <param name="targetModID">The unique ID of the mod providing the API.</param>
        public void SetupProxyType(InterfaceProxyFactory factory, ModuleBuilder moduleBuilder, Type interfaceType, string sourceModID, string targetModID)
        {
            // define proxy type
            TypeBuilder proxyBuilder = moduleBuilder.DefineType(this.ProxyTypeName, TypeAttributes.Public | TypeAttributes.Class);
            proxyBuilder.AddInterfaceImplementation(interfaceType);

            // create fields to store target instance and proxy factory
            FieldBuilder targetField = proxyBuilder.DefineField(TargetFieldName, this.TargetType, FieldAttributes.Private);
            FieldBuilder glueField = proxyBuilder.DefineField(GlueFieldName, typeof(InterfaceProxyGlue), FieldAttributes.Private);

            // create constructor which accepts target instance + factory, and sets fields
            {
                ConstructorBuilder constructor = proxyBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard | CallingConventions.HasThis, new[] { this.TargetType, typeof(InterfaceProxyGlue) });
                ILGenerator il = constructor.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0); // this
                // ReSharper disable once AssignNullToNotNullAttribute -- never null
                il.Emit(OpCodes.Call, typeof(object).GetConstructor(new Type[0])); // call base constructor
                il.Emit(OpCodes.Ldarg_0);      // this
                il.Emit(OpCodes.Ldarg_1);      // load argument
                il.Emit(OpCodes.Stfld, targetField); // set field to loaded argument
                il.Emit(OpCodes.Ldarg_0);      // this
                il.Emit(OpCodes.Ldarg_2);      // load argument
                il.Emit(OpCodes.Stfld, glueField); // set field to loaded argument
                il.Emit(OpCodes.Ret);
            }

            var allTargetMethods = this.TargetType.GetMethods().ToList();
            foreach (Type targetInterface in this.TargetType.GetInterfaces())
            {
                foreach (MethodInfo targetMethod in targetInterface.GetMethods())
                {
                    if (!targetMethod.IsAbstract)
                        allTargetMethods.Add(targetMethod);
                }
            }

            MatchingTypesResult AreTypesMatching(Type targetType, Type proxyType, MethodTypeMatchingPart part)
            {
                var typeA = part == MethodTypeMatchingPart.Parameter ? targetType : proxyType;
                var typeB = part == MethodTypeMatchingPart.Parameter ? proxyType : targetType;

                if (typeA.IsGenericMethodParameter != typeB.IsGenericMethodParameter)
                    return MatchingTypesResult.False;
                // TODO: decide if "assignable" checking is desired (instead of just 1:1 type equality)
                if (typeA.IsGenericMethodParameter ? typeA.GenericParameterPosition == typeB.GenericParameterPosition : typeA.IsAssignableFrom(typeB))
                    return MatchingTypesResult.True;

                if (!proxyType.IsGenericMethodParameter && proxyType.IsInterface && proxyType.Assembly == interfaceType.Assembly)
                    return MatchingTypesResult.IfProxied;
                return MatchingTypesResult.False;
            }

            // proxy methods
            foreach (MethodInfo proxyMethod in interfaceType.GetMethods())
            {
                var proxyMethodParameters = proxyMethod.GetParameters();
                var proxyMethodGenericArguments = proxyMethod.GetGenericArguments();

                foreach (MethodInfo targetMethod in allTargetMethods)
                {
                    // checking if `targetMethod` matches `proxyMethod`

                    if (targetMethod.Name != proxyMethod.Name)
                        continue;
                    if (targetMethod.GetGenericArguments().Length != proxyMethodGenericArguments.Length)
                        continue;
                    var positionsToProxy = new HashSet<int?>(); // null = return type; anything else = parameter position

                    switch (AreTypesMatching(targetMethod.ReturnType, proxyMethod.ReturnType, MethodTypeMatchingPart.ReturnType))
                    {
                        case MatchingTypesResult.False:
                            continue;
                        case MatchingTypesResult.True:
                            break;
                        case MatchingTypesResult.IfProxied:
                            positionsToProxy.Add(null);
                            break;
                    }

                    var mParameters = targetMethod.GetParameters();
                    if (mParameters.Length != proxyMethodParameters.Length)
                        continue;
                    for (int i = 0; i < mParameters.Length; i++)
                    {
                        switch (AreTypesMatching(mParameters[i].ParameterType, proxyMethodParameters[i].ParameterType, MethodTypeMatchingPart.Parameter))
                        {
                            case MatchingTypesResult.False:
                                goto targetMethodLoopContinue;
                            case MatchingTypesResult.True:
                                break;
                            case MatchingTypesResult.IfProxied:
                                if (proxyMethodParameters[i].IsOut)
                                {
                                    positionsToProxy.Add(i);
                                    break;
                                }
                                else
                                {
                                    goto targetMethodLoopContinue;
                                }
                        }
                    }

                    // method matched; proxying

                    this.ProxyMethod(factory, proxyBuilder, proxyMethod, targetMethod, targetField, glueField, positionsToProxy, sourceModID, targetModID);
                    goto proxyMethodLoopContinue;
                    targetMethodLoopContinue:;
                }

                throw new InvalidOperationException($"The {interfaceType.FullName} interface defines method {proxyMethod.Name} which doesn't exist in the API.");
                proxyMethodLoopContinue:;
            }

            // save info
            this.ProxyType = proxyBuilder.CreateType();
        }

        /// <summary>Create an instance of the proxy for a target instance.</summary>
        /// <param name="targetInstance">The target instance.</param>
        /// <param name="factory">The <see cref="InterfaceProxyFactory"/> that requested to build a proxy.</param>
        public object CreateInstance(object targetInstance, InterfaceProxyFactory factory)
        {
            ConstructorInfo constructor = this.ProxyType.GetConstructor(new[] { this.TargetType, typeof(InterfaceProxyGlue) });
            if (constructor == null)
                throw new InvalidOperationException($"Couldn't find the constructor for generated proxy type '{this.ProxyType.Name}'."); // should never happen
            return constructor.Invoke(new[] { targetInstance, new InterfaceProxyGlue(factory) });
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Define a method which proxies access to a method on the target.</summary>
        /// <param name="factory">The <see cref="InterfaceProxyFactory"/> that requested to build a proxy.</param>
        /// <param name="proxyBuilder">The proxy type being generated.</param>
        /// <param name="proxy">The proxy method.</param>
        /// <param name="target">The target method.</param>
        /// <param name="instanceField">The proxy field containing the API instance.</param>
        /// <param name="glueField">The proxy field containing an <see cref="InterfaceProxyGlue"/>.</param>
        /// <param name="positionsToProxy">Parameter type positions (or null for the return type) for which types should also be proxied.</param>
        /// <param name="sourceModID">The unique ID of the mod consuming the API.</param>
        /// <param name="targetModID">The unique ID of the mod providing the API.</param>
        private void ProxyMethod(InterfaceProxyFactory factory, TypeBuilder proxyBuilder, MethodInfo proxy, MethodInfo target, FieldBuilder instanceField, FieldBuilder glueField, ISet<int?> positionsToProxy, string sourceModID, string targetModID)
        {
            MethodBuilder methodBuilder = proxyBuilder.DefineMethod(proxy.Name, MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.Virtual);

            // set up generic arguments
            Type[] proxyGenericArguments = proxy.GetGenericArguments();
            string[] genericArgNames = proxyGenericArguments.Select(a => a.Name).ToArray();
            GenericTypeParameterBuilder[] genericTypeParameterBuilders = proxyGenericArguments.Length == 0 ? null : methodBuilder.DefineGenericParameters(genericArgNames);
            for (int i = 0; i < proxyGenericArguments.Length; i++)
                genericTypeParameterBuilders[i].SetGenericParameterAttributes(proxyGenericArguments[i].GenericParameterAttributes);

            // set up return type
            Type returnType = proxy.ReturnType.IsGenericMethodParameter ? genericTypeParameterBuilders[proxy.ReturnType.GenericParameterPosition] : proxy.ReturnType;
            methodBuilder.SetReturnType(returnType);

            // set up parameters
            Type[] argTypes = proxy.GetParameters()
                .Select(a => a.ParameterType)
                .Select(t => t.IsGenericMethodParameter ? genericTypeParameterBuilders[t.GenericParameterPosition] : t)
                .ToArray();
            methodBuilder.SetParameters(argTypes);

            // proxy additional types
            string returnValueProxyTypeName = null;
            string[] parameterProxyTypeNames = new string[argTypes.Length];
            if (positionsToProxy.Count > 0)
            {
                var targetParameters = target.GetParameters();
                foreach (int? position in positionsToProxy)
                {
                    // we don't check for generics here, because earlier code does and generic positions won't end up here
                    if (position == null) // it's the return type
                    {
                        var builder = factory.ObtainBuilder(target.ReturnType, proxy.ReturnType, sourceModID, targetModID);
                        returnType = proxy.ReturnType;
                        returnValueProxyTypeName = builder.ProxyTypeName;
                    }
                    else // it's one of the parameters
                    {
                        var builder = factory.ObtainBuilder(targetParameters[position.Value].ParameterType, argTypes[position.Value], sourceModID, targetModID);
                        argTypes[position.Value] = proxy.ReturnType;
                        parameterProxyTypeNames[position.Value] = builder.ProxyTypeName;
                    }
                }

                methodBuilder.SetReturnType(returnType);
                methodBuilder.SetParameters(argTypes);
            }

            // create method body
            {
                ILGenerator il = methodBuilder.GetILGenerator();

                var resultLocal = il.DeclareLocal(typeof(object)); // we store both unmodified and modified in here, hence `object`
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldfld, instanceField);
                for (int i = 0; i < argTypes.Length; i++)
                    il.Emit(OpCodes.Ldarg, i + 1);
                il.Emit(OpCodes.Callvirt, target);
                il.Emit(OpCodes.Stloc, resultLocal);

                if (returnValueProxyTypeName != null)
                {
                    // if (unmodifiedResultLocal == null) jump
                    var isNullLabel = il.DefineLabel();
                    il.Emit(OpCodes.Ldloc, resultLocal);
                    il.Emit(OpCodes.Brfalse, isNullLabel);

                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldfld, glueField);
                    il.Emit(OpCodes.Ldstr, returnValueProxyTypeName);
                    il.Emit(OpCodes.Ldloc, resultLocal);
                    il.Emit(OpCodes.Call, CreateInstanceForProxyTypeNameMethod);
                    il.Emit(OpCodes.Stloc, resultLocal);

                    il.MarkLabel(isNullLabel);
                }

                // return result
                il.Emit(OpCodes.Ldloc, resultLocal);
                il.Emit(OpCodes.Ret);
            }
        }

        /// <summary>The part of a method that is being matched.</summary>
        private enum MethodTypeMatchingPart
        {
            ReturnType, Parameter
        }

        /// <summary>The result of matching a target and a proxy type.</summary>
        private enum MatchingTypesResult
        {
            False, IfProxied, True
        }
    }
}
