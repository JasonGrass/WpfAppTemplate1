using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace WpfAppTemplate1.Generators;

/*
 * 使用 IIncrementalGenerator 实现，优化 VS 调用性能
 */

[Generator]
internal class ViewDependencyInjectionGenerator2 : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 注册一个语法接收器，筛选出所有以 View 或 ViewModel 结尾的类声明
        var classDeclarations = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: IsCandidateClass, // 先通过语法筛选
                transform: GetSemanticTarget // 再通过语义筛选
            )
            .Where(symbolAndClass => symbolAndClass.Symbol != null); // 过滤掉不符合条件

        // 收集所有符合条件的类的全名
        var classFullNames = classDeclarations
            .Select((symbolAndClass, ct) => symbolAndClass.Symbol!.ToDisplayString())
            .Collect();

        // 当收集完成后，进行代码的生成
        context.RegisterSourceOutput(
            classFullNames,
            (spc, fullNames) =>
            {
                if (fullNames.IsDefault || !fullNames.Any())
                {
                    // 如果没有符合条件的类，则不生成任何代码
                    return;
                }

                var sourceBuilder = new StringBuilder(
                    @"
using Microsoft.Extensions.DependencyInjection;

public static class ViewModelDependencyInjection
{
    public static void AddViewModelServices(this IServiceCollection services)
    {
"
                );

                // 使用 HashSet 来避免重复添加
                HashSet<string> added = new HashSet<string>();

                foreach (var fullName in fullNames.Distinct())
                {
                    if (added.Add(fullName))
                    {
                        sourceBuilder.AppendLine($"        services.AddSingleton<{fullName}>();");
                    }
                }

                sourceBuilder.AppendLine("    }");
                sourceBuilder.AppendLine("}");

                // 将生成的代码添加到编译过程中
                spc.AddSource(
                    "ViewModelDependencyInjection.g.cs",
                    SourceText.From(sourceBuilder.ToString(), Encoding.UTF8)
                );
            }
        );
    }

    /// <summary>
    /// 判断一个类声明是否是潜在的候选者（名称以 View 或 ViewModel 结尾）
    /// </summary>
    private static bool IsCandidateClass(SyntaxNode node, CancellationToken _)
    {
        return node is ClassDeclarationSyntax classDecl
            && (
                classDecl.Identifier.Text.EndsWith("View")
                || classDecl.Identifier.Text.EndsWith("ViewModel")
            );
    }

    /// <summary>
    /// 获取符合条件的类的符号信息
    /// </summary>
    private static (INamedTypeSymbol? Symbol, ClassDeclarationSyntax? ClassDecl) GetSemanticTarget(
        GeneratorSyntaxContext context,
        CancellationToken ct
    )
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;

        var symbol = context.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;

        if (symbol == null)
            return (null, null);

        // 检查继承关系
        if (classDecl.Identifier.Text.EndsWith("ViewModel"))
        {
            // ViewModel 必须继承 Stylet.Screen
            if (!InheritsFrom(symbol, "Stylet.Screen"))
                return (null, null);
        }
        else if (classDecl.Identifier.Text.EndsWith("View"))
        {
            // View 必须继承 System.Windows.FrameworkElement
            if (!InheritsFrom(symbol, "System.Windows.FrameworkElement"))
                return (null, null);
        }
        else
        {
            return (null, null);
        }

        return (symbol, classDecl);
    }

    /// <summary>
    /// 判断一个符号是否继承自指定的基类
    /// </summary>
    private static bool InheritsFrom(INamedTypeSymbol typeSymbol, string baseClassFullName)
    {
        var current = typeSymbol.BaseType;
        while (current != null)
        {
            if (current.ToDisplayString() == baseClassFullName)
            {
                return true;
            }
            current = current.BaseType;
        }
        return false;
    }
}
