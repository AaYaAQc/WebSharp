﻿//
// WebSharpObject.cs
//

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;


namespace WebSharpJs.Script
{


    public class WebSharpObject : ScriptObject
    {

        static CachedBag cachedObjects;

        /// <summary>
        /// Cached type of the instance
        /// </summary>
        protected Type InstanceType;

        ScriptMemberBag CachedPropertyInfo
        {
            get
            {
                if (cachedPropertyInfo == null)
                {
                    var props = new ScriptMemberBag();
                    var scriptAlias = string.Empty;
                    bool createIfNotExists;
                    bool hasOwnProperty;

                    foreach (var prop in InstanceType.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                    {
                        scriptAlias = prop.Name;
                        createIfNotExists = true;
                        hasOwnProperty = false;
                        if (prop.IsDefined(typeof(ScriptableMemberAttribute), false))
                        {
                            var att = prop.GetCustomAttribute<ScriptableMemberAttribute>(false);
                            scriptAlias = (att.ScriptAlias ?? scriptAlias);
                            createIfNotExists = att.CreateIfNotExists;
                            hasOwnProperty = att.HasOwnProperty;
                        }
                        props[prop.Name] = new ScriptMemberInfo(scriptAlias, createIfNotExists, hasOwnProperty);
                    }
                    foreach (var prop in InstanceType.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                    {
                        scriptAlias = prop.Name;
                        createIfNotExists = true;
                        hasOwnProperty = false;
                        if (prop.IsDefined(typeof(ScriptableMemberAttribute), false))
                        {
                            var att = prop.GetCustomAttribute<ScriptableMemberAttribute>(false);
                            scriptAlias = (att.ScriptAlias ?? scriptAlias);
                            createIfNotExists = att.CreateIfNotExists;
                            hasOwnProperty = att.HasOwnProperty;
                        }
                        props[prop.Name] = new ScriptMemberInfo(scriptAlias, createIfNotExists, hasOwnProperty);
                    }
                    return props;
                }

                return cachedPropertyInfo;
            }
        }

        /// <summary>
        /// Cached property info used for scriptAlias lookup for Properties
        /// </summary>
        ScriptMemberBag cachedPropertyInfo;

        static WebSharpObject()
        {
            cachedObjects = new CachedBag();
        }

        protected WebSharpObject(object obj) : base()
        {
            InstanceType = GetType();
            if (cachedPropertyInfo == null)
                cachedPropertyInfo = CachedPropertyInfo;
        }

        public WebSharpObject()
        {
            InstanceType = GetType();
            if (cachedPropertyInfo == null)
                cachedPropertyInfo = CachedPropertyInfo;
        }

        class EventHandlerBag : GrabBag<WebSharpEvent>
        { }

        EventHandlerBag EventHandlers = new EventHandlerBag();

        class EventHandlerTBag : GrabBag<WebSharpEvent>
        { }

        EventHandlerTBag EventTHandlers = new EventHandlerTBag();

        public async Task<bool> AttachEvent(string eventName, EventHandler handler)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException(nameof(eventName));

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var scriptAlias = eventName;
            var ei = InstanceType.GetTypeInfo().GetEvent(eventName, BindingFlags.Instance | BindingFlags.Public);

            if (ei != null)
            {
                if (ei.IsDefined(typeof(ScriptableMemberAttribute), false))
                {
                    var att = ei.GetCustomAttribute<ScriptableMemberAttribute>(false);
                    scriptAlias = (att.ScriptAlias ?? scriptAlias);
                }
            }

            WebSharpEvent websharpEvent;
            var result = false;
            if (!EventHandlers.TryGetValue(scriptAlias, out websharpEvent))
            {

                websharpEvent = new WebSharpEvent(this, scriptAlias);
                
                if (ScriptObjectProxy != null)
                {
                    var eventCallback = new
                    {
                        handle = Handle,
                        onEvent = scriptAlias,
                        uid = websharpEvent.UID,
                        callback = websharpEvent.EventCallbackFunction
                    };
                    result = await WebSharp.Bridge.AddEventListener(eventCallback);
                }
                EventHandlers[scriptAlias] = websharpEvent;
            }
            websharpEvent.AddEventHandler(handler);
            return result;
        }

        public async Task<bool> AttachEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException(nameof(eventName));
            
            WebSharpEvent websharpEvent;

            var scriptAlias = eventName;
            var ei = InstanceType.GetTypeInfo().GetEvent(eventName, BindingFlags.Instance | BindingFlags.Public);

            if (ei == null)
                return false;

            if (ei.IsDefined(typeof(ScriptableMemberAttribute), false))
            {
                var att = ei.GetCustomAttribute<ScriptableMemberAttribute>(false);
                scriptAlias = (att.ScriptAlias ?? scriptAlias);
            }

            var result = false;

            if (!EventHandlers.TryGetValue(eventName, out websharpEvent))
            {
                websharpEvent = new WebSharpEvent(this, eventName);
                if (ScriptObjectProxy != null)
                {
                    var eventCallback = new
                    {
                        handle = Handle,
                        onEvent = scriptAlias,
                        uid = websharpEvent.UID,
                        callback = websharpEvent.EventCallbackFunction
                    };
                    result = await WebSharp.Bridge.AddEventListener(eventCallback);
                }
                EventHandlers[eventName] = websharpEvent;
            }
            return result;
        }

