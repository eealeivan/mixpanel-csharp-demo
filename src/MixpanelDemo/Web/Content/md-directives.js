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
                bool: "Bool",
                //textArray: "Text Array",
                //numberArray: "Number Array",
                //dateArray: "Date Array",
                //boolArray: "Bool Array",
            };
            $scope.types["text-array"] = "Text Array";
            $scope.types["number-array"] = "Number Array";
            $scope.types["date-array"] = "Date Array";
            $scope.types["bool-array"] = "Bool Array";

            $scope.propertyTypeChanged = function (property) {
                var isArray =
                    property.type === "text-array" || property.type === "number-array" ||
                    property.type === "date-array" || property.type === "bool-array";

                if (isArray) {
                    property.value = [null, null, null];
                } else {
                    property.value = null;
                }
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