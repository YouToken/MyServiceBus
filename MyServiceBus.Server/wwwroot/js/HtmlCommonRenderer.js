var HtmlCommonRenderer = /** @class */ (function () {
    function HtmlCommonRenderer() {
    }
    HtmlCommonRenderer.renderGraph = function (c, showValue) {
        var max = Utils.getMax(c);
        var w = 50;
        var coef = max == 0 ? 0 : w / max;
        var result = '<svg width="240" height="' +
            w +
            '"> <rect width="240" height="' +
            w +
            '" style="fill:none;stroke-width:;stroke:black" />';
        var i = 0;
        for (var _i = 0, c_1 = c; _i < c_1.length; _i++) {
            var m = c_1[_i];
            var y = w - m * coef;
            result +=
                '<line x1="' +
                    i +
                    '" y1="' +
                    w +
                    '" x2="' +
                    i +
                    '" y2="' +
                    y +
                    '" style="stroke:lightblue;stroke-width:2" />';
            i += 2;
        }
        return result + '<text x="0" y="15" fill="red">' + showValue(max) + "</text></svg>";
    };
    HtmlCommonRenderer.RenderQueueSlices = function (queueSlices) {
        var result = "";
        for (var _i = 0, queueSlices_1 = queueSlices; _i < queueSlices_1.length; _i++) {
            var c = queueSlices_1[_i];
            result += c.from + "-" + c.to + "; ";
        }
        return result;
    };
    return HtmlCommonRenderer;
}());
//# sourceMappingURL=HtmlCommonRenderer.js.map