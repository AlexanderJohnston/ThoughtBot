using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThotLibrary;

namespace Realization.Skill
{

    
    public class Understanding
    {
        public List<Knowledge> Lessons;

    }
    public record Knowledge
    {
        public string Question;
        public string Answer;
    }
    public record LuisMessage
    {
        public List<string> Entities;
        public List<string> KeyPhrases;
        public string Sentiment;
        public string Language;
    }
    public record Intent
    {
        public List<string> Examples;
        public string Name;
    }
    public record Entity
    {
        public string Value;
    }
    public record ListEntity : Entity
    {
        public List<string> Synonyms;
    }
    public record RegexEntity : Entity
    {
        public string Expression;
    }
    public record Feature
    {
        public string Name;
        public string Type;
    }

    public class Imagination
    {
        //public Creativity GetCreative()
    }

    public record Schema
    {
        public List<Intention> Intentions;

        public Schema(List<Intention> intentions)
        {
            Intentions = intentions;
        }
    }
}
