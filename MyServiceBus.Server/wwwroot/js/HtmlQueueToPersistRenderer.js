var HtmlQueueToPersistRenderer = /** @class */ (function () {
    function HtmlQueueToPersistRenderer() {
    }
    HtmlQueueToPersistRenderer.RenderQueueToPersistTable = function (data) {
        var result = '<h3>Messages to Persist Amount:</h3><div>';
        for (var _i = 0, data_1 = data; _i < data_1.length; _i++) {
            var i = data_1[_i];
            result += '<div>' + i.id + ' = ' + i.size + '</div>';
        }
        return result + '</div>';
    };
    return HtmlQueueToPersistRenderer;
}());
//# sourceMappingURL=HtmlQueueToPersistRenderer.js.map