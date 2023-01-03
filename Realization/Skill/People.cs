namespace Realization.Skill
{
    public class People
    {
        public List<Person> Group = new();
        public People(List<Person> group)
        {
            Group = group;
        }
        public void Add(Person person)
        {
            Group.Add(person);
        }
        public void Remove(Person person)
        {
            Group.Remove(person);
        }
    }
}
