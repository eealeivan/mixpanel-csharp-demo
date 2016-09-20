var rawApp = angular.module("rawApp", ["ui.bootstrap", "hljs"]);

rawApp.controller("RawCtrl", ["$scope", "$http", "$window", function ($scope, $http, $window) {

    $scope.message = {
        type: "track",
        json: ""
    };

    $scope.sendRawMessage = function () {
        $http
            .post("/send-raw", JSON.stringify($scope.message, null, 2))
            .success(function(data) {
                $scope.result = {
                    success: data.success ? true : false
                };
            });
    };
}
]);