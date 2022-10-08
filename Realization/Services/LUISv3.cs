using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Realization.Services
{
    /*
    GET Get LUIS application cultures list
GET Get LUIS application domains list
GET Get LUIS application tokenizer versions for culture
GET Get LUIS application usage scenarios list
GET Get LUIS azure accounts assigned to the application
GET Get LUIS prebuilt domains for culture list
GET Get LUIS prebuilt domains list
GET Get personal assistant applications
GET Get publish settings
GET Get the asynchronously loaded query logs
POST Import application
POST Move app to another LUIS authoring Azure resource
GET PackagePublishedApplicationAsGzip
GET PackageTrainedApplicationAsGZip
POST Publish application
DELETE Removes an assigned LUIS azure accounts from an application
PUT Rename application
POST Start downloading application query logs asynchronously
PUT Update application settings
PUT Update publish settings 
    */
    public interface LuisApps
    {
        public Task AddApplication();
        public Task AddPrebuiltApp();
        public Task AddAzureSubscription();
        public Task RemoveSubscription();

        public Task DeleteApp();
        public Task GetAppLogs();
        public Task GetAppLogsAsync();
        public Task PrepareAppLogsAsync();
        public Task GetAppSettings();
        public Task GetEndpoints();
        public Task GetCultures();
        public Task GetDomains();
        public Task GetCulture();
        public Task GetUsageScenarios();
        public Task GetAccounts();
        public Task GetPrebuiltCultures();
        public Task GetPrebuiltDomains();
        public Task GetAssistant();
        public Task GetPublishSettings();
        public Task ImportApp();
        public Task MoveApp();
        public Task PublishApp();
        public Task RenameApp();
        public Task UpdateAppSettings();
        public Task UpdatePublishSettings();
    }
    public interface LuisDispatch
    {

    }
    public class LUISv3
    {
    }
}
