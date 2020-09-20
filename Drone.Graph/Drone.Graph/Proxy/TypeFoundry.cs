using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Drone.Graph.Proxy
{
    public abstract class Foundry
    {
        protected AssemblyName AssemblyName { get; set; }

        protected AssemblyBuilder AssemblyBuilder { get; set; }

        protected ModuleBuilder ModuleBuilder { get; set; }

        protected TypeBuilder TypeBuilder { get; set; }

        protected IEnumerable<PropertyInteceptor> PropertyInterceptors { get; set; }
        protected IEnumerable<MethodInterceptor> MethodInterceptors { get; set; }

        public class PropertyDefinition
        {
            public string Name { get; set; }
            public Type Type { get; set; }
        }

        public class MethodDefinition
        {
            public string Name { get; set; }
            public Type Type { get; set; }
            public Type[] Parameters { get; set; }

            public Func<object[], object> MethodBody { get; set; }
        }

        public class PropertyInteceptor
        {
            public Expression<Action<PropertyBuilder>> Before { set; get; }
            public Expression<Action<PropertyBuilder>> After { set; get; }
        }

        public class MethodInterceptor
        {
            public Expression<Action<MethodBuilder>> Before { set; get; }
            public Expression<Action<MethodBuilder>> After { set; get; }
        }

        protected string name;

        protected Foundry(string name)
        {
            AssemblyName = new AssemblyName(name + "Assembly");
            AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(AssemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule($"{name}Module");
        }

        public abstract Type Build();

        public class TypeFoundry : Foundry
        {
            private List<MethodBuilder> _methodBuilder = new List<MethodBuilder>();

            public TypeFoundry(string name) : base(name)
            {
                TypeBuilder = ModuleBuilder.DefineType(name,
                    TypeAttributes.Public |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.Class);
            }

            /**
         * Add a method to the type being built.
         */

            public TypeFoundry AddMethod(string methodName,
                Type returnType,
                Type[] typeParameters)
            {
                MethodBuilder method =
                    TypeBuilder.DefineMethod(methodName,
                        MethodAttributes.Virtual |
                        MethodAttributes.Public,
                        returnType,
                        typeParameters);

                _methodBuilder.Add(method);

                return this;
            }

            public TypeFoundry AddProperty(string propertyName, Type propertyType)
            {
                var property = TypeBuilder.DefineProperty(propertyName, PropertyAttributes.None, propertyType, null);

                var newPropertyGet = TypeBuilder.DefineMethod(
                    "get_" + property.Name,
                    MethodAttributes.Virtual |
                    MethodAttributes.Public |
                    MethodAttributes.SpecialName,
                    property.PropertyType,
                    Type.EmptyTypes);

                var newPropertySet = TypeBuilder.DefineMethod(
                    "set_" + property.Name,
                    MethodAttributes.Virtual |
                    MethodAttributes.Public |
                    MethodAttributes.SpecialName,
                    null,
                    new Type[] {property.PropertyType});

                var newPropertyField =
                    TypeBuilder.DefineField("__" + propertyName, propertyType, FieldAttributes.Private);

                ILGenerator getIlGenerator = newPropertyGet.GetILGenerator();

                getIlGenerator.Emit(OpCodes.Ldarg_0);
                getIlGenerator.Emit(OpCodes.Ldfld, newPropertyField);
                getIlGenerator.Emit(OpCodes.Ret);


                ILGenerator setIlGenerator = newPropertySet.GetILGenerator();

                setIlGenerator.Emit(OpCodes.Ldarg_0);
                setIlGenerator.Emit(OpCodes.Ldarg_1);
                setIlGenerator.Emit(OpCodes.Stfld, newPropertyField);
                setIlGenerator.Emit(OpCodes.Ret);

                property.SetGetMethod(newPropertyGet);
                property.SetSetMethod(newPropertySet);

                return this;
            }

            public TypeFoundry AddSuperclass(Type type)
            {
                TypeBuilder.SetParent(type);

                return this;
            }

            public TypeFoundry AddInterface(Type type)
            {
                TypeBuilder.AddInterfaceImplementation(type);

                return this;
            }

            public override Type Build()
            {
                TypeBuilder.CreateTypeInfo();
                return TypeBuilder.CreateType();
            }
        }

        public class ReflectionException : Exception
        {
            public ReflectionException(string message) : base(message)
            {

            }
        }
    }
}