using Microsoft.Extensions.Options;

namespace Proge.Teams.Edu.Abstraction.Providers
{
    public interface IAnnoCorrenteProvider
    {
        int AnnoCorrente { get; }
    }

    public class AnnoCorrenteFromConfigurationProvider : IAnnoCorrenteProvider
    {
        public int AnnoCorrente { get; private set; }

        public AnnoCorrenteFromConfigurationProvider(IOptions<AnnoCorrenteConfiguration> configuration)
        {
            AnnoCorrente = configuration.Value.AnnoCorrente;
        }
    }

    public class AnnoCorrenteConfiguration
    {
        public int AnnoCorrente { get; set; }
    }
}
