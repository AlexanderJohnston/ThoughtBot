namespace Realization.Skill
{
    public class Person
    {
        public string Name { get; set; }
        public ulong Id { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is Person person &&
                   Id == person.Id;
        }
    }
}
