namespace BepNhaPho.ViewModels;

public sealed class AdminStatisticsViewModel
{
    public int PendingRecipes { get; init; }
    public int ApprovedRecipes { get; init; }
    public int RejectedRecipes { get; init; }
    public int TotalRecipes => PendingRecipes + ApprovedRecipes + RejectedRecipes;
    public int ActiveUsers { get; init; }
    public int LockedUsers { get; init; }
    public int TotalUsers => ActiveUsers + LockedUsers;
    public List<CategoryStatisticsViewModel> Categories { get; init; } = [];
}

public sealed class CategoryStatisticsViewModel
{
    public string Name { get; init; } = "";
    public int PendingRecipes { get; init; }
    public int ApprovedRecipes { get; init; }
    public int RejectedRecipes { get; init; }
    public int TotalRecipes => PendingRecipes + ApprovedRecipes + RejectedRecipes;
}
