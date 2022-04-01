using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.Abstraction
{
    public interface IFacade 
    {       
        Task StartJob(CancellationToken cancellationToken = default);
    }
}
