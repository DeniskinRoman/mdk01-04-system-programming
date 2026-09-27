using System.Diagnostics;

namespace CounterSync;

internal static class Program
{
    private const int ThreadCount = 8;
    private const int IncrementsPerThread = 1_000_000;
    private static readonly long Expected = (long)ThreadCount * IncrementsPerThread;

    private static void Main()
    {
        Console.WriteLine("=== Демонстрация синхронизации потоков (lock) ===");
        Console.WriteLine($"Потоков:              {ThreadCount}");
        Console.WriteLine($"Инкрементов на поток: {IncrementsPerThread:N0}");
        Console.WriteLine($"Ожидаемое значение:   {Expected:N0}");
        Console.WriteLine(new string('-', 60));

        long withoutLock = RunWithoutLock();
        Console.WriteLine($"[Без lock] Результат: {withoutLock,15:N0}  " +
                          $"{(withoutLock == Expected ? "OK" : "ПОТЕРЯ ДАННЫХ!")}");

        Console.WriteLine();

        long withLock = RunWithLock();
        Console.WriteLine($"[С lock]   Результат: {withLock,15:N0}  " +
                          $"{(withLock == Expected ? "OK" : "ПОТЕРЯ ДАННЫХ!")}");

        Console.WriteLine(new string('-', 60));
        Console.WriteLine("Вывод: без lock часть инкрементов теряется из-за гонки,");
        Console.WriteLine("      с lock счётчик всегда равен ожидаемому значению.");
    }

    private static long RunWithoutLock()
    {
        long counter = 0;
        var threads = new Thread[ThreadCount];

        for (int i = 0; i < ThreadCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < IncrementsPerThread; j++)
                {
                    counter++;
                }
            });
        }

        var sw = Stopwatch.StartNew();
        foreach (var t in threads) t.Start();
        foreach (var t in threads) t.Join();
        sw.Stop();

        Console.WriteLine($"Время без lock: {sw.ElapsedMilliseconds} мс");
        return counter;
    }

    private static long RunWithLock()
    {
        long counter = 0;
        var locker = new object();
        var threads = new Thread[ThreadCount];

        for (int i = 0; i < ThreadCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int j = 0; j < IncrementsPerThread; j++)
                {
                    lock (locker)
                    {
                        counter++;
                    }
                }
            });
        }

        var sw = Stopwatch.StartNew();
        foreach (var t in threads) t.Start();
        foreach (var t in threads) t.Join();
        sw.Stop();

        Console.WriteLine($"Время с lock:   {sw.ElapsedMilliseconds} мс");
        return counter;
    }
}