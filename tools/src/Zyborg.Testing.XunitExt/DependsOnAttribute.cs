
// Based on:
//    https://damsteen.nl/blog/2016/06/08/ordered-tests-with-nunit-mstest-xunit-pt4-xunit

using System;

namespace Zyborg.Testing.XunitExt
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DependsOnAttribute : Attribute
    {

        public DependsOnAttribute(params string[] dependencies)
        {
            this.Dependencies = dependencies;
        }

        public string[] Dependencies
        { get; }
    }
}
