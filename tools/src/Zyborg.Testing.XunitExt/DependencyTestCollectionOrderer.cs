using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace Zyborg.Testing.XunitExt
{
    /// <summary>
    /// Implements a Test Collection ordering strategy for Xunit based on
    /// attributed test classes.
    /// </summary>
    /// <remarks>
    /// To enable this ordering strategy, define an assembly-level attribute
    /// using Xunit's <see cref="TestCollectionOrdererAttribute" /> class
    /// as follows:
    /// <code>
    /// [assembly: TestCollectionOrderer(
    ///         DependencyTestCollectionOrderer.Name,
    ///         DependencyTestCollectionOrderer.Assembly)]
    /// </code>
    /// <para>
    /// Adapted from the original implementation at:
    ///    https://github.com/Sebazzz/NetUnitTestComparison/blob/master/src/XUnit.Extensions.TestOrdering/DependencyTestCollectionOrderer.cs
    /// </para>
    /// </remarks>
    public sealed class DependencyTestCollectionOrderer : ITestCollectionOrderer
    {
        public const string Assembly = "Zyborg.Testing.XunitExt";
        public const string Name = Assembly + "." + nameof(DependencyTestCollectionOrderer);

        public IEnumerable<ITestCollection> OrderTestCollections(IEnumerable<ITestCollection> testCollections)
        {
            var orderedCollection = DependencySorter.Sort(testCollections.Select(x => new TestCollectionWrapper(x)))
                    .Select(x => x.TestCollection);
            
Console.WriteLine("My Collection Orderer Invoked: " + string.Join(",", orderedCollection.Select(x => x.DisplayName)));

            return orderedCollection;
        }

        private sealed class TestCollectionWrapper : IDependencyIndicator<TestCollectionWrapper>
        {
            private readonly Type[] _dependsOnTypes;
            private readonly Type _type;

            public TestCollectionWrapper(ITestCollection testCollection)
            {
                this.TestCollection = testCollection;

                this._type = TestCollectionCache.Instance.GetType(testCollection.DisplayName);

                CollectionDependsOnAttribute attribute = this._type?.GetTypeInfo()
                        .GetCustomAttribute<CollectionDependsOnAttribute>();

                if (attribute != null)
                {
                    this._dependsOnTypes = attribute.Dependencies;
                }
            }

            public string TestType => this._type?.ToString();
            public IEnumerable<string> TestTypeDependency =>
                    this._dependsOnTypes?.Select(x => x.ToString());

            public ITestCollection TestCollection { get; }

            public bool HasDependencies => this._dependsOnTypes?.Length > 0;

            public bool IsDependencyOf(TestCollectionWrapper other)
            {
                if (this.TestType == null) return false;
                if (other.TestTypeDependency?.Count() == 0) return false;
                return other.TestTypeDependency.Contains(this.TestType);
            }

            public bool Equals(TestCollectionWrapper other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(this.TestCollection, other.TestCollection);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj is TestCollectionWrapper && this.Equals((TestCollectionWrapper)obj);
            }

            public override int GetHashCode() => TestCollection?.GetHashCode() ?? 0;

            public override string ToString() => TestCollection.DisplayName;
        }

        /// <summary>
        /// Helper class for looking up types by collection name
        /// </summary>
        private sealed class TestCollectionCache
        {
            public static readonly TestCollectionCache Instance = new TestCollectionCache();

            private readonly Dictionary<string, Type> _collectionTypeMap;

            private TestCollectionCache()
            {
                this._collectionTypeMap = new Dictionary<string, Type>(StringComparer.Ordinal);

                CreateCollectionTypeMap(this._collectionTypeMap);
            }

            private static void CreateCollectionTypeMap(Dictionary<string, Type> collectionTypeMap)
            {
                var types = from assembly in AppDomain.CurrentDomain.GetAssemblies()
                        where assembly.IsDynamic == false
                        from type in assembly.GetExportedTypes()
                        // Xunit accesses attribute data by constructor arguments
                        let collectionAttrData = type.GetCustomAttributesData().FirstOrDefault(
                                x => x.AttributeType == typeof(CollectionAttribute))
                        where collectionAttrData != null
                        select new
                        {
                            CollectionName = collectionAttrData.ConstructorArguments[0].Value as string,
                            CollectionType = type
                        };

                // We don't use IDictionary because we can't give an useful exception then
                foreach (var tuple in types)
                {
                    // We don't support null collection names
                    if (tuple.CollectionName == null) continue;

                    try
                    {
                        collectionTypeMap.Add(tuple.CollectionName, tuple.CollectionType);
                    }
                    catch (ArgumentException)
                    {
                        throw new InvalidOperationException(
                                $"Duplicate collection name: {tuple.CollectionName}."
                                 + " Existing collection type with same name:"
                                 + " {collectionTypeMap[tuple.CollectionName]}."
                                 + " Trying to add {tuple.CollectionType}");
                    }
                }
            }

            public Type GetType(string collectionName)
            {
                this._collectionTypeMap.TryGetValue(collectionName, out var type);
                return type;
            }
        }
    }
}