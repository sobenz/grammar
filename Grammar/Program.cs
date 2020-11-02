using Microsoft.Extensions.DependencyInjection;
using System;
using TargetingTestApp.Evaluation;
using TargetingTestApp.Grammar;
using TargetingTestApp.Parser;
using Serialize.Linq.Serializers;
using System.Linq.Expressions;
using Serialize.Linq.Factories;
using System.IO.Compression;
using System.IO;
using System.Text;

namespace TargetingTestApp
{
    class Program
    {
        static void Main()
        {
            var collection = new ServiceCollection();
            collection.AddLogging();
            collection.AddSingleton<IRuleExpressionParser, StringRuleExpressionGrammar>();
            collection.AddSingleton<IRuleExpressionParser, NumericRuleExpressionGrammar>();
            collection.AddSingleton<IRuleExpressionParser, DateTimeRuleExpressionGrammar>();
            collection.AddSingleton<IRuleExpressionParser, ConsumerEventExpressionGrammar>();
            collection.AddSingleton(TestCriteria.All);
            collection.AddSingleton<TargetingExpressionGrammar>();
            var services = collection.BuildServiceProvider();

            var parser = new TargetExpressionParser(services.GetService<TargetingExpressionGrammar>());
            string expression = "VegetarianTestSegment AND 2RedeptionsLast31DaysNZ";
            Console.WriteLine($"Evaluating expression: {expression}");
            Expression<Func<ITargetEvaluator, bool>> expr = parser.GenerateExpression(expression);

            var settings = new FactorySettings { UseRelaxedTypeNames = true };
            var binarySerializer = new JsonSerializer();
            var expressionSerializer = new ExpressionSerializer(binarySerializer, settings);
            var data = expressionSerializer.SerializeText(expr);
            using (var str = new MemoryStream())
            using (var gzip = new GZipStream(str, CompressionLevel.Optimal))
            {
                var srcstr = new MemoryStream(Encoding.UTF8.GetBytes(data));
                srcstr.CopyTo(gzip);
                gzip.Flush();
                var dat2 = str.ToArray();
                Console.WriteLine($"Expression Data Size: {dat2.Length}");
            }
            
            Expression<Func<ITargetEvaluator, bool>> expr2 = (Expression<Func<ITargetEvaluator, bool>>)expressionSerializer.DeserializeText(data);

            var func = expr2.Compile();
            var evaluator = new ConsumerEvaluator(TestConsumer.Get());
            var result = func(evaluator);
            Console.WriteLine($"Result:{result}");
        }
    }
}
