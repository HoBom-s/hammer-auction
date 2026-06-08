using System.Diagnostics.CodeAnalysis;
using Hammer.Auction.Application.Notifications;
using Hammer.Auction.Application.Ports;
using Hammer.Auction.Application.UseCases.DeleteSearchHistory;
using Hammer.Auction.Application.UseCases.GetAuctionItemById;
using Hammer.Auction.Application.UseCases.GetAuctionItems;
using Hammer.Auction.Application.UseCases.GetCalendarSchedules;
using Hammer.Auction.Application.UseCases.GetCodeInfoById;
using Hammer.Auction.Application.UseCases.GetCodeInfos;
using Hammer.Auction.Application.UseCases.GetDashboardSummary;
using Hammer.Auction.Application.UseCases.GetInstitutionAuctionItemById;
using Hammer.Auction.Application.UseCases.GetInstitutionAuctionItems;
using Hammer.Auction.Application.UseCases.GetNewsById;
using Hammer.Auction.Application.UseCases.GetNewsList;
using Hammer.Auction.Application.UseCases.GetNotifications;
using Hammer.Auction.Application.UseCases.GetNotificationSettings;
using Hammer.Auction.Application.UseCases.GetPopularSearchTerms;
using Hammer.Auction.Application.UseCases.GetRandomQuiz;
using Hammer.Auction.Application.UseCases.GetRecentNews;
using Hammer.Auction.Application.UseCases.GetRecentSearchTerms;
using Hammer.Auction.Application.UseCases.GetUnreadNotificationCount;
using Hammer.Auction.Application.UseCases.ReadAllNotifications;
using Hammer.Auction.Application.UseCases.ReadNotification;
using Hammer.Auction.Application.UseCases.SearchAuctions;
using Hammer.Auction.Application.UseCases.SearchNews;
using Hammer.Auction.Application.UseCases.SubmitQuizAttempt;
using Hammer.Auction.Application.UseCases.SubmitQuizAttempts;
using Hammer.Auction.Application.UseCases.UpdateNotificationSettings;
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
        services.AddScoped<ISubmitQuizAttemptUseCase, SubmitQuizAttemptUseCase>();
        services.AddScoped<ISubmitQuizAttemptsUseCase, SubmitQuizAttemptsUseCase>();
        services.AddScoped<INotificationSender, NotificationSender>();
        services.AddScoped<IGetCalendarSchedulesUseCase, GetCalendarSchedulesUseCase>();
        services.AddScoped<ISearchAuctionsUseCase, SearchAuctionsUseCase>();
        services.AddScoped<IGetPopularSearchTermsUseCase, GetPopularSearchTermsUseCase>();
        services.AddScoped<IGetRecentSearchTermsUseCase, GetRecentSearchTermsUseCase>();
        services.AddScoped<IDeleteSearchHistoryUseCase, DeleteSearchHistoryUseCase>();
        services.AddScoped<IGetNotificationsUseCase, GetNotificationsUseCase>();
        services.AddScoped<IGetUnreadNotificationCountUseCase, GetUnreadNotificationCountUseCase>();
        services.AddScoped<IReadNotificationUseCase, ReadNotificationUseCase>();
        services.AddScoped<IReadAllNotificationsUseCase, ReadAllNotificationsUseCase>();
        services.AddScoped<IGetNotificationSettingsUseCase, GetNotificationSettingsUseCase>();
        services.AddScoped<IUpdateNotificationSettingsUseCase, UpdateNotificationSettingsUseCase>();
        services.AddScoped<IGetRecentNewsUseCase, GetRecentNewsUseCase>();
        services.AddScoped<IGetNewsByIdUseCase, GetNewsByIdUseCase>();
        services.AddScoped<IGetNewsListUseCase, GetNewsListUseCase>();
        services.AddScoped<ISearchNewsUseCase, SearchNewsUseCase>();
        return services;
    }
}
