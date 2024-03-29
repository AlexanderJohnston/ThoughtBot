﻿using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Emit;
using System.Web;
using Memory;
using Memory.Intent;

namespace Realization.Skill
{
    // Used for Microsoft CLU but this is not implemented in the current build.
    public class PlasticIntentions
    {
        private string _uriTemplate = @"https://westus.api.cognitive.microsoft.com/luis/api/v2.0/apps/{0}/versions/{1}/examples";

        private readonly HttpClient _client = new HttpClient();

        public List<Intention> Intentions = new List<Intention>();

        public string BuildUriTemplate(string appId, string versionId) => string.Format(_uriTemplate, appId, versionId);

        public List<Intention> DownloadIntentions()
        {
            var uriBuilder = CognitiveServicesUri();
            SetClientHeaders();

            var getJson = _client.GetStringAsync(uriBuilder.Uri);
            string result;
            var intentionJson = getJson.ContinueWith(t => result = t.Result).Result;
            var response = JsonConvert.DeserializeObject<TrainedIntentions[]>(intentionJson);

            var createdIntentions = new Dictionary<string, Intention>();
            foreach (var intent in response.Select(x => x).ToList())
            {
                var name = intent.IntentLabel;
                if (createdIntentions.ContainsKey(name))
                    continue;
                var newIntent = CreateIntention(name);
                newIntent.Name = name;
                newIntent.Threshold = 0.8f;
                createdIntentions.Add(name, newIntent);
            }
            return createdIntentions.Values.ToList();
        }

        private void SetClientHeaders()
        {
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private UriBuilder CognitiveServicesUri()
        {
            var uriTemplate = BuildUriTemplate("2a916803-0dc2-46b8-8caa-acef8df45eb4", "0.1");
            var builder = new UriBuilder(uriTemplate);
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["subscription-key"] = "YOUR_CLU_SUBSCRIPTION_KEY";
            query["skip"] = "0";
            query["take"] = "100";

            builder.Query = query.ToString();
            return builder;
        }

        private Intention CreateIntention(string intentName)
        {
            var ab = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("PlasticAssembly"), AssemblyBuilderAccess.RunAndCollect);
            var mb = ab.DefineDynamicModule("Intent");
            var name = new AssemblyName(intentName).FullName;
            var tb = mb.DefineType(name,
              TypeAttributes.Public |
              TypeAttributes.Class,
              null,
              new[] { typeof(Intention) }
              );
            CreateProperty(tb, "Name", typeof(string));
            CreateProperty(tb, "Threshold", typeof(float));
            Type type = tb.CreateType();
            return (Intention)Activator.CreateInstance(type);
        }


        private void CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            var fieldBuilder = typeBuilder.DefineField($"_{propertyName}", propertyType, FieldAttributes.Private);
            MethodAttributes getSetAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;

            MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod($"get_{propertyName}", getSetAttributes, propertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr = typeBuilder.DefineMethod($"set_{propertyName}", getSetAttributes, null, new Type[] { propertyType });
            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);
            setIl.Emit(OpCodes.Ret);

            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }
    }
}
