class HtmlTopicRenderer {
  public static renderTable(r: ITopicInfo[], c: IConnection[]): string {
    return (
      '<table class="table table-striped">' +
      "<tr>" +
      " <th>Topic</th>" +
      "<th>Size</th>" +
      "<th>Topic connections</th>" +
      "<th>Queue</th>" +
      "</tr>" +
      "<tbody>" +
      this.renderTableData(r, c) +
      "</tbody>" +
      "</table>"
    );
  }
  
  private static toDuration(v:number):string {
    if (v < 1000)
      return v + "ms";

    return (v / 1000).toFixed(3) + "sec";
  }

  public static renderTableData(r: ITopicInfo[], c: IConnection[]): string {
    let itm = "";

    for (let el of r)
      itm +=
        "<tr><td><b>" +
        el.id +
        "</b><div>Msg/sec: " +
        el.msgPerSec +
        "</div><div>Req/sec: " +
        el.requestsPerSec +
        "</div>" +
        "<hr/><div>Cached:</div><div>" +
        this.renderCachedPages(el) +
        "</div><div>" +
        this.renderGraph(el.messagesPerSecond, v => v.toFixed(0)) +
        "</div>" +
        "<td>" +
        el.size +
        "</td>" +
        "<td>" +
        this.renderTopicConnections(el, c) +
        "</td>" +
        "<td>" +
        this.renderQueues(el.consumers) +
        "</td>" +
        "</tr>";

    return itm;
  }

  private static renderTopicConnections(
    t: ITopicInfo,
    c: IConnection[]
  ): string {
    let itm = "";

    for (let pubId of t.publishers) {
      let con = Utils.findConnection(c, pubId);

      if (con) itm += "<div>" + Utils.renderName(con.name) + con.ip + "</div>";
    }

    return itm;
  }

  private static renderCachedPages(r: ITopicInfo) {
    let result = "";

    for (let info of r.cachedPages) {
      result +=
        '<span class="badge badge-secondary" style="margin-right: 5px">' +
        info +
        "</span>";
    }

    return result;
  }

  private static renderQueues(c: IConsumer[]): string {
    let itm = "";
    for (let el of c) {
      let deleteOnDisconnectBadge = el.deleteOnDisconnect
        ? '<span class="badge badge-success">auto-delete</span>'
        : '<span class="badge badge-warning">permanent</span>';

      let connectsBadge =
        el.connections == 0
          ? '<span class="badge badge-danger">Connects:' +
            el.connections +
            "</span>"
          : '<span class="badge badge-primary">Connects:' +
            el.connections +
            "</span>";

      let sizeBadge =
        el.queueSize < 1000
          ? '<span class="badge badge-success">size:' + el.queueSize + "</span>"
          : '<span class="badge badge-danger">size:' + el.queueSize + "</span>";

      let readSlicesBadge = '<span class="badge badge-success">QReady:';

      for (let c of el.readySlices) {
        readSlicesBadge += c.from + "-" + c.to + "; ";
      }

      readSlicesBadge += "</span>";

      let leasedSlicesBadge = '<span class="badge badge-warning">QLeased:';

      for (let c of el.leasedSlices) {
        leasedSlicesBadge += c.from + "-" + c.to + "; ";
      }

      leasedSlicesBadge += "</span>";

      itm += '<table style="width:100%"><tr><td style="width: 100%">'+
        "<div>"+sizeBadge+el.queueId + connectsBadge +"</div>"+
        deleteOnDisconnectBadge +
        "<span> </span>" +
        readSlicesBadge +
        "<span> </span>" +
        leasedSlicesBadge +
        "</td><td>"+this.renderGraph(el.executionDuration, v => this.toDuration(v))+"</td></tr></table>";
    }

    return itm;
  }

  private static renderGraph(c: number[], showValue: (v:number)=>string) {
    const max = Utils.getMax(c);

    const w = 50;

    let coef = max == 0 ? 0 : w / max;

    let result =
      '<svg width="240" height="' +
      w +
      '"> <rect width="240" height="' +
      w +
      '" style="fill:none;stroke-width:;stroke:black" />';

    let i = 0;
    for (let m of c) {
      let y = w - m * coef;

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
  }
}
