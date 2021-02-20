var Utils = /** @class */ (function () {
    function Utils() {
    }
    Utils.getMax = function (c) {
        var result = 0;
        for (var _i = 0, c_1 = c; _i < c_1.length; _i++) {
            var i = c_1[_i];
            if (i > result)
                result = i;
        }
        return result;
    };
    Utils.findConnection = function (c, id) {
        for (var _i = 0, c_2 = c; _i < c_2.length; _i++) {
            var con = c_2[_i];
            if (con.id === id)
                return con;
        }
    };
    Utils.renderName = function (name) {
        var lines = name.split(";");
        var result = "";
        for (var _i = 0, lines_1 = lines; _i < lines_1.length; _i++) {
            var i = lines_1[_i];
            result += "<div>" + i + "</div>";
        }
        return result;
    };
    Utils.renderBytes = function (b) {
        if (b < 1024)
            return b.toString();
        if (b < this.mb)
            return (b / 1024).toFixed(3) + "Kb";
        if (b < this.gb)
            return (b / this.mb).toFixed(3) + "Mb";
        return (b / this.gb).toFixed(3) + "Gb";
    };
    Utils.queueIsEmpty = function (slices) {
        if (slices.length > 1)
            return false;
        return slices[0].from > slices[0].to;
    };
    Utils.getQueueSize = function (slices) {
        var result = 0;
        for (var _i = 0, slices_1 = slices; _i < slices_1.length; _i++) {
            var slice = slices_1[_i];
            result += slice.to - slice.from + 1;
        }
        return result;
    };
    Utils.mb = 1024 * 1024;
    Utils.gb = 1024 * 1024 * 1024;
    return Utils;
}());
//# sourceMappingURL=Utils.js.map