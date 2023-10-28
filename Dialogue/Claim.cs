namespace Dialogue
{
    public class Claim
    {
        public Claim(Template template, Expression[] expressions)
        {
            Template = template;
            Expressions = expressions;
        }

        public Template? Template { get; set; }
        public Expression[] Expressions { get; set; } = Array.Empty<Expression>();

        
        public string Write() => string.Format(Template?.Format ?? "", Expressions.Select(e => e.Phrase).ToArray());

        /// <summary>
        /// For each {#} in the template, replace with an expression as the argument
        /// </summary>
        /// <returns></returns>
        public string WriteSafe()
        {
            // Count params in template
            var count = Template?.Format.Count(c => c == '{') ?? 0;

            // Count expressions available
            var available = Expressions.Length;

            //  populate list of params with expressions
            // If expressions less than template params, use empty string
            var list = new List<string>();
            for (int i = 0; i < count; i++)
            {
                if (i < available)
                {
                    list.Add(Expressions[i].Phrase ?? "");
                }
                else
                {
                    list.Add("");
                }
            }
            // Format the expressions array into the template now
            return string.Format(Template?.Format ?? "", list.ToArray());
        }
    }
}