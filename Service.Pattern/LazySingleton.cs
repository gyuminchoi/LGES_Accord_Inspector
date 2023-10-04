using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Service.Pattern
{
    public class LazySingleton<T> where T : class
    {
        private static readonly Lazy<T> LazyInstance =
            new Lazy<T>(() => Activator.CreateInstance(typeof(T), true) as T,
                LazyThreadSafetyMode.ExecutionAndPublication);

        public static T Instance => LazyInstance.Value;
    }

    // Multi Thread에서 Safe 함.
    // 처음부터 메모리를 점유하지 않고 인스턴스가 필요한 시점에 인스턴스를 만들 수 있다.
    public class LazySingleton<T, TTO> where T : class, TTO
    {
        private static readonly Lazy<TTO> LazyInstance =
            new Lazy<TTO>(() => Activator.CreateInstance(typeof(T), true) as T,
                LazyThreadSafetyMode.ExecutionAndPublication);

        public static TTO Instance => LazyInstance.Value;
    }
}
