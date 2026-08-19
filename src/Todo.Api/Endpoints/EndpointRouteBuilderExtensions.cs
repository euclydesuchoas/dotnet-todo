using System.Reflection;

namespace Todo.Api.Endpoints;

public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Descobre e mapeia, em uma única passada no startup, todos os grupos
    /// (<see cref="IEndpointGroup"/>) e endpoints (<see cref="IEndpoint{TGroup}"/>) do assembly.
    /// A ordem de mapeamento é determinística (nome completo do tipo) para que o documento
    /// OpenAPI e a resolução de rotas ambíguas não dependam da ordem de scan do assembly.
    /// </summary>
    public static IEndpointRouteBuilder MapEndpoints(this IEndpointRouteBuilder app, Assembly? assembly = null)
    {
        assembly ??= typeof(EndpointRouteBuilderExtensions).Assembly;

        var types = assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false })
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        var groupBuilders = new Dictionary<Type, IEndpointRouteBuilder>();

        foreach (var groupType in types.Where(type => type.IsAssignableTo(typeof(IEndpointGroup))))
        {
            MapGroupRecursive(groupType, app, groupBuilders, resolving: []);
        }

        foreach (var endpointType in types.Where(type => type.IsAssignableTo(typeof(IEndpoint))))
        {
            var groupType = GetTypeArgument(endpointType, typeof(IEndpoint<>))
                ?? throw new InvalidOperationException(
                    $"'{endpointType.Name}' implementa IEndpoint sem declarar um grupo. " +
                    $"Use IEndpoint<TGroup> para indicar a qual grupo o endpoint pertence.");

            if (!groupBuilders.TryGetValue(groupType, out var group))
            {
                throw new InvalidOperationException(
                    $"'{endpointType.Name}' aponta para o grupo '{groupType.Name}', que não foi encontrado. " +
                    $"O grupo precisa ser uma classe concreta deste assembly implementando IEndpointGroup.");
            }

            Create<IEndpoint>(endpointType).MapEndpoint(group);
        }

        return app;
    }

    private static IEndpointRouteBuilder MapGroupRecursive(
        Type groupType,
        IEndpointRouteBuilder root,
        Dictionary<Type, IEndpointRouteBuilder> groupBuilders,
        HashSet<Type> resolving)
    {
        if (groupBuilders.TryGetValue(groupType, out var alreadyMapped))
        {
            return alreadyMapped;
        }

        if (!resolving.Add(groupType))
        {
            throw new InvalidOperationException(
                $"Ciclo na hierarquia de grupos de endpoints envolvendo '{groupType.Name}'.");
        }

        var parentType = GetTypeArgument(groupType, typeof(IEndpointGroup<>));

        // Sem argumento de tipo, o grupo é raiz e pendura direto no app.
        var parent = parentType is null
            ? root
            : MapGroupRecursive(parentType, root, groupBuilders, resolving);

        var builder = Create<IEndpointGroup>(groupType).MapGroup(parent);

        resolving.Remove(groupType);
        groupBuilders[groupType] = builder;

        return builder;
    }

    private static Type? GetTypeArgument(Type type, Type openInterface) =>
        type.GetInterfaces()
            .FirstOrDefault(candidate => candidate.IsGenericType
                && candidate.GetGenericTypeDefinition() == openInterface)
            ?.GetGenericArguments()[0];

    private static T Create<T>(Type type)
    {
        if (type is { IsAbstract: true } or { IsInterface: true } || type.GetConstructor(Type.EmptyTypes) is null)
        {
            throw new InvalidOperationException(
                $"'{type.Name}' precisa ser uma classe concreta com construtor público sem parâmetros. " +
                $"Dependências devem ser recebidas nos parâmetros do delegate da rota, e não no construtor.");
        }

        return (T)Activator.CreateInstance(type)!;
    }
}
