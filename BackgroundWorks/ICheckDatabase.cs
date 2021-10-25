using System.Threading;
using System.Threading.Tasks;

namespace RestService.BackgroundWorks
{
    public interface ICheckDatabase
    {
        Task DoWork(CancellationToken cancellationToken);
    }
}