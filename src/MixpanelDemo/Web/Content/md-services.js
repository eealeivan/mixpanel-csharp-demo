var mdServicesModule = angular.module("md-services", []);

mdServicesModule.factory('localStorageService', function () {
    var isStorageSupported = function() {
        return typeof (Storage) !== "undefined";
    }

    var set = function (key, value) {
        if (isStorageSupported) {
            localStorage.setItem(key, value);
        }
    };

    var get = function (key) {
        if (isStorageSupported) {
            return localStorage.getItem(key);
        } else {
            return undefined;
        }
    };

    return {
        set: set,
        get: get
    };
});