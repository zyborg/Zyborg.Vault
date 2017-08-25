using System;
using System.Collections.Generic;
using System.Linq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Zyborg.Testing.XunitExt
{
    /// <summary>
    /// Implements a Test Collection ordering strategy for Xunit based on
    /// attributed test classes.
    /// </summary>
    /// <remarks>
    /// To enable this ordering strategy, define an assembly-level attribute
    /// using Xunit's <see cref="TestCaseOrdererAttribute" /> class
    /// as follows:
    /// <code>
    /// [assembly: TestCaseOrderer(
    ///         DependencyTestCaseOrderer.Name,
    ///         DependencyTestCaseOrderer.Assembly)]
    /// </code>
    /// <para>
    /// Adapted from the original implementation at:
    ///    https://github.com/Sebazzz/NetUnitTestComparison/blob/master/src/XUnit.Extensions.TestOrdering/DependencyTestCaseOrderer.cs
    /// </para>
    /// </remarks>
    public sealed class DependencyTestCaseOrderer : ITestCaseOrderer
    {
        public const string Assembly = "Zyborg.Testing.XunitExt";
        public const string Name = Assembly + "." + nameof(DependencyTestCaseOrderer);

        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
            where TTestCase : ITestCase
        {
Console.WriteLine("My Test Orderer Invoked!");
            try
            {
                var orderedTests = DependencySorter.Sort(testCases.Select(x => new TestCaseWrapper(x)))
                        .Select(x => x.TestCase).Cast<TTestCase>();
            
Console.WriteLine("My Test Orderer Invoked: " + string.Join(",", orderedTests.Select(x => x.DisplayName)));

                return orderedTests;
            }
            catch (Exception ex)
            {
                Console.WriteLine("EXCEPTION: " + ex);
                throw new Exception("???", ex);
            }
        }

        private sealed class TestCaseWrapper : IDependencyIndicator<TestCaseWrapper>
        {
            public TestCaseWrapper(ITestCase testCase)
            {
                this.TestCase = testCase;

                var attributeInfo = testCase.TestMethod.Method
                        .GetCustomAttributes(typeof(DependsOnAttribute))
                        .OfType<ReflectionAttributeInfo>()
                        .SingleOrDefault();

                if (attributeInfo != null)
                {
                    var attribute = (DependsOnAttribute)attributeInfo.Attribute;

                    this.TestMethodDependencies = attribute.Dependencies;
                }
            }

            public string TestMethod => this.TestCase.TestMethod.Method.Name;

            public ITestCase TestCase { get; }

            public IEnumerable<string> TestMethodDependencies { get; }

            public bool HasDependencies => this.TestMethodDependencies?.Count() > 0;

            public bool IsDependencyOf(TestCaseWrapper other)
            {
Console.WriteLine("HERE!");
                if (other.TestMethodDependencies?.Count() == 0) return false;
                return other.TestMethodDependencies.Contains(this.TestMethod);
            }

            public bool Equals(TestCaseWrapper other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(this.TestCase, other.TestCase);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is TestCaseWrapper && this.Equals((TestCaseWrapper)obj);
            }

            public override int GetHashCode() => TestCase?.GetHashCode() ?? 0;

            public override string ToString() => TestCase.DisplayName;
        }
    }
}