var mdServicesModule = angular.module("md-services", []);

mdServicesModule.factory('localStorageService', function () {
    var isStorageSupported = function() {
        return typeof (Storage) !== "undefined";
    }

    var set = function (key, value) {
        if (isStorageSupported) {
            if (window.angular.isObject(value)) {
                value = JSON.stringify(value);
            }

            localStorage.setItem(key, value);
        }
    };

    var get = function (key, objectType) {
        if (isStorageSupported) {
            var value = localStorage.getItem(key);
            if (objectType && objectType === "object") {
                return JSON.parse(value);
            }
            return value;
        } else {
            return undefined;
        }
    };

    return {
        set: set,
        get: get
    };
});