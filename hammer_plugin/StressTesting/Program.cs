using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualBasic.Devices;
using HammerPluginCore.Model;
using HammerPluginBuilder;

namespace HammerStressTesting
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Hammer Plugin Infinite Stress Test ===");
            Console.WriteLine();

            Console.WriteLine("Select hammer parameters:");
            Console.WriteLine("1 - Minimal parameters (small hammer)");
            Console.WriteLine("2 - Average parameters (medium hammer)");
            Console.WriteLine("3 - Maximum parameters (large hammer)");
            Console.Write("Your choice: ");

            var choice = Console.ReadLine();
            Parameters parameters;

            switch (choice)
            {
                case "1":
                    parameters = GetMinimalParameters();
                    Console.WriteLine("Using minimal parameters");
                    break;
                case "2":
                    parameters = GetAverageParameters();
                    Console.WriteLine("Using average parameters");
                    break;
                case "3":
                    parameters = GetMaximalParameters();
                    Console.WriteLine("Using maximum parameters");
                    break;
                default:
                    Console.WriteLine("Invalid choice! Using average parameters.");
                    parameters = GetAverageParameters();
                    break;
            }

            Console.WriteLine();
            Console.WriteLine("Starting infinite stress test...");
            Console.WriteLine("Press Ctrl+C to stop");
            Console.WriteLine();

            RunInfiniteTest(parameters);
        }

        /// <summary>
        /// Выполняет нагрузочное тестирование в бесконечном цикле.
        /// </summary>
        /// <param name="parameters">Параметры молотка</param>
        private static void RunInfiniteTest(Parameters parameters)
        {
            var builder = new Builder();
            var stopWatch = new Stopwatch();
            var computerInfo = new ComputerInfo();
            Process currentProcess = Process.GetCurrentProcess();

            const double gigabyteInByte = 0.000000000931322574615478515625;
            var count = 0;
            var startTime = DateTime.Now;

            // Создаем файл лога
            var fileName = $"hammer_stress_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            using var streamWriter = new StreamWriter(fileName);

            // Записываем заголовок
            streamWriter.WriteLine($"Stress Test Started: {DateTime.Now}");
            streamWriter.WriteLine($"Parameters: H={parameters.GetParam(ParameterType.HeightH)}, " +
                                   $"L={parameters.GetParam(ParameterType.LengthL)}, " +
                                   $"D={parameters.GetParam(ParameterType.FaceDiameterD)}");
            streamWriter.WriteLine("Count\tTime\tRAM (B)");
            streamWriter.Flush();

            Console.WriteLine($"Log file: {fileName}");
            Console.WriteLine("Count\tTime\t\tRAM (B)\t");
            Console.WriteLine("------------------------------------------------------");

            try
            {
                while (true)
                {
                    count++;

                    try
                    {
                        stopWatch.Start();
                        builder.Build(parameters);
                        stopWatch.Stop();
                        builder.CloseDocument();
                    }
                    catch (Exception ex)
                    {
                        stopWatch.Stop();
                        Console.WriteLine($"\nBuild {count} failed: {ex.GetType().Name}");

                        if (ex is OutOfMemoryException)
                        {
                            Console.WriteLine("*** OUT OF MEMORY ***");
                            streamWriter.WriteLine($"\nOUT OF MEMORY at build {count}: {ex.Message}");
                            break;
                        }
                    }

                    var usedMemory = (computerInfo.TotalPhysicalMemory
                    - computerInfo.AvailablePhysicalMemory);
                    var totalPhysicalMemory = computerInfo.TotalPhysicalMemory * gigabyteInByte;
                    var elapsedTime = stopWatch.Elapsed;

                    // Запись в лог файл
                    streamWriter.WriteLine(
                        $"{count}\t{elapsedTime:hh\\:mm\\:ss\\.fff}\t{usedMemory:F3}");
                    streamWriter.Flush();

                    // Вывод в консоль каждые 10 итераций
                    if (count % 10 == 0 || count == 1)
                    {
                        var totalElapsed = DateTime.Now - startTime;
                        Console.Write($"\r{count}\t{elapsedTime:hh\\:mm\\:ss\\.fff}\t{usedMemory:F3} B");
                        Console.Write($" | Total: {totalElapsed:hh\\:mm\\:ss}");
                    }

                    stopWatch.Reset();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n\n*** CRITICAL ERROR: {ex.GetType().Name} ***");
                streamWriter.WriteLine($"\nCRITICAL ERROR: {ex.GetType().Name} - {ex.Message}");
            }
            finally
            {
                var totalElapsed = DateTime.Now - startTime;

                // Записываем итоги
                streamWriter.WriteLine($"\nTest Finished: {DateTime.Now}");
                streamWriter.WriteLine($"Total builds: {count}");
                streamWriter.WriteLine($"Total time: {totalElapsed:hh\\:mm\\:ss}");
                streamWriter.WriteLine($"Average time per build: {totalElapsed.TotalMilliseconds / count:F0} ms");
                streamWriter.WriteLine($"Total physical memory: {computerInfo.TotalPhysicalMemory * gigabyteInByte:F3} GB");

                Console.WriteLine($"\n\n=== Test Results ===");
                Console.WriteLine($"Total builds: {count}");
                Console.WriteLine($"Total time: {totalElapsed:hh\\:mm\\:ss}");
                Console.WriteLine($"Average time per build: {totalElapsed.TotalMilliseconds / count:F0} ms");
                Console.WriteLine($"Total physical memory: {computerInfo.TotalPhysicalMemory * gigabyteInByte:F3} GB");
                Console.WriteLine($"Results saved to: {fileName}");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Получает минимальные параметры молотка
        /// </summary>
        private static Parameters GetMinimalParameters()
        {
            var parameters = new Parameters();
            parameters.SetParameter(ParameterType.HeightH, 150);
            parameters.SetParameter(ParameterType.LengthL, 100);
            parameters.SetParameter(ParameterType.MiddleM, 20);
            parameters.SetParameter(ParameterType.FaceDiameterD, 20);
            parameters.SetParameter(ParameterType.FaceWidthC, 10);
            parameters.SetParameter(ParameterType.NeckWidthA, 10);
            parameters.SetParameter(ParameterType.NeckDiameterB, 15);
            parameters.HasNailPuller = false;
            return parameters;
        }

        /// <summary>
        /// Получает средние параметры молотка
        /// </summary>
        private static Parameters GetAverageParameters()
        {
            var parameters = new Parameters();
            parameters.SetParameter(ParameterType.HeightH, 540);
            parameters.SetParameter(ParameterType.LengthL, 280);
            parameters.SetParameter(ParameterType.MiddleM, 80);
            parameters.SetParameter(ParameterType.FaceDiameterD, 85);
            parameters.SetParameter(ParameterType.FaceWidthC, 40);
            parameters.SetParameter(ParameterType.NeckWidthA, 40);
            parameters.SetParameter(ParameterType.NeckDiameterB, 70);
            parameters.HasNailPuller = true;
            return parameters;
        }

        /// <summary>
        /// Получает максимальные параметры молотка
        /// </summary>
        private static Parameters GetMaximalParameters()
        {
            var parameters = new Parameters();
            parameters.SetParameter(ParameterType.HeightH, 600);
            parameters.SetParameter(ParameterType.LengthL, 300);
            parameters.SetParameter(ParameterType.MiddleM, 100);
            parameters.SetParameter(ParameterType.FaceDiameterD, 150);
            parameters.SetParameter(ParameterType.FaceWidthC, 80);
            parameters.SetParameter(ParameterType.NeckWidthA, 80);
            parameters.SetParameter(ParameterType.NeckDiameterB, 100);
            parameters.HasNailPuller = true;
            return parameters;
        }
    }
}