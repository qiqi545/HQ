using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using HQ.Common;
using HQ.Common.Models;
using HQ.Extensions.Caching.Configuration;
using HQ.CodeGeneration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace HQ.Extensions.Caching
{
    public class DistributedCache : ICache
    {
        private readonly IDistributedCache _cache;
        private readonly IOptions<CacheOptions> _options;
        private readonly ICacheSerializer _serializer;

        public DistributedCache(IOptions<CacheOptions> options)
        {
            _serializer = new JsonCacheSerializer();
            _cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions
            {
                CompactionPercentage = 0.05,
                ExpirationScanFrequency = TimeSpan.FromMinutes(1.0),
                SizeLimit = null,
                Clock = new LocalServerTimestampService()
            }));
            _options = options;
        }

        #region Set

        public bool Set(string key, object value)
        {
            return Try(() =>
            {
                var entry = CreateEntry();
                SetWithType(key, value, entry);
            });
        }

        public bool Set(string key, object value, DateTime absoluteExpiration)
        {
            return Try(() =>
            {
                var entry = CreateEntry(absoluteExpiration);
                SetWithType(key, value, entry);
            });
        }

        public bool Set(string key, object value, TimeSpan slidingExpiration)
        {
            return Try(() =>
            {
                var entry = CreateEntry(slidingExpiration: slidingExpiration);
                SetWithType(key, value, entry);
            });
        }

        public bool Set(string key, object value, ICacheDependency dependency)
        {
            return Try(() =>
            {
                var entry = CreateEntry();
                SetWithType(key, value, entry);
            });
        }

        public bool Set(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return Try(() =>
            {
                var entry = CreateEntry(absoluteExpiration);
                SetWithType(key, value, entry);
            });
        }

        public bool Set(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return Try(() =>
            {
                var entry = CreateEntry(slidingExpiration: slidingExpiration);
                SetWithType(key, value, entry);
            });
        }

        public bool Set<T>(string key, T value)
        {
            return Try(() =>
            {
                var entry = CreateEntry();
                SetWithType(key, value, entry);
            });
        }

        public bool Set<T>(string key, T value, DateTime absoluteExpiration)
        {
            return Try(() =>
            {
                var entry = CreateEntry(absoluteExpiration);
                SetWithType(key, value, entry);
            });
        }

        public bool Set<T>(string key, T value, TimeSpan slidingExpiration)
        {
            return Try(() =>
            {
                var entry = CreateEntry(slidingExpiration: slidingExpiration);
                SetWithType(key, value, entry);
            });
        }

        public bool Set<T>(string key, T value, ICacheDependency dependency)
        {
            return Try(() =>
            {
                var entry = CreateEntry();
                SetWithType(key, value, entry);
            });
        }

        public bool Set<T>(string key, T value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return Try(() =>
            {
                var entry = CreateEntry(absoluteExpiration);
                SetWithType(key, value, entry);
            });
        }
        
        public bool Set<T>(string key, T value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return Try(() =>
            {
                var entry = CreateEntry(slidingExpiration: slidingExpiration);
                SetWithType(key, value, entry);
            });
        }

        #endregion

        #region Add

        public bool Add(string key, object value)
        {
            if (_cache.Get(key) != null)
                return false;
            var entry = CreateEntry();
            SetWithType(key, value, entry);
            return true;
        }

        public bool Add(string key, object value, DateTime absoluteExpiration)
        {
            if (_cache.Get(key) != null)
                return false;
            var entry = CreateEntry(absoluteExpiration);
            SetWithType(key, value, entry);
            return true;
        }

        public bool Add(string key, object value, TimeSpan slidingExpiration)
        {
            if (_cache.Get(key) != null)
                return false;
            var entry = CreateEntry(slidingExpiration: slidingExpiration);
            SetWithType(key, value, entry);
            return true;
        }

        public bool Add(string key, object value, ICacheDependency dependency)
        {
            if (_cache.Get(key) != null)
                return false;
            var entry = CreateEntry();
            SetWithType(key, value, entry);
            return true;
        }

        public bool Add(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            if (_cache.Get(key) != null)
                return false;
            var entry = CreateEntry(absoluteExpiration);
            SetWithType(key, value, entry);
            return true;
        }

        public bool Add(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            if (_cache.Get(key) != null)
                return false;
            var entry = CreateEntry(slidingExpiration: slidingExpiration);
            SetWithType(key, value, entry);
            return true;
        }

        public bool Add<T>(string key, T value)
        {
            if (_cache.Get(key) != null)
                return false;
            var entry = CreateEntry();
            SetWithType(key, value, entry);
            return true;
        }

        public bool Add<T>(string key, T value, DateTime absoluteExpiration)
        {
            if (_cache.Get(key) != null)
                return false;
            var entry = CreateEntry(absoluteExpiration);
            SetWithType(key, value, entry);
            return true;
        }

        public bool Add<T>(string key, T value, TimeSpan slidingExpiration)
        {
            if (_cache.Get(key) != null)
                return false;
            var entry = CreateEntry(slidingExpiration: slidingExpiration);
            SetWithType(key, value, entry);
            return true;
        }

        public bool Add<T>(string key, T value, ICacheDependency dependency)
        {
            if (_cache.Get(key) != null)
                return false;
            var entry = CreateEntry();
            SetWithType(key, value, entry);
            return true;
        }

        public bool Add<T>(string key, T value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            if (_cache.Get(key) != null)
                return false;
            var entry = CreateEntry(absoluteExpiration);
            SetWithType(key, value, entry);
            return true;
        }
        
        public bool Add<T>(string key, T value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            if (_cache.Get(key) != null)
                return false;
            var entry = CreateEntry(slidingExpiration: slidingExpiration);
            SetWithType(key, value, entry);
            return true;
        }

        #endregion

        #region Replace

        public bool Replace(string key, object value)
        {
            return EnsureKeyExistsThen(key, () => RemoveByKeyThen(key, () => Add(key, value)));
        }

        public bool Replace(string key, object value, DateTime absoluteExpiration)
        {
            return EnsureKeyExistsThen(key, () => RemoveByKeyThen(key, () => Add(key, value, absoluteExpiration)));
        }

        public bool Replace(string key, object value, TimeSpan slidingExpiration)
        {
            return EnsureKeyExistsThen(key, () => RemoveByKeyThen(key, () => Add(key, value, slidingExpiration)));
        }

        public bool Replace(string key, object value, ICacheDependency dependency)
        {
            return EnsureKeyExistsThen(key, () => RemoveByKeyThen(key, () => Add(key, value, dependency: dependency)));
        }

        public bool Replace(string key, object value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return EnsureKeyExistsThen(key, () => RemoveByKeyThen(key, () => Add(key, value, absoluteExpiration, dependency)));
        }

        public bool Replace(string key, object value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return EnsureKeyExistsThen(key, () => RemoveByKeyThen(key, () => Add(key, value, slidingExpiration, dependency)));
        }
        
        public bool Replace<T>(string key, T value)
        {
            return EnsureKeyExistsThen(key, () => RemoveByKeyThen(key, () => Add(key, value)));
        }

        public bool Replace<T>(string key, T value, DateTime absoluteExpiration)
        {
            return EnsureKeyExistsThen(key, () => RemoveByKeyThen(key, () => Add(key, value, absoluteExpiration)));
        }

        public bool Replace<T>(string key, T value, TimeSpan slidingExpiration)
        {
            return EnsureKeyExistsThen(key, () => RemoveByKeyThen(key, () => Add(key, value, slidingExpiration)));
        }

        public bool Replace<T>(string key, T value, ICacheDependency dependency)
        {
            return EnsureKeyExistsThen(key, () => RemoveByKeyThen(key, () => Add(key, value, dependency: dependency)));
        }

        public bool Replace<T>(string key, T value, DateTime absoluteExpiration, ICacheDependency dependency)
        {
            return EnsureKeyExistsThen(key, () => RemoveByKeyThen(key, () => Add(key, value, absoluteExpiration, dependency)));
        }

        public bool Replace<T>(string key, T value, TimeSpan slidingExpiration, ICacheDependency dependency)
        {
            return EnsureKeyExistsThen(key, () => RemoveByKeyThen(key, () => Add(key, value, slidingExpiration, dependency)));
        }

        #endregion

        #region Get

        public object Get(string key, TimeSpan? timeout = null)
        {
            return GetOrAdd(key, null, timeout);
        }

        public object GetOrAdd(string key, Func<object> add = null, TimeSpan? timeout = null)
        {
            var typeBytes = _cache.Get($"{key}:type");
            if (typeBytes == null)
                return null;

            var type = Type.GetType(Encoding.UTF8.GetString(typeBytes));
            if (type == null)
                return null;

            var bytes = _cache.Get(key);
            if (bytes == null)
                return default;

            var item = DeserializeInternal(type, bytes);
            if (item != null)
                return item;

            if (add == null)
                return null;

            if (!_options.Value.ContentionTimeout.HasValue)
                return Add();

            using (TimedLock.Lock(CacheLockScope.AcquireLock<object>(key), timeout ?? _options.Value.ContentionTimeout.Value))
                return Add();

            object Add()
            {
                var itemToAdd = add();
                if (itemToAdd != null)
                    this.Add(key, itemToAdd);
                return itemToAdd;
            }
        }

        public object GetOrAdd(string key, object add = null, TimeSpan? timeout = null)
        {
            return GetOrAdd(key, () => add, timeout);
        }

        public T Get<T>(string key, TimeSpan? timeout = null)
        {
            return GetOrAdd<T>(key, null, timeout);
        }

        public T GetOrAdd<T>(string key, Func<T> add = null, TimeSpan? timeout = null)
        {
            var bytes = _cache.Get(key);
            if (bytes == null)
                return default;

            var deserialized = DeserializeInternal<T>(bytes);

            var item = deserialized is T typed ? typed : default;
            if (item != null)
                return item;

            if (add == null)
                return default;

            if (!_options.Value.ContentionTimeout.HasValue)
                return Add();

            using (TimedLock.Lock(CacheLockScope.AcquireLock<T>(key), timeout ?? _options.Value.ContentionTimeout.Value))
                return Add();

            T Add()
            {
                var itemToAdd = add();
                if (itemToAdd != null)
                {
                    this.Add(key, itemToAdd);
                }
                return itemToAdd;
            }
        }

        public T GetOrAdd<T>(string key, T add = default, TimeSpan? timeout = null)
        {
            return GetOrAdd(key, () => add, timeout);
        }

        #endregion

        public void Remove(string key)
        {
            _cache.Remove(key);
        }

        private static bool Try(Action closure)
        {
            try
            {
                closure.Invoke();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool RemoveByKeyThen(string key, Func<bool> operation)
        {
            try
            {
                _cache.Remove(key);
                return operation();
            }
            catch
            {
                return false;
            }
        }

        private bool EnsureKeyExistsThen(string key, Func<bool> operation)
        {
            try
            {
                return _cache.Get(key) != null && operation();
            }
            catch
            {
                return false;
            }
        }

        private static readonly MethodInfo SerializeMethod = typeof(ICacheSerializer).GetMethod(nameof(ICacheSerializer.Serialize));
        private static readonly ConcurrentDictionary<Type, MethodInfo> SerializeMethods = new ConcurrentDictionary<Type, MethodInfo>();
        
        private byte[] SerializeInternal(Type type, object value)
        {
            var method = SerializeMethods.GetOrAdd(type, t => SerializeMethod.MakeGenericMethod(t));
            var handler = HandlerFactory.Default.GetOrCacheHandlerFromMethod(typeof(ICacheSerializer), method, DelegateBuildStrategy.MethodInfo);
            return (byte[]) handler.Invoke(_serializer, new [] { value });
        }

        private byte[] SerializeInternal<T>(T value)
        {
            var method = SerializeMethods.GetOrAdd(typeof(T), t => SerializeMethod.MakeGenericMethod(t));
            var handler = HandlerFactory.Default.GetOrCacheHandlerFromMethod(typeof(ICacheSerializer), method, DelegateBuildStrategy.MethodInfo);
            return (byte[])handler.Invoke(_serializer, new object[] { value });
        }

        private static readonly MethodInfo DeserializeMethod = typeof(ICacheSerializer).GetMethod(nameof(ICacheSerializer.Deserialize));
        private static readonly ConcurrentDictionary<Type, MethodInfo> DeserializeMethods = new ConcurrentDictionary<Type, MethodInfo>();

        private object DeserializeInternal(Type type, byte[] bytes)
        {
            var method = DeserializeMethods.GetOrAdd(type, t => DeserializeMethod.MakeGenericMethod(t));
            var handler = HandlerFactory.Default.GetOrCacheHandlerFromMethod(typeof(ICacheSerializer), method, DelegateBuildStrategy.MethodInfo);
            return handler.Invoke(_serializer, new object[] { bytes });
        }

        private T DeserializeInternal<T>(byte[] bytes)
        {
            return _serializer.Deserialize<T>(bytes);
        }

        private static readonly ConcurrentDictionary<Type, byte[]> TypeSignatures = new ConcurrentDictionary<Type, byte[]>();

        private void SetWithType(string key, object value, DistributedCacheEntryOptions entry)
        {
            var type = value.GetType();
            var typeSignature = TypeSignatures.GetOrAdd(type, t => Encoding.UTF8.GetBytes(t.AssemblyQualifiedName ?? throw new InvalidOperationException()));
            _cache.Set($"{key}:type", typeSignature, entry);
            _cache.Set(key, SerializeInternal(type, value), entry);
        }
        
        private void SetWithType<T>(string key, T value, DistributedCacheEntryOptions entry)
        {
            var type = value.GetType();
            var typeSignature = TypeSignatures.GetOrAdd(type, t => Encoding.UTF8.GetBytes(t.AssemblyQualifiedName ?? throw new InvalidOperationException()));
            _cache.Set($"{key}:type", typeSignature, entry);
            _cache.Set(key, SerializeInternal(value), entry);
        }

        private static DistributedCacheEntryOptions CreateEntry(DateTimeOffset? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
        {
            var policy = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = absoluteExpiration,
                SlidingExpiration = slidingExpiration
            };

            return policy;
        }
    }
}
