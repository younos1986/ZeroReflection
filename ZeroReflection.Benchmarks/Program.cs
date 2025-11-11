using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using ZeroReflection.Benchmarks;

var config = DefaultConfig.Instance
    .AddLogger(ConsoleLogger.Default)
    .AddExporter(RPlotExporter.Default)
    .AddExporter(MarkdownExporter.GitHub);

//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);

//BenchmarkRunner.Run<MappingBenchmarksCustomMapper>(config);
//BenchmarkRunner.Run<MappingBenchmarksComplexObject>();
//BenchmarkRunner.Run<MappingBenchmarksSimpleObject>(config);
BenchmarkRunner.Run<MappingBenchmarksOrderEntity>();
//BenchmarkRunner.Run<MappingBenchmarksAddressModelSimpleObject>(config);

return 1;
