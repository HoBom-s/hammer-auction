using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Notifications;
using Hammer.Auction.Application.Ports;
using Hammer.Auction.Application.UseCases.CreateQuiz;
using Hammer.Auction.Application.UseCases.DeleteQuiz;
using Hammer.Auction.Application.UseCases.GetAuctionItemById;
using Hammer.Auction.Application.UseCases.GetAuctionItems;
using Hammer.Auction.Application.UseCases.GetCodeInfoById;
using Hammer.Auction.Application.UseCases.GetCodeInfos;
using Hammer.Auction.Application.UseCases.GetDashboardSummary;
using Hammer.Auction.Application.UseCases.GetInstitutionAuctionItemById;
using Hammer.Auction.Application.UseCases.GetInstitutionAuctionItems;
using Hammer.Auction.Application.UseCases.GetQuizzes;
using Hammer.Auction.Application.UseCases.GetRandomQuiz;
using Hammer.Auction.Application.UseCases.SubmitQuizAttempt;
using Hammer.Auction.Application.UseCases.UpdateQuiz;
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
        services.AddScoped<IGetDashboardSummaryUseCase, GetDashboardSummaryUseCase>();
        services.AddScoped<IGetRandomQuizUseCase, GetRandomQuizUseCase>();
        services.AddScoped<ICreateQuizUseCase, CreateQuizUseCase>();
        services.AddScoped<IUpdateQuizUseCase, UpdateQuizUseCase>();
        services.AddScoped<IDeleteQuizUseCase, DeleteQuizUseCase>();
        services.AddScoped<IGetQuizzesUseCase, GetQuizzesUseCase>();
        services.AddScoped<ISubmitQuizAttemptUseCase, SubmitQuizAttemptUseCase>();
        services.AddScoped<INotificationSender, NotificationSender>();
        return services;
    }
}
