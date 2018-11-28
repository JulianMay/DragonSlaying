using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DragonSlaying
{
    /// <summary>
    /// Helps creates instances of classes with bloated constructors
    /// Dependencies are defined by type=>instance. Undefined dependencies are replaced by a strict mock.
    /// </summary>
    public class BloatedConstructorHelper
    {
        public static Builder<TSut> Make<TSut>()
        {
            return new Builder<TSut>();
        }

        public class Builder<TSut>
        {
            private Dictionary<Type, object> _definedDependencies = new Dictionary<Type, object>();
            private ConstructorInfo _ctor;

            public Builder()
            {
                _ctor = GetSutConstructor();
                
                //all null's will be resolved when building
                foreach (var ctorParam in _ctor.GetParameters())
                {
                    _definedDependencies.Add(ctorParam.ParameterType, null);
                }
            }

            private static ConstructorInfo GetSutConstructor()
            {
                var ctors = typeof(TSut).GetConstructors();

                if (ctors.Length == 0)
                    throw new InvalidOperationException($"'BloatedConstructorHelper' only works for types that has at least 1 public constructor. {typeof(TSut).FullName} has no public constructors");

                if (ctors.Length == 1)
                    return ctors.First();

                //By convention, when there are more than 1 constructor, we choose the one with most arguments
                return ctors.OrderByDescending(x => x.GetParameters().Length).First();
            }

            private void AutoMockRemainingDependencies()
            {
                var parameterTypes = _definedDependencies.Keys.ToArray();
                
                foreach (var paramType in parameterTypes)
                {
                    if(_definedDependencies[paramType] == null)
                        _definedDependencies[paramType] = MockDependency(paramType);                    
                }
            }

            private object MockDependency(Type t)
            {
                //Fallback til Mock
                Type myParameterizedSomeClass = typeof(Mock<>).MakeGenericType(t);
                ConstructorInfo c = myParameterizedSomeClass.GetConstructor(new[] { typeof(MockBehavior) });
                return ((Mock)c.Invoke(new object[] { MockBehavior.Strict })).Object;
            }


            /// <summary>
            /// Will throw an error if the SUT has no such constructor dependency           
            /// </summary>
            /// <param name="dependency"></param>
            /// <typeparam name="TDependency"></typeparam>
            /// <returns></returns>
            public Builder<TSut> With<TDependency>(TDependency dependency)
            {
                var t = typeof(TDependency);

                if (!_definedDependencies.ContainsKey(t))
                    throw new ArgumentException($"The (longest) constructor of '{typeof(TSut).FullName}' does not take a {t.FullName}");

                _definedDependencies[typeof(TDependency)] = dependency;
                return this;
            }

            /// <summary>
            /// If not null, the specified dependency will be used
            /// If it is null, the default strict-mock will be used
            /// </summary>
            public Builder<TSut> WithOrDefault<TDependency>(TDependency dependency)
            {
                if (dependency == null)
                    return this;

                return With(dependency);
            }

            public TSut Build()
            {                
                AutoMockRemainingDependencies();
                
                var args = _ctor.GetParameters()
                    .Select(p => _definedDependencies[p.ParameterType]).ToArray();

                return (TSut)_ctor.Invoke(args);
            }

        }
    }
}
