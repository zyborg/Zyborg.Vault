
// Based on:
//    https://damsteen.nl/blog/2016/06/08/ordered-tests-with-nunit-mstest-xunit-pt4-xunit

using System;

namespace Zyborg.Testing.XunitExt
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CollectionDependsOnAttribute : Attribute
    {
        public CollectionDependsOnAttribute(params Type[] dependencies)
        {
            this.Dependencies = dependencies;
        }

        public Type[] Dependencies { get; }
    }
}