        public async Task<bool> DetachEvent(string eventName, EventHandler handler)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException(nameof(eventName));

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            var scriptAlias = eventName;
            var ei = InstanceType.GetTypeInfo().GetEvent(eventName, BindingFlags.Instance | BindingFlags.Public);

            if (ei != null)
            {
                if (ei.IsDefined(typeof(ScriptableMemberAttribute), false))
                {
                    var att = ei.GetCustomAttribute<ScriptableMemberAttribute>(false);
                    scriptAlias = (att.ScriptAlias ?? scriptAlias);
                }
            }

            WebSharpEvent websharpEvent;
            var result = false;
            if (!EventHandlers.TryGetValue(scriptAlias, out websharpEvent))
            {

                if (ScriptObjectProxy != null)
                {
                    var eventCallback = new
                    {
                        handle = Handle,
                        onEvent = scriptAlias,
                        uid = websharpEvent.UID,
                        callback = websharpEvent.EventCallbackFunction
                    };
                    result = await WebSharp.Bridge.RemoveEventListener(eventCallback);
                }
                EventHandlers.Remove(eventName);
            }
            websharpEvent.RemoveEventHandler(handler);
            return result;
        }


        public override async Task<bool> SetProperty(string name, object value)
        {
            ScriptMemberInfo property = null;
            if (cachedPropertyInfo.TryGetValue(name, out property))
                return await base.TrySetProperty(property.ScriptAlias, value, property.CreateIfNotExists, property.HasOwnProperty);
            else
                return await base.SetProperty(name, value);
        }

        public override Task<T> GetProperty<T>(string name)
        {
            ScriptMemberInfo property = null;
            if (cachedPropertyInfo.TryGetValue(name, out property))
                return base.GetProperty<T>(property.ScriptAlias);
            else
                return base.GetProperty<T>(name);
        }


        public override Task<T> Invoke<T>(string name, params object[] args)
        {
            var scriptAlias = name;

            var mi = InstanceType.GetTypeInfo().GetMethod(name, BindingFlags.Instance | BindingFlags.Public);

            if (mi != null)
            {
                if (mi.IsDefined(typeof(ScriptableMemberAttribute), false))
                {
                    var att = mi.GetCustomAttribute<ScriptableMemberAttribute>(false);
                    scriptAlias = (att.ScriptAlias ?? scriptAlias);
                }
            }
            return base.Invoke<T>(scriptAlias, args);
        }
    }

    internal class GrabBag<T> : Dictionary<string, T>
    { }

    internal class CachedBag : GrabBag<WeakReference>
    { }

    internal class ScriptMemberBag : GrabBag<ScriptMemberInfo>
    { }


    internal class ScriptMemberInfo
    {
        public string ScriptAlias { get; set; }
        public bool CreateIfNotExists { get; set; }
        public bool HasOwnProperty { get; set; }

        public ScriptMemberInfo(string scriptAlias, bool createIfNotExists, bool hasOwnProperty)
        {
            ScriptAlias = scriptAlias;
            CreateIfNotExists = createIfNotExists;
            HasOwnProperty = hasOwnProperty;
        }
    }
}