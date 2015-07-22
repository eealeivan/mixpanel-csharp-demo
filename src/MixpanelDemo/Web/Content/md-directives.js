var mdDirectivesModule = angular.module("md-directives", []);

mdDirectivesModule.directive("mdProperties", function () {
    return {
        restrict: 'E',
        replace: true,
        scope: {
            properties: "=ngModel",
            allowedTypes: "@"
        },
        controller: ["$scope", function ($scope) {
            var types = [
                { code: "text", text: "Text" },
                { code: "number", text: "Number" },
                { code: "date", text: "Date" },
                { code: "bool", text: "Bool" },
                { code: "text-array", text: "Text Array" },
                { code: "number-array", text: "Number Array" },
                { code: "date-array", text: "Date Array" },
                { code: "bool-array", text: "Bool Array" }
            ];

            if ($scope.allowedTypes) {
                var argumentTypes = $scope.allowedTypes.split(/[\s,]+/);

                for (var i = types.length - 1; i >= 0 ; i--) {
                    if (argumentTypes.indexOf(types[i].code) === -1) {
                        types.splice(i, 1);
                    }
                }
            }
            $scope.types = types;

            $scope.propertyTypeChanged = function (property) {
                property.value = getPropertyDefaultValue(property.type);
            };

            $scope.add = function () {
                if (!$scope.types || $scope.types.length <= 0) {
                    return;
                }

                if (!$scope.properties) {
                    $scope.properties = [];
                }
                var propertyType = $scope.types[0].code;
                $scope.properties.push(
                    {
                        name: null,
                        type: propertyType,
                        value: getPropertyDefaultValue(propertyType)
                    });
            };

            $scope.delete = function (index) {
                $scope.properties.splice(index, 1);
            };

            function getPropertyDefaultValue(propertyType) {
                var isArray =
                   propertyType === "text-array" || propertyType === "number-array" ||
                   propertyType === "date-array" || propertyType === "bool-array";

                if (isArray) {
                    return  [null, null, null];
                } else {
                    return null;
                }
            };
        }],
        templateUrl: 'content/properties.html'
    };
});