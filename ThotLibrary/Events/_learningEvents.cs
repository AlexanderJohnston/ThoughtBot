using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using ThotBot.Libray.Cognitive;
using Totem.Timeline;

namespace ThotLibrary.Events
{

    public class Phrase : Command
    {
        public readonly Guid Context;
        public readonly string Message;

        public Phrase(Guid context, string message)
        {
            Context = context;
            Message = message;
        }
    }

    public class KnownIntent : Command
    {
        public readonly Guid Context;
        public readonly string Message;
        public readonly ThotLibrary.Intention Intent;

        public KnownIntent(string message, ThotLibrary.Intention intent, Guid context)
        {
            Context = context;
            Message = message;
            Intent = intent;
        }
    }

    public class Association : Event
    {
        public readonly Guid Context;
        public readonly ThotLibrary.Intention Intent;
        public readonly string Resource;

        public Association(Guid context, ThotLibrary.Intention intent, string resource)
        {
            Context = context;
            Intent = intent;
            Resource = resource;
        }
    }

    public class Conception : Event
    {
        public readonly Guid Context;
        public readonly ThotLibrary.Intention Intent;
        public readonly string Resource;
        public readonly string Response;

        public Conception(Guid context, ThotLibrary.Intention intent, string resource, string response)
        {
            Context = context;
            Intent = intent;
            Resource = resource;
            Response = response;
        }
    }



    //public class Association : Event
    //{
    //    public readonly string Intent;
    //    public readonly string Resource;
    //}

    //public class Conception : Event
    //{
    //       public readonly string Intent;
    //    public readonly string Resource;
    //    public readonly string Response;
    //}


    //public class LearnNewCommand : Command
    //{
    //    public readonly string Intent;
    //    public LearnNewCommand(string intent)
    //    {
    //        Intent = intent;
    //    }
    //}

    //public class LearnNewPhrase : Command
    //{
    //    public readonly string ExamplePhrase;
    //    public readonly LearnedEntity[] Entities;
    //    public LearnNewPhrase(string examplePhrase, LearnedEntity[] entities)
    //    {
    //        ExamplePhrase = examplePhrase;
    //        Entities = entities;
    //    }
    //}

    //public class ConfirmLearning : Command
    //{

    //}

}
