using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Newtonsoft.Json;

namespace Zyborg.Vault.MockServer.Util
{
    /// <summary>
    /// Implements routing logic against a locally-scoped class.
    /// </summary>
    /// <remarks>
    /// Route matching is done in a similar fashion to MVC
    /// attributed route definition, but against a single class type.
    /// Routes are split between four List, Read, Write and Delete operations.
    /// Within each, routes are matched against defined route templates.
    /// When a route matches, it invokes the corresponding method by binding
    /// any parameters against the context:
    /// <list>
    /// <item><see cref="FromRouteAttribute">FromRoute</see></item>
    /// <definition>binds a parameter to the value defined by a named route segment.</definition>
    /// <item><see cref="FromBodyAttribute">FromRoute</see></item>
    /// <definition>binds a parameter to the entire content of the body payload.</definition>
    /// <item><see cref="FromFormAttribute">FromRoute</see></item>
    /// <definition>binds a parameter to the value defined a named property obtained by parsing
    ///     the payload a JSON object.</definition>
    /// <item><see cref="FromHeaderAttribute">FromRoute</see></item>
    /// <definition>binds a parameter to the value of a named request header (NOT CURRENTLY
    ///     IMPLEMENTED).</definition>
    /// </list>
    /// You can also optionally tag parameters with the <see cref="RequiredAttribute"/> to
    /// require a binding, otherwise an ArgumentException is thrown.
    /// </remarks>
    public class LocalRouter<T>
    {
        private Dictionary<string, MethodMatcher> _matchersByName = new Dictionary<string, MethodMatcher>();

        private List<MethodMatcher> _listMatchers = new List<MethodMatcher>();
        private List<MethodMatcher> _readMatchers = new List<MethodMatcher>();
        private List<MethodMatcher> _writeMatchers = new List<MethodMatcher>();
        private List<MethodMatcher> _deleteMatchers = new List<MethodMatcher>();

        public virtual void Init()
        {
            var type = typeof(T);
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (var m in methods)
            {
                foreach (var a in m.GetCustomAttributes())
                {
                    if (!(a is LocalRouteAttribute lra))
                        continue;

                    var name = lra.Name ?? m.Name;
                    if (string.IsNullOrEmpty(name))
                        throw new InvalidOperationException(
                                $"local route name could not be resolved for method [{m.Name}]");
                    if (_matchersByName.ContainsKey(name))
                        throw new InvalidOperationException(
                                $"duplicate local route name [{name}] for method [{m.Name}]");


                    var mm = new MethodMatcher
                    {
                        LocalRoute = lra,
                        Method = m,
                        Matcher = new TemplateMatcher(TemplateParser.Parse(lra.PathPattern), null),
                        Name = name,
                    };

                    switch (lra)
                    {
                        case LocalListRouteAttribute _:
                            _listMatchers.Add(mm);
                            break;
                        case LocalReadRouteAttribute _:
                            _readMatchers.Add(mm);
                            break;
                        case LocalWriteRouteAttribute _:
                            _writeMatchers.Add(mm);
                            break;
                        case LocalDeleteRouteAttribute _:
                            _deleteMatchers.Add(mm);
                            break;
                        default:
                            throw new InvalidOperationException("unknown local route type");
                    }
                    
                    _matchersByName.Add(name, mm);
                }
            }
        }

        public virtual async Task<(string name, IEnumerable<string>)> ListAsync(T target, string path)
        {
            if (!path.StartsWith("/"))
                path = $"/{path}";

            var rvd = new RouteValueDictionary();
            foreach (var mm in _listMatchers)
            {
                rvd.Clear();
                if (mm.Matcher.TryMatch(path, rvd))
                {
                    var pValues = BindParameters(mm.Method, rvd);
                    var ret = mm.Method.Invoke(target, pValues);
                    if (mm.Method.ReturnType == typeof(Task<IEnumerable<string>>))
                        return (mm.Name, await (Task<IEnumerable<string>>)ret);

                    return (mm.Name, (IEnumerable<string>)ret);
                }
            }

            return (null, null);
        }

