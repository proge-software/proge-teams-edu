using System;
using System.Collections.Generic;
using System.Text;

namespace Proge.Teams.Edu.DAL
{
    public interface ICustomMigration
    {
        string Up();
        string Down();
    }
}
