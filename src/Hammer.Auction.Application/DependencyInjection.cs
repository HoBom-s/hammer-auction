using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.UseCases.GetAuctionItemById;
using Hammer.Auction.Application.UseCases.GetAuctionItems;
using Microsoft.Extensions.DependencyInjection;

namespace Hammer.Auction.Application;

/// <summary>
/// Registers application layer services.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    /// <summary>
    /// Adds application services to the DI container.
    /// </summary>
    /// <returns></returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IGetAuctionItemsUseCase, GetAuctionItemsUseCase>();
        services.AddScoped<IGetAuctionItemByIdUseCase, GetAuctionItemByIdUseCase>();

        return services;
    }
}
