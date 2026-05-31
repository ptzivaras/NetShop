using Eshop.Web.ViewModels;

namespace Eshop.Web.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardViewModel> GetDashboardAsync();
    }
}
