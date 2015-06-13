var mdDirectivesModule = angular.module("md-directives", []);

mdDirectivesModule.directive("mdProperties", function () {
    return {
        restrict: 'E',
        replace: true,
        scope: {
            properties: "=ngModel"
        },
        controller: ["$scope", function ($scope) {
            $scope.types = {
                text: "Text",
                number: "Number",
                date: "Date",
                bool: "Bool"
            };

            $scope.add = function () {
                if (!$scope.properties) {
                    $scope.properties = [];
                }

                $scope.properties.push({ name: null, type: "text", value: null });
            };

            $scope.delete = function(index) {
                $scope.properties.splice(index, 1);
            };
        }],
        templateUrl: 'content/properties.html'
    };
});