using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Mixpanel;
using Nancy;
using Nancy.ModelBinding;
using Newtonsoft.Json;
using Web.Models;

namespace Web
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/"] = _ => View["index.html"];
            Post["/track", true] = async (m, ct) => await HandleTrackAsync(this.Bind<Track>());
            Post["/alias", true] = async (m, ct) => await HandleAliasAsync(this.Bind<Alias>());
            Post["/people-set", true] = async (m, ct) => await HandlePeopleSetAsync(this.Bind<ModelBase>());
            Post["/people-set-once", true] = async (m, ct) => await HandlePeopleSetOnceAsync(this.Bind<ModelBase>());
            Post["/people-add", true] = async (m, ct) => await HandlePeopleAddAsync(this.Bind<ModelBase>());
            Post["/people-append", true] = async (m, ct) => await HandlePeopleAppendAsync(this.Bind<ModelBase>());
            Post["/people-union", true] = async (m, ct) => await HandlePeopleUnionAsync(this.Bind<ModelBase>());
            Post["/people-unset", true] = async (m, ct) => await HandlePeopleUnsetAsync(this.Bind<PeopleUnset>());
            Post["/people-delete", true] = async (m, ct) => await HandlePeopleDeleteAsync(this.Bind<ModelBase>());
            Post["/people-track-charge", true] = async (m, ct) => await HandlePeopleTrackChargeAsync(this.Bind<PeopleTrackCharge>());
        }

        private async Task<MessageResult> HandleTrackAsync(Track model)
        {
            return await new MessageHandler(model).HandleAsync(
                (client, properties) => client.TrackTest(model.Event, model.DistinctId, properties),
                (client, properties) => client.TrackAsync(model.Event, model.DistinctId, properties)); 
        } 
        
        private async Task<MessageResult> HandleAliasAsync(Alias model)
        {
            return await new MessageHandler(model).HandleAsync(
                (client, properties) => client.AliasTest(model.DistinctId, model.FromDistinctId),
                (client, properties) => client.AliasAsync(model.DistinctId, model.FromDistinctId));
        }

        private async Task<MessageResult> HandlePeopleSetAsync(ModelBase model)
        {
            return await new MessageHandler(model).HandleAsync(
                (client, properties) => client.PeopleSetTest(model.DistinctId, properties),
                (client, properties) => client.PeopleSetAsync(model.DistinctId, properties));
        }

        private async Task<MessageResult> HandlePeopleSetOnceAsync(ModelBase model)
        {
            return await new MessageHandler(model).HandleAsync(
                (client, properties) => client.PeopleSetOnceTest(model.DistinctId, properties),
                (client, properties) => client.PeopleSetOnceAsync(model.DistinctId, properties));
        }

        private async Task<MessageResult> HandlePeopleAddAsync(ModelBase model)
        {
            return await new MessageHandler(model).HandleAsync(
               (client, properties) => client.PeopleAddTest(model.DistinctId, properties),
               (client, properties) => client.PeopleAddAsync(model.DistinctId, properties));
        }

        private async Task<MessageResult> HandlePeopleAppendAsync(ModelBase model)
        {
            return await new MessageHandler(model).HandleAsync(
               (client, properties) => client.PeopleAppendTest(model.DistinctId, properties),
               (client, properties) => client.PeopleAppendAsync(model.DistinctId, properties));
        }

        private async Task<MessageResult> HandlePeopleUnionAsync(ModelBase model)
        {
            return await new MessageHandler(model).HandleAsync(
               (client, properties) => client.PeopleUnionTest(model.DistinctId, properties),
               (client, properties) => client.PeopleUnionAsync(model.DistinctId, properties));
        }

        private async Task<MessageResult> HandlePeopleUnsetAsync(PeopleUnset model)
        {
            var propertyNamesObj = ValueParser.ParseArray(model.PropertyNames, ValueParser.ParseText);
            IList<string> propertyNames;
            if (propertyNamesObj == null)
            {
                propertyNames = new List<string>(0);
            }
            else
            {
                propertyNames = propertyNamesObj
                    .OfType<string>()
                    .ToList();
            }

            return await new MessageHandler(model).HandleAsync(
               (client, properties) => client.PeopleUnsetTest(model.DistinctId, propertyNames),
               (client, properties) => client.PeopleUnsetAsync(model.DistinctId, propertyNames));
        }

        private async Task<MessageResult> HandlePeopleDeleteAsync(ModelBase model)
        {
            return await new MessageHandler(model).HandleAsync(
               (client, properties) => client.PeopleDeleteTest(model.DistinctId),
               (client, properties) => client.PeopleDeleteAsync(model.DistinctId));
        }  
        
        private async Task<MessageResult> HandlePeopleTrackChargeAsync(PeopleTrackCharge model)
        {
            return await new MessageHandler(model).HandleAsync(
               (client, properties) => 
                   client.PeopleTrackChargeTest(model.DistinctId, model.Amount, model.Time ?? DateTime.UtcNow),
               (client, properties) =>
                   client.PeopleTrackChargeAsync(model.DistinctId, model.Amount, model.Time ?? DateTime.UtcNow));
        }

        private class MessageHandler
        {
            private IMixpanelClient Client { get; set; }
            private IDictionary<string, object> Properties { get; set; }

            public MessageHandler(ModelBase model)
            {
                Client = GetMixpanelClient(model);
                Properties = GetPropertiesDictionary(model.Properties);
            }

            public async Task<MessageResult> HandleAsync(
                Func<IMixpanelClient, IDictionary<string, object>, MixpanelMessageTest> testFn,
                Func<IMixpanelClient, IDictionary<string, object>, Task<bool>> sendAsyncFn
                )
            {
                var testResult = testFn(Client, Properties);
                if (testResult.Exception != null)
                {
                    return await Task.FromResult(
                        new MessageResult
                        {
                            Error = testResult.Exception.Message
                        });
                }

                bool mixpanelResponse = await sendAsyncFn(Client, Properties);
                return new MessageResult
                {
                    SentJson = testResult.Json,
                    MixpanelResponse = mixpanelResponse
                };
            } 

            private IMixpanelClient GetMixpanelClient(ModelBase model)
            {
                var config = new MixpanelConfig();
                if (model.Config.UseJsonNet)
                {
                    config.SerializeJsonFn = JsonConvert.SerializeObject;
                }

                if (model.Config.UseHttpClient)
                {
                    config.HttpPostFn = (url, stringContent) =>
                    {
                        using (var client = new HttpClient())
                        {
                            HttpResponseMessage responseMessage =
                                client.PostAsync(url, new StringContent(stringContent)).Result;
                            if (!responseMessage.IsSuccessStatusCode)
                            {
                                return false;
                            }

                            string responseContent = responseMessage.Content.ReadAsStringAsync().Result;
                            return responseContent == "1";
                        }
                    };

                    config.AsyncHttpPostFn = async (url, stringContent) =>
                    {
                        using (var client = new HttpClient())
                        {
                            HttpResponseMessage responseMessage =
                                await client.PostAsync(url, new StringContent(stringContent));
                            if (!responseMessage.IsSuccessStatusCode)
                            {
                                return false;
                            }

                            string responseContent = await responseMessage.Content.ReadAsStringAsync();
                            return responseContent == "1";
                        }
                    };
                }

                var superProperties = GetPropertiesDictionary(model.SuperProperties);

                return new MixpanelClient(model.Token, config, superProperties);
            }

            private IDictionary<string, object> GetPropertiesDictionary(IEnumerable<Property> properties)
            {
                return (properties ?? Enumerable.Empty<Property>())
                    .ToDictionary(x => x.Name, ValueParser.ParsePropertyValue);
            }
        }

        private class ValueParser
        {
            public static object ParsePropertyValue(Property property)
            {
                var value = property.Value;
                switch (property.Type)
                {
                    case "text":
                        return ParseText(value);
                    case "number":
                        return ParseNumber(value);
                    case "date":
                        return ParseDateTime(value);
                    case "bool":
                        return ParseBool(value);
                    case "text-array":
                        return ParseArray(value, ParseText);
                    case "number-array":
                        return ParseArray(value, ParseNumber);
                    case "date-array":
                        return ParseArray(value, ParseDateTime);
                    case "bool-array":
                        return ParseArray(value, ParseBool);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public static object ParseText(object value)
            {
                return (value is string) ? value : null;
            }

            public static object ParseNumber(object value)
            {
                return (value is int || value is decimal) ? value : null;
            }

            public static object ParseDateTime(object value)
            {
                DateTime date;
                if (value != null && DateTime.TryParse(value as string, out date))
                {
                    return date;
                }
                return null;
            }

            public static object ParseBool(object value)
            {
                return (value is bool) ? value : null;
            }

            public static List<object> ParseArray(object value, Func<object, object> parseArrayItemFn)
            {
                if (value is IEnumerable)
                {
                    return ((IEnumerable)(value))
                        .Cast<object>()
                        .Select(parseArrayItemFn)
                        .Where(x => x != null)
                        .ToList();
                }
                return null;
            }
        }
    }
}