using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Company.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreferCancellationTokenOverloadAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "CT0001";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        title: "Use CancellationToken overload",
        messageFormat: "Call the overload of '{0}' that accepts a CancellationToken",
        category: "Reliability",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        var symbol = context.SemanticModel
            .GetSymbolInfo(invocation, context.CancellationToken)
            .Symbol as IMethodSymbol;

        if (symbol == null)
            return;

        if (symbol.ContainingType?.ToDisplayString() == "System.Threading.Tasks.Task"
            && symbol.Name == "Run")
        {
            return; // 忽略 Task.Run
        }

        var methodDecl = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (methodDecl != null)
        {

            var containingMethod = context.SemanticModel
                .GetDeclaredSymbol(methodDecl, context.CancellationToken);

            if (containingMethod != null)
            {
                if (SymbolEqualityComparer.Default.Equals(
                        containingMethod.ContainingAssembly,
                        symbol.ContainingAssembly))
                {
                    return;
                }
            }

            if (containingMethod != null &&
          SymbolEqualityComparer.Default.Equals(
              containingMethod.ContainingType,
              symbol.ContainingType))
            {
                if (containingMethod.Name == symbol.Name)
                    return;
                // 当前方法本身有 CancellationToken 参数时，仍然强制要求使用 token 重载
                if (!containingMethod.Parameters.Any(p => IsCancellationToken(p.Type)))
                {
                    return;
                }
            }
        }

        // 已经包含 CancellationToken 参数的调用，不管
        if (symbol.Parameters.Any(p => IsCancellationToken(p.Type)))
            return;

        // 在同一个类型中查找可用的 token 重载
        var candidates = symbol.ContainingType
            .GetMembers(symbol.Name)
            .OfType<IMethodSymbol>();

        foreach (var candidate in candidates)
        {
            if (!IsBetterTokenOverload(symbol, candidate))
                continue;

            var diagnostic = Diagnostic.Create(
                Rule,
                invocation.GetLocation(),
                symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));

            context.ReportDiagnostic(diagnostic);
            return;
        }
    }

    private static bool IsBetterTokenOverload(IMethodSymbol used, IMethodSymbol candidate)
    {
        if (SymbolEqualityComparer.Default.Equals(used, candidate))
            return false;

        // 必须多一个参数
        if (candidate.Parameters.Length != used.Parameters.Length + 1)
            return false;

        // 最后一个参数必须是 CancellationToken
        if (!IsCancellationToken(candidate.Parameters.Last().Type))
            return false;

        // 前面的参数必须完全一致
        for (int i = 0; i < used.Parameters.Length; i++)
        {
            if (!SymbolEqualityComparer.Default.Equals(
                    used.Parameters[i].Type,
                    candidate.Parameters[i].Type))
                return false;
        }

        return true;
    }

    private static bool IsCancellationToken(ITypeSymbol type)
    {
        return type is INamedTypeSymbol nts
            && nts.Name == "CancellationToken"
            && nts.ContainingNamespace.ToDisplayString() == "System.Threading";
    }
}
