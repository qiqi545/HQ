using System;
using System.Linq;
using HQ.Remix;
using HQ.Touchstone.Assertions;
using ImpromptuInterface;
using Microsoft.Extensions.Logging;

namespace HQ.Touchstone
{
    public abstract class TestBase
    {
        protected readonly ILoggerFactory defaultLoggerFactory = new LoggerFactory();

        protected static void TryInstallShouldAssertions(ITypeResolver resolver)
        {
            var assert = resolver.ResolveByExample<IAssert>().FirstOrDefault();
            if (assert != null)
            {
                var instance = Activator.CreateInstance(assert);

                Should.Assert = instance.ActLike<IAssert>();
            }
            else
            {
                throw new InvalidOperationException("No testing framework implementation was found!");
            }
        }

        protected static ActionLoggerProvider CreateLoggerProvider()
        {
            var actionLoggerProvider = new ActionLoggerProvider(message =>
            {
                var outputProvider = AmbientContext.OutputProvider;
                if (outputProvider?.IsAvailable != true)
                    return;
                outputProvider.WriteLine(message);
            });
            return actionLoggerProvider;
        }
    }
}
