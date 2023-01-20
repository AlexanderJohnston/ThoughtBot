namespace Realization.Behavior
{

    


    /// <summary>
    /// The fields of a trait store the behavior of an agent at a high level.
    /// </summary>
    /// 
    public class Trait 
    {
        public string Name { get; set; }
        public List<Field> Fields { get; set; }
    }

    /// <summary>
    /// The <see cref="Agreement"/> is a <see cref="Behavior.Claim"/> with a known decision.
    /// </summary>

    public class Agreement 
    {
        public Claim Claim { get; set; }
        public string Decision { get; set; }
    }

    /// <summary>
    /// The <see cref="Resonance"/> is the <see cref="Behavior.Agreements"/> with their associated <see cref="Behavior.Knowledge"/> making up the Trait.
    /// </summary>
    public class Resonance 
    {
        public List<Accord> Accords { get; set; }
    }

    /// <summary>
    /// The <see cref="Field"/> is the <see cref="Behavior.Resonance"/> of some <see cref="Trait"/> at a certain harmonic level.
    /// </summary>
    public class Field 
    {
        public Resonance Resonance { get; set; }
        public int Level { get; set; }
    }

    /// <summary>
    /// The <see cref="Accord"/> is the <see cref="Behavior.Agreement"/> that abides to some <see cref="Behavior.Knowledge"/>
    /// </summary>
    public class Accord 
    {
        public Agreement Agreement { get; set; }
        public Knowledge Knowledge { get; set; }
    }

    /// <summary>
    /// The <see cref="Claim"/> is an <see cref="Behavior.Template"/> coupled to an expectation of outcome.
    /// </summary>
    public class Claim 
    {
        public Template Template { get; set; }
        public string Expectation { get; set; }
    }

    /// <summary>
    /// The reificaiton of knowledge into form through the aggregation of <see cref="IExpression"/> through <see cref="Trait"/>.
    /// </summary>
    public class Realization 
    {
        public string Concept { get; set; }
        public List<Trait> Traits { get; set; }
    }

    /// <summary>
    /// Any given <see cref="Template"/> that is known through memories.
    /// </summary>
    public class Knowledge 
    {
        public List<Guid> Memories { get; set; }
    }
    
    /// <summary>
    /// The <see cref="Template"/> is a taught prompt that is used to generate a behavior.
    /// </summary>
    public class Template
    {
        public IExpression[] Behavior { get; set; }
        public string Name { get; set; }

    }

    // Converges a Template by calling the format on each IExpression.
    public class BehaviorConvergence
    {
        public string Converge(Template template, string input)
        {
            var output = input;
            foreach (var expression in template.Behavior)
            {
                // order of operation is important here
                output = expression.ExpressFormat(output);
            }
            return output;
        }
    }
        
}
