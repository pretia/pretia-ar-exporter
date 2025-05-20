namespace PretiaEditor
{
    [System.Serializable]
    public class Team
    {
        public string id;
        public string name;
    }
    
    [System.Serializable]
    public class TeamList
    {
        public Team[] teams;
    }

}