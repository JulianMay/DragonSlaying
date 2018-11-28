using System;
using NUnit.Framework;

namespace DragonSlaying.Tests
{
    [TestFixture]
    class BloadedConstructorExamples
    {
        [Test]
        public void WithoutTheHelperAllDependenciesMustBeAccountedFor()
        {
            //This is why e need the helper - safe refactorings of poorly modelled code tend to
            //bloat constructors of (too) big classes, and we want tests to be both easy to write and maintain
            //... especially when you are about to make more meaningful (less mechanic) refactorings 
            Assert.Throws<ArgumentNullException>(() => new BBOM_WithOnlyAbstractDependencies(null, null));            
        }
        
        [Test]
        public void InterfacesAreStrictlyMockedByDefault()
        {
            //AutoMocking dependencies A and B
            var sut = BloatedConstructorHelper.Make<BBOM_WithOnlyAbstractDependencies>().Build();
            Assert.IsNotNull(sut);
            Assert.IsInstanceOf<BBOM_WithOnlyAbstractDependencies>(sut);
        }        
        
        [Test]
        public void TestFailsWhenUnspecifiedDependenciesAreCalled()
        {
            var sut = BloatedConstructorHelper.Make<BBOM_WithOnlyAbstractDependencies>().Build();
            Assert.Throws<Moq.MockException>(() => 
                sut.SomeMethodDependingOnA("I have not specified dependency A for this test"));
        }
        
        [Test]
        public void InvolvedDependenciesCanBeSpecified()
        {           
            var sut = BloatedConstructorHelper.Make<BBOM_WithOnlyAbstractDependencies>()
                .With<IDependencyA>(new SomeImplementationOfDependencyA())
                .Build();

            Assert.AreEqual(10, sut.SomeMethodDependingOnA("5"));                           
        }
        
        [Test]
        public void NonAbstractDependenciesWithConstructorArgumentsCannotBeAutoMocked()
        {           
            Assert.Throws<Castle.DynamicProxy.InvalidProxyConstructorArgumentsException>(()=>
                BloatedConstructorHelper.Make<BBOM_WithClassDependencies>()
                    .Build()); 
            
            Assert.DoesNotThrow(()=>
                    BloatedConstructorHelper.Make<BBOM_WithClassDependencies>()
                        .With(new ImplementationX(1,2))
                        .Build());                 
        }                
    }    
}
