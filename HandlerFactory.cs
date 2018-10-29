﻿// Decompiled with JetBrains decompiler
// Type: hq.platform.HandlerFactory
// Assembly: hq.platform, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6EC00BB3-D92E-4DC4-8941-F29EEA84E4A3
// Assembly location: C:\Users\User\Desktop\hq.platform.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace hq.platform
{
  public class HandlerFactory
  {
    private readonly PlatformCompilerContext _context;
    private readonly Assembly[] _defaultDependencies;
    private const string NoCodeHandler = "\r\nnamespace hq\r\n{ \r\n    public class Main\r\n    { \r\n        public static string Execute()\r\n        { \r\n            return \"Hello, World!\";\r\n        }\r\n    }\r\n}";

    public HandlerFactory(PlatformCompilerContext context, params Assembly[] defaultDependencies)
    {
      this._context = context;
      this._defaultDependencies = defaultDependencies ?? new Assembly[0];
    }

    public MethodInfo BuildHandler(HandlerInfo info, params Assembly[] dependencies)
    {
      string code = info.Code ?? "\r\nnamespace hq\r\n{ \r\n    public class Main\r\n    { \r\n        public static string Execute()\r\n        { \r\n            return \"Hello, World!\";\r\n        }\r\n    }\r\n}";
      string str = info.Namespace ?? "hq";
      string name1 = info.Entrypoint ?? str + ".Main";
      string name2 = info.Function ?? "Execute";
      Assembly[] array = ((IEnumerable<Assembly>) this._defaultDependencies).Union<Assembly>((IEnumerable<Assembly>) dependencies).Distinct<Assembly>().ToArray<Assembly>();
      Assembly assemblyFromCode = PlatformCompiler.GenerateAssemblyFromCode(this._context, code, (Func<string>) null, array);
      Type type = (object) assemblyFromCode != null ? assemblyFromCode.GetType(name1) : (Type) null;
      if ((object) type == null)
        return (MethodInfo) null;
      return TypeExtensions.GetMethod(type, name2, BindingFlags.Static | BindingFlags.Public);
    }
  }
}
