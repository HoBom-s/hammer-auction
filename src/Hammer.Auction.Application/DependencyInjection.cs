using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.UseCases.GetAuctionItemById;
using Hammer.Auction.Application.UseCases.GetAuctionItems;
using Hammer.Auction.Application.UseCases.GetCodeInfoById;
using Hammer.Auction.Application.UseCases.GetCodeInfos;
using Hammer.Auction.Application.UseCases.GetInstitutionAuctionItemById;
using Hammer.Auction.Application.UseCases.GetInstitutionAuctionItems;
using Hammer.Auction.Application.UseCases.GetRealEstateTrades;
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
        services.AddScoped<IGetInstitutionAuctionItemsUseCase, GetInstitutionAuctionItemsUseCase>();
        services.AddScoped<IGetInstitutionAuctionItemByIdUseCase, GetInstitutionAuctionItemByIdUseCase>();
        services.AddScoped<IGetCodeInfosUseCase, GetCodeInfosUseCase>();
        services.AddScoped<IGetCodeInfoByIdUseCase, GetCodeInfoByIdUseCase>();
        services.AddScoped<IGetRealEstateTradesUseCase, GetRealEstateTradesUseCase>();

        return services;
    }
}
