using System;
using System.Threading;

namespace APMOkLib
{
    /// <summary>
    /// Вспомогательный класс, создаёт Мьютексы для инсталлятора
    /// </summary>
    public static class AppMutex
    {
        public class ExceptionMutextAlreadyExists : Exception
        {
            public ExceptionMutextAlreadyExists() : base("Mutex already exists")
            { }
            public ExceptionMutextAlreadyExists(string sMsg) : base(sMsg)
            { }
        }

        public static Mutex SetMutext(string MutexName)
        {
            if (!CheckMutex(MutexName))
                return null;

            Mutex mutex = new(false, MutexName);
            if (!mutex.WaitOne(0))
            {
                mutex.Dispose();
                throw new ExceptionMutextAlreadyExists();
            }
            return mutex;
        }

        public static bool CheckMutex(string MutexName)
        {
            if (Mutex.TryOpenExisting(MutexName, out Mutex mutex))
            {
                mutex.Dispose();
                return false;
            }
            return true;
        }
    }
}
