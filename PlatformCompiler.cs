// Decompiled with JetBrains decompiler
// Type: hq.platform.PlatformCompiler
// Assembly: hq.platform, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null
// MVID: AD23095B-C5CD-42A2-B496-1FB560BA52AA
// Assembly location: D:\Drive\HQ.IO\src\Archive\Reconstructions\Decompile\hq.platform.1.0.1\hq.platform.dll

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;

namespace hq.platform
{
  public static class PlatformCompiler
  {
    public static Assembly GenerateAssemblyFromCode(PlatformCompilerContext context, string code, Func<string> moniker = null, params Assembly[] dependencies)
    {
      CSharpCompilation compilation;
      PlatformCompiler.BuildCompilation(context, code, out compilation, moniker, dependencies);
      return PlatformCompiler.AssemblyFromCompilation(context, (Compilation) compilation, false);
    }

    public static Stream GenerateAssemblyStreamFromCode(PlatformCompilerContext context, string code, Func<string> moniker = null, params Assembly[] dependencies)
    {
      CSharpCompilation compilation;
      PlatformCompiler.BuildCompilation(context, code, out compilation, moniker, dependencies);
      Stream assemblyStream;
      Stream symbolStream;
      PlatformCompiler.StreamsFromCompilation(context, (Compilation) compilation, out assemblyStream, out symbolStream, false);
      return assemblyStream;
    }

    private static void BuildCompilation(PlatformCompilerContext context, string code, out CSharpCompilation compilation, Func<string> moniker = null, params Assembly[] dependencies)
    {
      moniker = moniker ?? (Func<string>) (() => Guid.NewGuid().ToString("N"));
      IEnumerable<MetadataReference> metadataReferences = PlatformCompiler.GetMetadataReferences(context, dependencies);
      SyntaxTree text = CSharpSyntaxTree.ParseText(code, (CSharpParseOptions) null, "", (Encoding) null, new CancellationToken());
      string assemblyName = string.Format("{0}.dll", (object) moniker());
      compilation = CSharpCompilation.Create(assemblyName, (IEnumerable<SyntaxTree>) new SyntaxTree[1]
      {
        text
      }, metadataReferences, new CSharpCompilationOptions((OutputKind) 2, false, (string) null, (string) null, (string) null, (IEnumerable<string>) null, (OptimizationLevel) 0, false, false, (string) null, (string) null, new ImmutableArray<byte>(), new bool?(), (Platform) 0, (ReportDiagnostic) 0, 4, (IEnumerable<KeyValuePair<string, ReportDiagnostic>>) null, true, false, (XmlReferenceResolver) null, (SourceReferenceResolver) null, (MetadataReferenceResolver) null, (AssemblyIdentityComparer) null, (StrongNameProvider) null, false));
    }

    private static IEnumerable<MetadataReference> GetMetadataReferences(PlatformCompilerContext context, params Assembly[] dependencies)
    {
      HashSet<MetadataReference> metadataReferenceSet = new HashSet<MetadataReference>();
      PlatformCompiler.LoadInMemoryMetadataReferences((ISet<MetadataReference>) metadataReferenceSet, dependencies);
      PlatformCompiler.LoadPhysicalMetadataReferences(context, (ISet<MetadataReference>) metadataReferenceSet);
      return (IEnumerable<MetadataReference>) metadataReferenceSet;
    }

    private static void LoadInMemoryMetadataReferences(ISet<MetadataReference> references, params Assembly[] dependencies)
    {
      foreach (Assembly dependency in dependencies)
        PlatformCompiler.AddAssemblyMetadataReferenceUnsafe(references, dependency);
    }

    private static unsafe void AddAssemblyMetadataReferenceUnsafe(ISet<MetadataReference> references, Assembly assembly)
    {
      byte* numPtr;
      int num;
      if (!AssemblyExtensions.TryGetRawMetadata(assembly, ref numPtr, ref num))
        return;
      PortableExecutableReference reference = AssemblyMetadata.Create(ModuleMetadata.CreateFromMetadata((IntPtr) ((void*) numPtr), num)).GetReference((DocumentationProvider) null, new ImmutableArray<string>(), false, (string) null, (string) null);
      references.Add((MetadataReference) reference);
    }

