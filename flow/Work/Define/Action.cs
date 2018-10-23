namespace Mchnry.Flow.Work.Define
{
    public struct Action
    {
        public Action(string id, string description)
        {
            this.Id = id;
            this.Description = description;
        }

        public string Id { get; set; }
        public string Description { get; set; }
    }
}
