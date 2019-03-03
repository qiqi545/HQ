// Copyright (c) Blowdart, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace Blowdart.UI.Web
{
    public class HtmlSystem : UiSystem
    {
        private static readonly ObjectPool<StringBuilder> StringBuilderPool = new DefaultObjectPool<StringBuilder>(
            new StringBuilderPooledObjectPolicy());

        private string _dom;
        private string _scripts;

        public StringBuilder Dom;
        public StringBuilder Scripts;

        public string RenderDom => _dom ?? Dom?.ToString();
        public string RenderScripts => _scripts ?? Scripts?.ToString();

        public override void Begin()
        {
            _dom = null;
            _scripts = null;
            Dom = StringBuilderPool.Get();
            Scripts = StringBuilderPool.Get();
        }

        public override void End()
        {
            _dom = RenderDom;
            _scripts = RenderScripts;

            StringBuilderPool.Return(Dom);
            StringBuilderPool.Return(Scripts);
        }

        internal string BuildString(Action<StringBuilder> action)
        {
            var sb = StringBuilderPool.Get();
            try
            {
                action(sb);
                return sb.ToString();
            }
            finally
            {
                StringBuilderPool.Return(sb);
            }
        }

        public override bool Button(Ui ui, string text)
        {
            throw new NotSupportedException("You must use a higher-order UI system, or raw DOM elements.");
        }

        public virtual string ScriptsSection()
        {
            return "<!-- SCRIPTS -->";
        }

        public virtual string StylesSection()
        {
            return "<!-- STYLES -->";
        }
    }
}