    private static void LoadPhysicalMetadataReferences(PlatformCompilerContext context, ISet<MetadataReference> references)
    {
      foreach (CompilationLibrary compileLibrary in (IEnumerable<CompilationLibrary>) context.DependencyContext.CompileLibraries)
      {
        if (compileLibrary.Serviceable)
        {
          IEnumerable<string> strings = compileLibrary.ResolveReferencePaths();
          try
          {
            foreach (string path in strings)
            {
              ModuleMetadata fromStream;
              using (FileStream fileStream = File.OpenRead(path))
                fromStream = ModuleMetadata.CreateFromStream((Stream) fileStream, PEStreamOptions.PrefetchMetadata);
              AssemblyMetadata assemblyMetadata = AssemblyMetadata.Create(fromStream);
              references.Add((MetadataReference) assemblyMetadata.GetReference((DocumentationProvider) null, new ImmutableArray<string>(), false, (string) null, (string) null));
            }
          }
          catch (Exception ex)
          {
            ILogger logger = context.Logger;
            if (logger != null)
              logger.LogWarning((EventId) 500, ex, string.Format("Unable to load assemblies from library '{0}'", (object) compileLibrary.Name), Array.Empty<object>());
          }
        }
      }
    }

    private static Assembly AssemblyFromCompilation(PlatformCompilerContext context, Compilation compilation, bool emitSymbols = false)
    {
      Assembly assembly = (Assembly) null;
      using (MemoryStream memoryStream1 = new MemoryStream())
      {
        using (MemoryStream memoryStream2 = new MemoryStream())
        {
          EmitResult emitResult = compilation.Emit((Stream) memoryStream1, emitSymbols ? (Stream) memoryStream2 : (Stream) null, (Stream) null, (Stream) null, (IEnumerable<ResourceDescription>) null, (EmitOptions) null, (IMethodSymbol) null, new CancellationToken());
          ImmutableArray<Diagnostic> diagnostics = emitResult.get_Diagnostics();
          if (emitResult.get_Success())
          {
            memoryStream1.Seek(0L, SeekOrigin.Begin);
            if (emitSymbols)
            {
              memoryStream2.Seek(0L, SeekOrigin.Begin);
              assembly = context.AssemblyLoadContext.LoadFromStream((Stream) memoryStream1, (Stream) memoryStream2);
            }
            else
              assembly = context.AssemblyLoadContext.LoadFromStream((Stream) memoryStream1);
          }
          else
          {
            using (IEnumerator<Diagnostic> enumerator = diagnostics.Where<Diagnostic>((Func<Diagnostic, bool>) (d => d.get_Severity() == 2)).GetEnumerator())
            {
              while (((IEnumerator) enumerator).MoveNext())
              {
                Diagnostic current = enumerator.Current;
                ILogger logger = context.Logger;
                if (logger != null)
                  logger.LogWarning(current.GetMessage((IFormatProvider) null), Array.Empty<object>());
              }
            }
            using (IEnumerator<Diagnostic> enumerator = diagnostics.Where<Diagnostic>((Func<Diagnostic, bool>) (d => d.get_Severity() == 3)).GetEnumerator())
            {
              while (((IEnumerator) enumerator).MoveNext())
              {
                Diagnostic current = enumerator.Current;
                ILogger logger = context.Logger;
                if (logger != null)
                  logger.LogError(current.GetMessage((IFormatProvider) null), Array.Empty<object>());
              }
            }
          }
        }
      }
      return assembly;
    }

    private static void StreamsFromCompilation(PlatformCompilerContext context, Compilation compilation, out Stream assemblyStream, out Stream symbolStream, bool emitSymbols = false)
    {
      assemblyStream = (Stream) new MemoryStream();
      symbolStream = (Stream) new MemoryStream();
      EmitResult emitResult = compilation.Emit(assemblyStream, emitSymbols ? symbolStream : (Stream) null, (Stream) null, (Stream) null, (IEnumerable<ResourceDescription>) null, (EmitOptions) null, (IMethodSymbol) null, new CancellationToken());
      ImmutableArray<Diagnostic> diagnostics = emitResult.get_Diagnostics();
      if (emitResult.get_Success())
      {
        assemblyStream.Seek(0L, SeekOrigin.Begin);
        if (!emitSymbols)
          return;
        symbolStream.Seek(0L, SeekOrigin.Begin);
      }
      else
      {
        using (IEnumerator<Diagnostic> enumerator = diagnostics.Where<Diagnostic>((Func<Diagnostic, bool>) (d => d.get_Severity() == 2)).GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            Diagnostic current = enumerator.Current;
            context.Logger.LogWarning(current.GetMessage((IFormatProvider) null), Array.Empty<object>());
          }
        }
        using (IEnumerator<Diagnostic> enumerator = diagnostics.Where<Diagnostic>((Func<Diagnostic, bool>) (d => d.get_Severity() == 3)).GetEnumerator())
        {
          while (((IEnumerator) enumerator).MoveNext())
          {
            Diagnostic current = enumerator.Current;
            context.Logger.LogError(current.GetMessage((IFormatProvider) null), Array.Empty<object>());
          }
        }
      }
    }
  }
}
