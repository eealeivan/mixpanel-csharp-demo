using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Mixpanel;
using Nancy;
using Nancy.ModelBinding;
using Newtonsoft.Json;
using NLog;
using Web.Models;

namespace Web
{
    public class MainModule : NancyModule
    {
        private static readonly Logger MixpanelLogger = LogManager.GetLogger("Mixpanel");

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
            Post["/send", true] = async (m, ct) =>
            {
                // There is a strange behavior when all IDictinary<string, object> entries are
                // created with camel cased key. Case is important in our case, so for now we just
                // use JSON.NET to deserialize model.
                Request.Body.Seek(0L, SeekOrigin.Begin);
                using (var reader = new StreamReader(Request.Body))
                {
                    var model = JsonConvert.DeserializeObject<Send>(
                        await reader.ReadToEndAsync(), new JsonNetDictionaryConverter());
                    return await HandleSendAsync(model);
                }
            };
            Get["/raw"] = _ => View["raw.html"];
            Post["/send-raw", true] = async (m, ct) => await HandleSendRawAsync(this.Bind<RawMessage>());
        }

        private async Task<object> HandleTrackAsync(Track model)
        {
            try
            {
                return await new MessageHandler(model).HandleSendSingleOrGetAsync(
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
                return await new MessageHandler(model).HandleSendSingleOrGetAsync(
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
                return await new MessageHandler(model).HandleSendSingleOrGetAsync(
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
                return await new MessageHandler(model).HandleSendSingleOrGetAsync(
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
                return await new MessageHandler(model).HandleSendSingleOrGetAsync(
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
                return await new MessageHandler(model).HandleSendSingleOrGetAsync(
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
                return await new MessageHandler(model).HandleSendSingleOrGetAsync(
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

                return await new MessageHandler(model).HandleSendSingleOrGetAsync(
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
                return await new MessageHandler(model).HandleSendSingleOrGetAsync(
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
                return await new MessageHandler(model).HandleSendSingleOrGetAsync(
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

        private async Task<object> HandleSendAsync(Send model)
        {
            try
            {
                return await new MessageHandler(model).HandleSendBatchAsync();
            }
            catch (Exception e)
            {
                return HandleException(e);
            }
        }

        private async Task<object> HandleSendRawAsync(RawMessage rawMessage)
        {
            MixpanelMessageEndpoint endpoint;
            switch (rawMessage.Type)
            {
                case "track":
                    endpoint = MixpanelMessageEndpoint.Track;
                    break;
                case "engage":
                    endpoint = MixpanelMessageEndpoint.Engage;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var mixpanelClient = new MixpanelClient();

            return await Task.FromResult(
                new
                {
                    Success = mixpanelClient.SendJson(endpoint, rawMessage.Json)
                });
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

            public async Task<object> HandleSendSingleOrGetAsync(
                Func<IMixpanelClient, IDictionary<string, object>, MixpanelMessageTest> testFn,
                Func<IMixpanelClient, IDictionary<string, object>, MixpanelMessage> getMessageFn,
                Func<IMixpanelClient, IDictionary<string, object>, Task<bool>> sendAsyncFn
                )
            {
                switch (_model.ActionType)
                {
                    case ActionType.SendSingleMessage:
                        return await HandleSendSingleAsync(testFn, sendAsyncFn);

                    case ActionType.GetMessage:
                        return await HandleGetMessageAsync(testFn, getMessageFn);

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            public async Task<object> HandleSendSingleAsync(
                Func<IMixpanelClient, IDictionary<string, object>, MixpanelMessageTest> testFn, 
                Func<IMixpanelClient, IDictionary<string, object>, Task<bool>> sendAsyncFn)
            {
                var testMessage = testFn(_client, _properties);
                if (testMessage.Exception != null)
                {
                    return await Task.FromResult(
                        new
                        {
                            Success = false,
                            Error = testMessage.Exception.Message
                        });
                }

                bool success = await sendAsyncFn(_client, _properties);
                if (!success)
                {
                    return await Task.FromResult(
                        new
                        {
                            Success = false,
                            Error = "An error occurs while sending a message."
                        });
                }

                return await Task.FromResult(
                        new
                        {
                            Success = true,
                            SentJson = testMessage.Json
                        });
            }

            public async Task<object> HandleGetMessageAsync(
                Func<IMixpanelClient, IDictionary<string, object>, MixpanelMessageTest> testFn,
                Func<IMixpanelClient, IDictionary<string, object>, MixpanelMessage> getMessageFn)
            {
                var testMessage = testFn(_client, _properties);
                if (testMessage.Exception != null)
                {
                    return await Task.FromResult(
                        new
                        {
                            Success = false,
                            Error = testMessage.Exception.Message
                        });
                }

                MixpanelMessage message = getMessageFn(_client, _properties);
                if (message != null)
                {
                    return await Task.FromResult(
                        new
                        {
                            Success = true,
                            Message = message
                        });
                }
                return await Task.FromResult(
                    new
                    {
                        Success = false,
                        Error = "An error occurs while generating the message."
                    });
            }

            public async Task<object> HandleSendBatchAsync()
            {
                IList<MixpanelMessage> messages = ((Send)_model).Messages;

                IReadOnlyCollection<MixpanelBatchMessageTest> testBatchMessages = 
                    _client.SendTest(messages);
                var sendResult = await _client.SendAsync(messages);

                var sentTestBatchMessages = new List<MixpanelBatchMessageTest>();
                if (sendResult.SentBatches != null)
                {
                    foreach (var sentBatch in sendResult.SentBatches)
                    {
                        var sentTestBatchMessage = testBatchMessages
                            .Single(testBatchMessage => MixpanelDictionaryUtils.DictionaryCollectionsEqual(
                                sentBatch.Select(x => x.Data).ToList(),
                                testBatchMessage.Data));

                        sentTestBatchMessages.Add(sentTestBatchMessage);
                    }
                }

                var failedTestBatchMessages = new List<MixpanelBatchMessageTest>();
                if (sendResult.FailedBatches != null)
                {
                    foreach (var sentBatch in sendResult.FailedBatches)
                    {
                        var sentTestBatchMessage = testBatchMessages
                            .Single(testBatchMessage => MixpanelDictionaryUtils.DictionaryCollectionsEqual(
                                sentBatch.Select(x => x.Data).ToList(),
                                testBatchMessage.Data));

                        failedTestBatchMessages.Add(sentTestBatchMessage);
                    }
                }

                return new
                {
                    sendResult.Success,
                    SentBatches = sentTestBatchMessages
                        .Select(sentTestBatchMessage => new
                        {
                            sentTestBatchMessage.Json
                        }),
                    FailedBatches = failedTestBatchMessages
                        .Select(failedTestBatchMessage =>
                        {
                            var json = failedTestBatchMessage.Json ??
                                       JsonConvert.SerializeObject(failedTestBatchMessage.Data);
                            return new
                            {
                                Json = json,
                                Error = failedTestBatchMessage.Exception != null
                                    ? failedTestBatchMessage.Exception.Message
                                    : "An error occurs while sending the batch message."
                            };
                        })
                };
            }


            private IMixpanelClient GetMixpanelClient(ModelBase model)
            {
                var config = new MixpanelConfig();
                config.ErrorLogFn = (message, exception) => MixpanelLogger.Error(exception, message);

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

                switch (model.ActionType)
                {
                    case ActionType.SendSingleMessage:
                    case ActionType.GetMessage:
                        return new MixpanelClient(model.Token, config, superProperties);
                    case ActionType.SendBatchMessage:
                        return new MixpanelClient(config, superProperties);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
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