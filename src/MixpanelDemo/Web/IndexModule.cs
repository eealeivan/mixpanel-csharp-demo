using System;
using System.Collections.Generic;
using System.Linq;
using Mixpanel;
using Nancy;
using Nancy.ModelBinding;
using Web.Models;

namespace Web
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/"] = _ => View["index.html"];
            Post["/track"] = _ => HandleTrack(this.Bind<Track>());
            Post["/alias"] = _ => HandleAlias(this.Bind<Alias>());
            Post["/people-set"] = _ => HandlePeopleSet(this.Bind<ModelBase>());
            Post["/people-set-once"] = _ => HandlePeopleSetOnce(this.Bind<ModelBase>());
            Post["/people-add"] = _ => HandlePeopleAdd(this.Bind<ModelBase>());
            Post["/people-append"] = _ => HandlePeopleAppend(this.Bind<ModelBase>());
            Post["/people-union"] = _ => HandlePeopleUnion(this.Bind<ModelBase>());
            Post["/people-unset"] = _ => HandlePeopleUnset(this.Bind<PeopleUnset>());
            Post["/people-delete"] = _ => HandlePeopleDelete(this.Bind<ModelBase>());
            Post["/people-track-charge"] = _ => HandlePeopleTrackCharge(this.Bind<PeopleTrackCharge>());
        }

        private MessageResult HandleTrack(Track model)
        {
            return new MessageHandler(model).Handle(
                (client, properties) => client.TrackTest(model.Event, model.DistinctId, properties),
                (client, properties) => client.Track(model.Event, model.DistinctId, properties));
        } 
        
        private MessageResult HandleAlias(Alias model)
        {
            return new MessageHandler(model).Handle(
                (client, properties) => client.AliasTest(model.DistinctId, model.FromDistinctId),
                (client, properties) => client.Alias(model.DistinctId, model.FromDistinctId));
        }

        private MessageResult HandlePeopleSet(ModelBase model)
        {
            return new MessageHandler(model).Handle(
                (client, properties) => client.PeopleSetTest(model.DistinctId, properties),
                (client, properties) => client.PeopleSet(model.DistinctId, properties));
        }

        private MessageResult HandlePeopleSetOnce(ModelBase model)
        {
            return new MessageHandler(model).Handle(
                (client, properties) => client.PeopleSetOnceTest(model.DistinctId, properties),
                (client, properties) => client.PeopleSetOnce(model.DistinctId, properties));
        }

        private MessageResult HandlePeopleAdd(ModelBase model)
        {
            return new MessageHandler(model).Handle(
               (client, properties) => client.PeopleAddTest(model.DistinctId, properties),
               (client, properties) => client.PeopleAdd(model.DistinctId, properties));
        }

        private MessageResult HandlePeopleAppend(ModelBase model)
        {
            return new MessageHandler(model).Handle(
               (client, properties) => client.PeopleAppendTest(model.DistinctId, properties),
               (client, properties) => client.PeopleAppend(model.DistinctId, properties));
        }

        private MessageResult HandlePeopleUnion(ModelBase model)
        {
            return new MessageHandler(model).Handle(
               (client, properties) => client.PeopleUnionTest(model.DistinctId, properties),
               (client, properties) => client.PeopleUnion(model.DistinctId, properties));
        }

        private MessageResult HandlePeopleUnset(PeopleUnset model)
        {
            return new MessageHandler(model).Handle(
               (client, properties) => client.PeopleUnsetTest(model.DistinctId, model.PropertyNames),
               (client, properties) => client.PeopleUnion(model.DistinctId, model.PropertyNames));
        }

        private MessageResult HandlePeopleDelete(ModelBase model)
        {
            return new MessageHandler(model).Handle(
               (client, properties) => client.PeopleDeleteTest(model.DistinctId),
               (client, properties) => client.PeopleDelete(model.DistinctId));
        }  
        
        private MessageResult HandlePeopleTrackCharge(PeopleTrackCharge model)
        {
            return new MessageHandler(model).Handle(
               (client, properties) => 
                   client.PeopleTrackChargeTest(model.DistinctId, model.Amount, model.Time ?? DateTime.UtcNow),
               (client, properties) =>
                   client.PeopleTrackCharge(model.DistinctId, model.Amount, model.Time ?? DateTime.UtcNow));
        }

        private class MessageHandler
        {
            public IMixpanelClient Client { get; set; }
            public ModelBase Model { get; set; }
            public IDictionary<string, object> Properties { get; set; }

            public MessageHandler(ModelBase model)
            {
                Client = GetMixpanelClient(model);
                Model = model;
                Properties = GetPropertiesDictionary(model.Properties);
            }

            public MessageResult Handle(
                Func<IMixpanelClient, IDictionary<string, object>, MixpanelMessageTest> testFn,
                Func<IMixpanelClient, IDictionary<string, object>, bool> sendFn)
            {
                var testResult = testFn(Client, Properties);
                if (testResult.Exception != null)
                {
                    return new MessageResult
                    {
                        Error = testResult.Exception.Message
                    };
                }

                var mixpanelResponse = sendFn(Client, Properties);
                return new MessageResult
                {
                    SentJson = testResult.Json,
                    MixpanelResponse = mixpanelResponse
                };
            }

            private IMixpanelClient GetMixpanelClient(ModelBase model)
            {
                var superProperties = GetPropertiesDictionary(model.SuperProperties);
                return new MixpanelClient(model.Token, superProperties: superProperties);
            }

            private IDictionary<string, object> GetPropertiesDictionary(IEnumerable<Property> properties)
            {
                return (properties ?? Enumerable.Empty<Property>())
                    .ToDictionary(x => x.Name, ParsePropertyValue);
            }

            private object ParsePropertyValue(Property property)
            {
                switch (property.Type)
                {
                    case "date":
                        DateTime date;
                        if (DateTime.TryParse(property.Value as string, out date))
                        {
                            return date;
                        }
                        return property.Value;
                    case "text":
                    case "number":
                    case "bool":
                        return property.Value;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}