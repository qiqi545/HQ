// Decompiled with JetBrains decompiler
// Type: hq.platform.PlatformCompilerContext
// Assembly: hq.platform, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: AD23095B-C5CD-42A2-B496-1FB560BA52AA
// Assembly location: D:\Drive\HQ.IO\src\Archive\Reconstructions\Decompile\hq.platform.1.0.1\hq.platform.dll

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
