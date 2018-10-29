// Decompiled with JetBrains decompiler
// Type: hq.platform.PlatformCompilerContext
// Assembly: hq.platform, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6EC00BB3-D92E-4DC4-8941-F29EEA84E4A3
// Assembly location: C:\Users\User\Desktop\hq.platform.dll

using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using System.Runtime.Loader;

namespace hq.platform
{
  public class PlatformCompilerContext
  {
    public readonly ILogger Logger;
    public readonly AssemblyLoadContext AssemblyLoadContext;
    public readonly DependencyContext DependencyContext;

    public PlatformCompilerContext(ILogger logger = null, DependencyContext dependencyContext = null, AssemblyLoadContext assemblyLoadContext = null)
    {
      this.Logger = logger;
      this.DependencyContext = dependencyContext ?? DependencyContext.Default;
      this.AssemblyLoadContext = assemblyLoadContext ?? AssemblyLoadContext.get_Default();
    }
  }
}
