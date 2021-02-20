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

    return (v / 1000).toFixed(3) + "ms";
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
          HtmlCommonRenderer.renderGraph(el.messagesPerSecond, v => v.toFixed(0)) +
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

      let readSlicesBadge = '<span class="badge badge-success">QReady:'+HtmlCommonRenderer.RenderQueueSlices(el.readySlices)+'</span>';

      let leasedAmount = '<span class="badge badge-warning">Leased:'+el.leasedAmount+'</span>';

      itm += '<table style="width:100%"><tr><td style="width: 100%">'+
        "<div>"+el.queueId+"<span> </span>" +sizeBadge + "<span> </span>" +deleteOnDisconnectBadge +"</div>"+
         connectsBadge +
        "<span> </span>" +
        readSlicesBadge +
        "<span> </span>" +
        leasedAmount +
        "</td><td>"+HtmlCommonRenderer.renderGraph(el.executionDuration, v => this.toDuration(v))+"</td></tr></table>";
    }

    return itm;
  }


}
