var indexApp = angular.module("indexApp", ["ui.bootstrap", "hljs", "md-services", "md-directives"]);

indexApp.controller("IndexCtrl", ["$scope", "$http", "localStorageService", function ($scope, $http, localStorageService) {
    // Set init model values
    $scope.model = {
        token: null,
        distinctId: null,
        track: {
            event: null,
            properties: null
        },
        alias: {
            fromDistinctId: null
        },
        peopleSet: {
            properties: null
        },
        peopleSetOnce: {
            properties: null
        },
        peopleAdd: {
            properties: null
        },
        peopleAppend: {
            properties: null
        },
        peopleUnion: {
            properties: null
        },
        peopleUnset: {
            propertyNames: [null, null, null]
        },
        peopleTrackCharge: {
            amount: null,
            time: null
        }
    };
    $scope.config = {
        useJsonNet: false,
        useHttpClient: false
    };

    $scope.messageTypes = [
        "track", "alias", "people-set", "people-set-once", "people-add", "people-append",
        "people-union", "people-unset", "people-delete", "people-track-charge"];
    $scope.activeMessageType = $scope.messageTypes[0];
    $scope.changeActiveMessageType = function (messageType) {
        $scope.activeMessageType = messageType;
    };

    $scope.send = function () {
        performMessageAction("send", function (data) {
            $scope.resultType = "send";
            $scope.result = {
                json: JSON.stringify(JSON.parse(data.data), null, 2),
                error: data.error,
                success: data.success ? true : false
            };
        });
    };

    $scope.addToQueue = function () {
        performMessageAction("queue", function (data) {

        });
        var actionData = messageActionGrid[$scope.activeMessageType];
        var model = actionData.getModelFn();
        model.actionType = "queue";
    }

    function performMessageAction(actionType, successFn) {
        var actionData = messageActionGrid[$scope.activeMessageType];
        var model = actionData.getModelFn();
        model.actionType = actionType;

        $http
            .post(actionData.url, angular.toJson(model, true))
            .success(function (data) {
                successFn(data);
            });
    };

    var messageActionGrid = {};
    var buildModel = function (properties, additinalData) {
        var model = {
            token: $scope.model.token,
            distinctId: $scope.model.distinctId,
            properties: properties,
            superProperties: $scope.model.superProperties,
            config: {
                useJsonNet: $scope.config.useJsonNet,
                useHttpClient: $scope.config.useHttpClient
            }
        };

        if (additinalData) {
            angular.extend(model, additinalData);
        }

        return model;
    }
    messageActionGrid["track"] = {
        url: "/track",
        getModelFn: function () {
            return buildModel($scope.model.track.properties, { event: $scope.model.track.event });
        }
    };
    messageActionGrid["alias"] = {
        url: "/alias",
        getModelFn: function () {
            return buildModel(null, { fromDistinctId: $scope.model.alias.fromDistinctId });
        }
    };
    messageActionGrid["people-set"] = {
        url: "/people-set",
        getModelFn: function () {
            return buildModel($scope.model.peopleSet.properties);
        }
    };
    messageActionGrid["people-set-once"] = {
        url: "/people-set-once",
        getModelFn: function () {
            return buildModel($scope.model.peopleSetOnce.properties);
        }
    };
    messageActionGrid["people-add"] = {
        url: "/people-add",
        getModelFn: function () {
            return buildModel($scope.model.peopleAdd.properties);
        }
    };
    messageActionGrid["people-append"] = {
        url: "/people-append",
        getModelFn: function () {
            return buildModel($scope.model.peopleAppend.properties);
        }
    };

    messageActionGrid["people-union"] = {
        url: "/people-union",
        getModelFn: function () {
            return buildModel($scope.model.peopleUnion.properties);
        }
    };

    messageActionGrid["people-unset"] = {
        url: "/people-unset",
        getModelFn: function () {
            return buildModel(null, $scope.model.peopleUnset);
        }
    };

    messageActionGrid["people-delete"] = {
        url: "/people-delete",
        getModelFn: function () {
            return buildModel(null, null);
        }
    };

    messageActionGrid["people-track-charge"] = {
        url: "/people-track-charge",
        getModelFn: function () {
            return buildModel(null, {
                amount: $scope.model.peopleTrackCharge.amount,
                time: $scope.model.peopleTrackCharge.time
            });
        }
    };

    $scope.autosaveManager = {
        performAutosave: function () {
            localStorageService.set("autosavedValues", {
                token: $scope.model.token || null,
                distinctId: $scope.model.distinctId || null,
                useJsonNet: $scope.config.useJsonNet || false,
                useHttpClient: $scope.config.useHttpClient || false
            });
        },
        loadAutosavedValues: function () {
            var autosavedValues = localStorageService.get("autosavedValues", "object");
            if (autosavedValues) {
                $scope.model.token = autosavedValues.token;
                $scope.model.distinctId = autosavedValues.distinctId;
                $scope.config.useJsonNet = autosavedValues.useJsonNet;
                $scope.config.useHttpClient = autosavedValues.useHttpClient;
            }
        }
    };
    $scope.autosaveManager.loadAutosavedValues();
}
]);