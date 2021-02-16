namespace Proge.Teams.Edu.Abstraction
{
    public interface ITeamMember
    {
        string AzureAdId { get; set; }
        string UserPrincipalName { get; set; }
        string Mail { get; set; }
        string Name { get; set; }
        string SecondName { get; set; }
    }
}
