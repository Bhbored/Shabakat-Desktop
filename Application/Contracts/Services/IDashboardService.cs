using Shabakat.Application.DTOs.Dashboard;

namespace Shabakat.Application.Contracts.Services;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(int? year = null, int? month = null);
}