        public virtual async Task<(string name, object result)> ReadAsync(T target, string path)
        {
            if (!path.StartsWith("/"))
                path = $"/{path}";

            var rvd = new RouteValueDictionary();
            foreach (var mm in _readMatchers)
            {
                rvd.Clear();
                if (mm.Matcher.TryMatch(path, rvd))
                {
                    var pValues = BindParameters(mm.Method, rvd);
                    var ret = mm.Method.Invoke(target, pValues);
                    if (mm.Method.ReturnType == typeof(Task<object>))
                        return (mm.Name, (await (Task<object>)ret));

                    return (mm.Name, ret);
                }
            }

            return (null, null);
        }

        public virtual async Task<(string name, object result)> WriteAsync(T target, string path, string payload)
        {
            if (!path.StartsWith("/"))
                path = $"/{path}";

            var rvd = new RouteValueDictionary();
            foreach (var mm in _writeMatchers)
            {
                rvd.Clear();
                if (mm.Matcher.TryMatch(path, rvd))
                {
                    var pValues = BindParameters(mm.Method, rvd, payload);
                    var ret = mm.Method.Invoke(target, pValues);
                    if (mm.Method.ReturnType == typeof(Task<object>))
                        return (mm.Name, await (Task<object>)ret);

                    return (mm.Name, ret);
                }
            }

            return (null, null);
        }

        public virtual async Task<(string name, object reserved)> DeleteAsync(T target, string path)
        {
            if (!path.StartsWith("/"))
                path = $"/{path}";

            var rvd = new RouteValueDictionary();
            foreach (var mm in _deleteMatchers)
            {
                rvd.Clear();
                if (mm.Matcher.TryMatch(path, rvd))
                {
                    var pValues = BindParameters(mm.Method, rvd);
                    var ret = mm.Method.Invoke(target, pValues);
                    if (mm.Method.ReturnType == typeof(Task))
                        await (Task)ret;

                    return (mm.Name, null);
                }
            }

            return (null, null);
        }

        private object[] BindParameters(MethodInfo method, RouteValueDictionary rvd, string payload = null)
        {
            // TODO:  we do a lot of "dynamic" discovery and binding at the point of invocation
            // we can improve this somewhat by parsing the parameters at initialization and
            // building a list of "binding" lambdas to be applied later on

            var pInfos = method.GetParameters();
            var pValues = new List<object>(pInfos.Length);
            Dictionary<string, object> bodyForm = null;
            foreach (var p in pInfos)
            {
                var attrs = p.GetCustomAttributes();
                var isReq = p.GetCustomAttribute<RequiredAttribute>() != null;
                var bound = false;
                var value = (object)null;
                foreach (var a in attrs)
                {
                    switch (a)
                    {
                        case FromRouteAttribute fra:
                            if ((bound = rvd.ContainsKey(p.Name)))
                                value = rvd[p.Name];
                            break;
                        case FromBodyAttribute fba:
                            if ((bound = !string.IsNullOrEmpty(payload)))
                                value = payload;
                            break;
                        case FromFormAttribute ffa:
                            if (bodyForm == null)
                                if (string.IsNullOrEmpty(payload))
                                    bodyForm = new Dictionary<string, object>();
                                else
                                    bodyForm = JsonConvert.DeserializeObject<
                                            Dictionary<string, object>>(payload);
                            bound = bodyForm.TryGetValue(p.Name, out value);
                            break;

                        case FromHeaderAttribute fha:
                            throw new NotImplementedException(
                                    "FromHeader parameters are not currently supported");
                    }
                    if (bound)
                        break;
                }
                if (isReq && !bound)
                    throw new ArgumentException($"missing {p.Name}");
                pValues.Add(value);
            }
            return pValues.ToArray();
        }

        public class MethodMatcher
        {
            public LocalRouteAttribute LocalRoute
            { get; set; }

            public MethodInfo Method
            { get; set; }

            public TemplateMatcher Matcher
            { get; set; }

            public string Name
            { get; set; }
        }
    }
}