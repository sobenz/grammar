using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using TargetingTestApp.Criterion;
using TargetingTestApp.Evaluation;
using TargetingTestApp.Grammar;

namespace TargetingTestApp
{
    class Program
    {
        static void Main()
        {
            /* TODO
             * ====
             * Null value types
             * Limitation on what is applicable for Targeting Parser 
             * Base class refactor
             */

            var collection = new ServiceCollection();
            collection.AddLogging();
            collection.AddSingleton<IRuleExpressionParser, StringRuleExpressionGrammar>();
            collection.AddSingleton<IRuleExpressionParser, NumericRuleExpressionGrammar>();
            collection.AddSingleton<IRuleExpressionParser, DateTimeRuleExpressionGrammar>();
            collection.AddSingleton(TestCriteria.All);
            collection.AddSingleton<TargetingExpressionGrammar>();
            var services = collection.BuildServiceProvider();

            var parser = new TargetExpressionParser(TestCriteria.All, services.GetService<TargetingExpressionGrammar>());
            var expr = parser.GenerateExpression("NOT (BenOffers AND Age20To50)");
            var func = expr.Compile();
            var evaluator = new ConsumerEvaluator(TestConsumer.Get());
            var result = func(evaluator);
            Console.WriteLine($"Result:{result}");
        }
    }
}
