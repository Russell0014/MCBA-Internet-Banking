using MCBA.Models;
using X.PagedList;

namespace MCBA.ViewModel;

public class MyStatementsViewModel
{
    public Account Account { get; set; }
    public IPagedList<Transaction> Transactions { get; set; }
}