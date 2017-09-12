using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;

namespace Zyborg.Vault.Server.Protocol
{
    public class SubRoutable<T>
    {
        private List<MethodMatcher> _readMatchers = new List<MethodMatcher>();
        private List<MethodMatcher> _writeMatchers = new List<MethodMatcher>();

        public void Init()
        {
            var type = typeof(T);
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (var m in methods)
            {
                foreach (var a in m.GetCustomAttributes())
                {
                    switch (a)
                    {
                        case SubReadAttribute ra:
                            var rt = TemplateParser.Parse(ra.PathPattern);
                            var rtm = new TemplateMatcher(rt, null);
                            _readMatchers.Add(new MethodMatcher
                            {
                                Method = m,
                                Matcher = rtm,
                            });
                            break;
                        case SubWriteAttribute wa:
                            var wt = TemplateParser.Parse(wa.PathPattern);
                            var wtm = new TemplateMatcher(wt, null);
                            _writeMatchers.Add(new MethodMatcher
                            {
                                Method = m,
                                Matcher = wtm,
                            });
                            break;
                    }
                }
            }
        }

        public void Write(object target, string path, string payload)
        {
            foreach (var mm in _writeMatchers)
            {
                if (mm.Matcher.TryMatch(path, null))
                {

                    mm.Method.Invoke(target, new[] { path });
                }
            }
        }

        public class MethodMatcher
        {
            public MethodInfo Method
            { get; set; }

            public TemplateMatcher Matcher
            { get; set; }
        }
    }
}