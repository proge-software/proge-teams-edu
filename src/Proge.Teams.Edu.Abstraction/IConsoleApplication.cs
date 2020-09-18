using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Proge.Teams.Edu.Abstraction
{
    public interface IConsoleApplication
    {
        Task Run(string[] args);
    }
}
