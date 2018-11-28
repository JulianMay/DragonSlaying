using System;

namespace DragonSlaying.Tests
{
    /// <summary>
    /// Imagine 7 or even 10 (or more?) constructor dependencies, as a result of refactoring some god class / big ball of mud
    /// </summary>
    public class BBOM_WithOnlyAbstractDependencies
    {
        private readonly IDependencyA _a;
        private readonly IDependencyB _b;
        

        public BBOM_WithOnlyAbstractDependencies(IDependencyA a, IDependencyB b)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _b = b ?? throw new ArgumentNullException(nameof(b));
            //....
        }
        
        public double SomeMethodDependingOnA(string someArgument)
        {
            var x =_a.YadaYada(someArgument);
            double someResult = x * 2;
            return someResult;
        } 
    }

    /// <summary>
    /// //Sometimes, depending on a type is easier (like Func/Action<T>)
    /// </summary>
    public class BBOM_WithClassDependencies
    {
        private readonly IDependencyA _a;
        private readonly ImplementationX _b;
        
        public BBOM_WithClassDependencies(IDependencyA a, ImplementationX b)
        {
            _a = a ?? throw new ArgumentNullException(nameof(a));
            _b = b ?? throw new ArgumentNullException(nameof(b));
            //....
        }

        
    }

    public interface IDependencyA
    {
        int YadaYada(string blabla);
    }

    public class SomeImplementationOfDependencyA : IDependencyA
    {
        public int YadaYada(string blabla)
        {
            return int.Parse(blabla);
        }
    }

    public interface IDependencyB
    {
        string Yxoktl(int magicNumberz);
    }

    
    public class ImplementationX
    {
        private int _x, _y;
        
        /// <summary>
        /// Any class/struct with constructor arguments cannot be autoMocked. But... ValueObjects... right?
        /// </summary>
        public ImplementationX(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public byte[] SomethingImportant()
        {
            return null;
        }
    }
}