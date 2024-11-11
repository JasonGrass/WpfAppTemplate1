using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace WpfAppTemplate1.Generators;

[Generator]
public class ViewDependencyInjectionGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        // System.Diagnostics.Debugger.Launch();

        // 获取所有语法树
        var compilation = context.Compilation;
        var syntaxTrees = context.Compilation.SyntaxTrees;

        // 查找目标类型（ViewModel和View）
        var clsNodeList = syntaxTrees
            .SelectMany(tree => tree.GetRoot().DescendantNodes())
            .OfType<ClassDeclarationSyntax>()
            .Where(cls =>
                cls.Identifier.Text.EndsWith("ViewModel") || cls.Identifier.Text.EndsWith("View")
            )
            .Select(cls => new
            {
                ClassDeclaration = cls,
                ModelSymbol = compilation.GetSemanticModel(cls.SyntaxTree).GetDeclaredSymbol(cls),
            })
            .ToList();

        // 生成注册代码
        var sourceBuilder = new StringBuilder(
            @"
using Microsoft.Extensions.DependencyInjection;

public static class ViewModelDependencyInjection
{
    public static void AddViewModelServices(this IServiceCollection services)
    {
"
        );

        HashSet<string> added = new HashSet<string>();

        foreach (var clsNode in clsNodeList)
        {
            if (clsNode.ModelSymbol == null)
            {
                continue;
            }

            // var namespaceName = type.ModelSymbol.ContainingNamespace.ToDisplayString();
            var fullName = clsNode.ModelSymbol.ToDisplayString(); // 包含命名空间的全称

            if (!added.Add(fullName))
            {
                // 避免因为 partial class 造成的重复添加
                continue;
            }

            // ViewModel 必须继承 Stylet.Screen
            if (
                clsNode.ClassDeclaration.Identifier.Text.EndsWith("ViewModel")
                && InheritsFrom(clsNode.ModelSymbol, "Stylet.Screen")
            )
            {
                sourceBuilder.AppendLine($"        services.AddSingleton<{fullName}>();");
            }
            // View 必须继承 System.Windows.FrameworkElement
            else if (
                clsNode.ClassDeclaration.Identifier.Text.EndsWith("View")
                && InheritsFrom(clsNode.ModelSymbol, "System.Windows.FrameworkElement")
            )
            {
                sourceBuilder.AppendLine($"        services.AddSingleton<{fullName}>();");
            }
        }

        sourceBuilder.AppendLine("    }");
        sourceBuilder.AppendLine("}");

        var code = sourceBuilder.ToString();

        // 添加生成的代码到编译过程
        context.AddSource(
            "ViewModelDependencyInjection.g.cs",
            SourceText.From(code, Encoding.UTF8)
        );
    }

    private bool InheritsFrom(INamedTypeSymbol typeSymbol, string baseClassName)
    {
        while (typeSymbol.BaseType != null)
        {
            if (typeSymbol.BaseType.ToDisplayString() == baseClassName)
            {
                return true;
            }
            typeSymbol = typeSymbol.BaseType;
        }
        return false;
    }
}
