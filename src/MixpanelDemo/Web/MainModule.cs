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
    public class MainModule : NancyModule
    {
        public MainModule()
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
            Post["/send", true] = async (m, ct) => await HandleSendAsync(this.Bind<List<MixpanelMessage>>());
        }

        private async Task<object> HandleTrackAsync(Track model)
        {
            try
            {
                return await new MessageHandler(model).HandleAsync(
                    (client, properties) => client.TrackTest(model.Event, model.DistinctId, properties),
                    (client, properties) => client.GetTrackMessage(model.Event, model.DistinctId, properties),
                    (client, properties) => client.TrackAsync(model.Event, model.DistinctId, properties));
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        private async Task<object> HandleAliasAsync(Alias model)
        {
            try
            {
                return await new MessageHandler(model).HandleAsync(
                    (client, properties) => client.AliasTest(model.DistinctId, model.FromDistinctId),
                    (client, properties) => client.GetAliasMessage(model.DistinctId, model.FromDistinctId),
                    (client, properties) => client.AliasAsync(model.DistinctId, model.FromDistinctId));
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        private async Task<object> HandlePeopleSetAsync(ModelBase model)
        {
            try
            {
                return await new MessageHandler(model).HandleAsync(
                    (client, properties) => client.PeopleSetTest(model.DistinctId, properties),
                    (client, properties) => client.GetPeopleSetMessage(model.DistinctId, properties),
                    (client, properties) => client.PeopleSetAsync(model.DistinctId, properties));
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        private async Task<object> HandlePeopleSetOnceAsync(ModelBase model)
        {
            try
            {
                return await new MessageHandler(model).HandleAsync(
                    (client, properties) => client.PeopleSetOnceTest(model.DistinctId, properties),
                    (client, properties) => client.GetPeopleSetOnceMessage(model.DistinctId, properties),
                    (client, properties) => client.PeopleSetOnceAsync(model.DistinctId, properties));
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        private async Task<object> HandlePeopleAddAsync(ModelBase model)
        {
            try
            {
                return await new MessageHandler(model).HandleAsync(
                    (client, properties) => client.PeopleAddTest(model.DistinctId, properties),
                    (client, properties) => client.GetPeopleAddMessage(model.DistinctId, properties),
                    (client, properties) => client.PeopleAddAsync(model.DistinctId, properties));
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        private async Task<object> HandlePeopleAppendAsync(ModelBase model)
        {
            try
            {
                return await new MessageHandler(model).HandleAsync(
                    (client, properties) => client.PeopleAppendTest(model.DistinctId, properties),
                    (client, properties) => client.GetPeopleAppendMessage(model.DistinctId, properties),
                    (client, properties) => client.PeopleAppendAsync(model.DistinctId, properties));
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        private async Task<object> HandlePeopleUnionAsync(ModelBase model)
        {
            try
            {
                return await new MessageHandler(model).HandleAsync(
                    (client, properties) => client.PeopleUnionTest(model.DistinctId, properties),
                    (client, properties) => client.GetPeopleUnionMessage(model.DistinctId, properties),
                    (client, properties) => client.PeopleUnionAsync(model.DistinctId, properties));
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        private async Task<object> HandlePeopleUnsetAsync(PeopleUnset model)
        {
            try
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
                    (client, properties) => client.GetPeopleUnsetMessage(model.DistinctId, propertyNames),
                    (client, properties) => client.PeopleUnsetAsync(model.DistinctId, propertyNames));
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        private async Task<object> HandlePeopleDeleteAsync(ModelBase model)
        {
            try
            {
                return await new MessageHandler(model).HandleAsync(
                    (client, properties) => client.PeopleDeleteTest(model.DistinctId),
                    (client, properties) => client.GetPeopleDeleteMessage(model.DistinctId),
                    (client, properties) => client.PeopleDeleteAsync(model.DistinctId));
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        private async Task<object> HandlePeopleTrackChargeAsync(PeopleTrackCharge model)
        {
            try
            {
                return await new MessageHandler(model).HandleAsync(
                    (client, properties) =>
                        client.PeopleTrackChargeTest(model.DistinctId, model.Amount, model.Time ?? DateTime.UtcNow),
                        (client, properties) =>
                        client.GetPeopleTrackChargeMessage(model.DistinctId, model.Amount, model.Time ?? DateTime.UtcNow),
                    (client, properties) =>
                        client.PeopleTrackChargeAsync(model.DistinctId, model.Amount, model.Time ?? DateTime.UtcNow));
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        private async Task<object> HandleSendAsync(List<MixpanelMessage> messages)
        {
            try
            {
                return await Task.FromResult(
                       new
                       {
                       });
            }
            catch (Exception e)
            {
                return Task.FromResult(HandleException(e)).Result;
            }
        }

        private object HandleException(Exception e)
        {
            return new
            {
                Success = false,
                Error = e.Message
            };
        }

        private class MessageHandler
        {
            private readonly ModelBase _model;
            private readonly IMixpanelClient _client;
            private readonly IDictionary<string, object> _properties;

            public MessageHandler(ModelBase model)
            {
                _model = model;
                _client = GetMixpanelClient(model);
                _properties = GetPropertiesDictionary(model.Properties);
            }

            public async Task<object> HandleAsync(
                Func<IMixpanelClient, IDictionary<string, object>, MixpanelMessageTest> testFn,
                Func<IMixpanelClient, IDictionary<string, object>, MixpanelMessage> getMessageFn,
                Func<IMixpanelClient, IDictionary<string, object>, Task<bool>> sendAsyncFn
                )
            {
                var testResult = testFn(_client, _properties);
                if (testResult.Exception != null)
                {
                    return await Task.FromResult(
                        new
                        {
                            Success = false,
                            Error = testResult.Exception.Message
                        });
                }

                switch (_model.ActionType)
                {
                    case ActionType.SendSingleMessage:
                        bool success = await sendAsyncFn(_client, _properties);
                        return new
                        {
                            Success = success,
                            SentJson = testResult.Json
                        };
                    case ActionType.GetMessage:
                        MixpanelMessage message = getMessageFn(_client, _properties);
                        return await Task.FromResult(
                            new
                            {
                                Success = true,
                                Message = message
                            });
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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