﻿var indexApp = angular.module("indexApp", ["ui.bootstrap", "hljs", "md-services", "md-directives"]);

indexApp.controller("IndexCtrl", ["$scope", "$http", "localStorageService", function ($scope, $http, localStorageService) {
    $scope.model = {};
    $scope.config = {};
    $scope.messageTypes = [
        "track", "alias", "people-set", "people-set-once", "people-add", "people-append",
        "people-union"];
    $scope.activeMessageType = $scope.messageTypes[0];
    $scope.changeActiveMessageType = function (messageType) {
        $scope.activeMessageType = messageType;
    };

    $scope.send = function () {
        var actionData = messageActionGrid[$scope.activeMessageType];
        $http
            .post(actionData.url, angular.toJson(actionData.getModelFn(), true))
            .success(function (data) {
                $scope.result = {
                    json: JSON.stringify(JSON.parse(data.sentJson), null, 2),
                    error: data.error,
                    mixpanelResponse:
                        data.mixpanelResponse
                            ? (data.mixpanelResponse === true ? "1" : "0")
                            : undefined
                };
            });
    };

    $scope.isResultSuccess = function () {
        return $scope.result && !$scope.result.error;
    };

    $scope.isResultError = function () {
        return $scope.result && $scope.result.error;
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